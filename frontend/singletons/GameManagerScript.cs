using System.Collections.Generic;
using Camera;
using Godot;
using Newtonsoft.Json;
using Robots;
using shared;
using Stations;
using Trains;
using UI;

namespace Singletons;

public partial class GameManagerScript : Node
{
    public static GameManagerScript Instance { get; set; }
    public SimulationStateData SimulationState { get; set; }
    public SimulationSettingsData SimulationSettings { get; set; }
    public List<StationScript> Stations { get; set; } = [];
    public List<TrainScript> Trains { get; set; } = [];
    public List<RobotScript> Robots { get; set; } = [];

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");
    private PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://assets/robot/Robot.tscn");

    private List<LineData> lines = [];

    public override void _Ready()
    {
        Instance = this;
        SimulationSettings = new();
        SimulationState = new();
    }

    public static void SetSimulationConfiguration(SimulationSettingsData simulationSettingsData)
    {
        WebSocketMessage settingsMessage = new(204, MessageType.SETTINGS, JsonConvert.SerializeObject(simulationSettingsData));
        SessionManager.Instance.Request(settingsMessage);
    }

    public void StartSimulation()
    {
        Reset(false);
        SessionManager.Instance.Request(200, MessageType.STARTSIMULATION);
    }

    public void StopSimulation()
    {
        SessionManager.Instance.Request(205, MessageType.STOPSIMULATION);
    }

    public static void PauseSimulation(bool isPaused)
    {
        if (isPaused)
        {
            SessionManager.Instance.Request(203, MessageType.CONTINUESTIMULATION);
            return;
        }
        SessionManager.Instance.Request(202, MessageType.PAUSESIMULATION);
    }

    public void SpawnStations(List<StationData> stations)
    {
        foreach (StationData stationData in stations)
        {
            StationScript station = stationScene.Instantiate<StationScript>();
            GetTree().CurrentScene.AddChild(station);
            station.Initialize(stationData);
            Stations.Add(station);
        }

        GetTree().CurrentScene.GetNode<LoadingStationsMenuScript>("HUD/LoadingStationsMenu").UpdateStationsOption(stations);
        GetTree().CurrentScene.GetNode<ChargingStationsMenuScript>("HUD/ChargingStationsMenu").UpdateStationsOption(stations);
        SessionManager.Instance.Request(102, MessageType.TRAINLINES);
    }

    public void DrawLinePaths(List<LineData> lineDatas)
    {
        lines = lineDatas;
        Dictionary<(string, string), List<(string, string)>> segments = [];
        foreach (LineData lineData in lineDatas)
        {
            for (int i = 0; i < lineData.Stations.Count - 1; i++)
            {
                string from = lineData.Stations[i];
                string to = lineData.Stations[i + 1];
                (string, string) key = from.CompareTo(to) < 0 ? (from, to) : (to, from);
                if (!segments.ContainsKey(key))
                    segments[key] = [];
                if (!segments[key].Contains((lineData.LineName, lineData.LineColor)))
                    segments[key].Add((lineData.LineName, lineData.LineColor));
            }
        }

        foreach (var entry in segments)
        {
            Path3D path = new();
            Curve3D curve = new();
            path.Curve = curve;
            GetTree().CurrentScene.AddChild(path);

            StationScript from = Stations.Find(x => x.Data.StationId == entry.Key.Item1);
            StationScript to = Stations.Find(x => x.Data.StationId == entry.Key.Item2);

            int lineIndex = 0;
            foreach (var line in entry.Value)
            {
                SurfaceTool st = new();
                st.Begin(Mesh.PrimitiveType.Triangles);

                Vector3 start = from.GlobalPosition;
                Vector3 end = to.GlobalPosition;
                Vector3 dir = (end - start).Normalized();
                Vector3 right = dir.Cross(Vector3.Up).Normalized();

                float centerOffset = (entry.Value.Count - 1) * 11f * 0.5f;
                Vector3 adjustedOffset = (lineIndex * 11f - centerOffset) * right;
                start += adjustedOffset + lineIndex * 0.01f * Vector3.Up;
                end += adjustedOffset + lineIndex * 0.01f * Vector3.Up;

                Vector3 side = right * (10f / 2.0f);
                Vector3 a = start + side;
                Vector3 b = start - side;
                Vector3 c = end + side;
                Vector3 d = end - side;
                
                st.AddVertex(a);
                st.AddVertex(b);
                st.AddVertex(c);
                st.AddVertex(c);
                st.AddVertex(b);
                st.AddVertex(d);

                Mesh mesh = st.Commit();
                MeshInstance3D meshInstance = new();
                meshInstance.Mesh = mesh;
                meshInstance.MaterialOverride = new StandardMaterial3D
                {
                    AlbedoColor = new Color(line.Item2, 1.0f),
                    ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                    DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always,
                    CullMode = BaseMaterial3D.CullModeEnum.Disabled
                };
                path.AddChild(meshInstance);

                lineIndex++;
            }
        }
    }

    public void SpawnTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = trainScene.Instantiate<TrainScript>();
            GetTree().CurrentScene.AddChild(train);
            LineData line = lines.Find(entry => entry.TrainId == trainData.TrainId);
            train.Initialize(trainData, line.LineName, line.LineColor);
            Trains.Add(train);
        }
    }

    public void SpawnRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = robotScene.Instantiate<RobotScript>();
            robot.Initialize(robotData);
            Robots.Add(robot);
        }
    }

    public void UpdateTrains(List<TrainData> trains)
    {
        foreach (TrainData trainData in trains)
        {
            TrainScript train = Trains.Find(train => train.Data.TrainId == trainData.TrainId);
            train?.Update(trainData);
        }
    }

    public void UpdateRobots(List<RobotData> robots)
    {
        foreach (RobotData robotData in robots)
        {
            RobotScript robot = Robots.Find(robot => robot.Data.RobotId == robotData.RobotId);
            robot?.Update(robotData);
        }
    }

    public void Reset(bool withStationExtras)
    {
        FollowCameraScript followCamera = (FollowCameraScript)GetTree().GetFirstNodeInGroup("FollowCamera");
        followCamera.GetParent().RemoveChild(followCamera);
        GetTree().CurrentScene.GetNode("Cameras").AddChild(followCamera);
        
        Robots.ForEach(robot => robot.QueueFree());
        Robots.Clear();
        Stations.ForEach(station => station.Robots.Clear());
        Trains.ForEach(train => train.QueueFree());
        Trains.Clear();
        if (withStationExtras)
        {
            Stations.ForEach(station => station.DisableExtraLoading());
            Stations.ForEach(station => station.DisableExtraCharging());
        }
    }
}
