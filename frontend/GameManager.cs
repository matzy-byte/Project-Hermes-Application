using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node
{
    public static GameManager Instance;
    public string simulationSettings;

    private PackedScene stationScene = ResourceLoader.Load<PackedScene>("res://assets/station/Station.tscn");
    private PackedScene trainScene = ResourceLoader.Load<PackedScene>("res://assets/train/Train.tscn");
    private PackedScene robotScene = ResourceLoader.Load<PackedScene>("res://assets/robot/Robot.tscn");

    public List<StationScript> stations = [];
    public List<TrainScript> trains = [];
    public List<RobotScript> robots = [];

    public override void _Ready()
    {
        Instance = this;
    }

    public void UpdateSimulationSettings()
    {}

    public void StartSimulation()
    {
        SessionManager.Instance.Request(3, MessageType.STARTSIMULATION);
    }

    public void StopSimulation()
    {
        SessionManager.Instance.Request(3, MessageType.STOPSIMULATION);
    }

    public void PauseSimulation(bool pause)
    {
        if (pause)
        {
            SessionManager.Instance.Request(3, MessageType.PAUSESIMULATION);
            return;
        }
        SessionManager.Instance.Request(3, MessageType.CONTINUESIMULATION);
    }

    public void SpawnStations(Station[] stations)
    {
        foreach (Station station in stations)
        {
            StationScript newStation = stationScene.Instantiate<StationScript>();
            newStation.Setup(station.StationID, station.StationName, station.Long, station.Lat);
            GetTree().CurrentScene.AddChild(newStation);
            newStation.GlobalPosition = GeoMapper.LatLonToGameCoords(station.Lat / 1e13, station.Long / 1e14);
            this.stations.Add(newStation);
        }
    }

    public void SpawnTrains(Train[] trains)
    {
        foreach (Train train in trains)
        {
            TrainScript newTrain = trainScene.Instantiate<TrainScript>();
            StationScript currentStation = stations.Find(x => train.CurrentStation == x.StationID);
            StationScript nextStation = stations.Find(x => train.NextStation == x.StationID);
            newTrain.Setup(train.TrainID, currentStation, nextStation, train.TravelDistance, train.WaitingTime);
            GetTree().CurrentScene.AddChild(newTrain);
            newTrain.GlobalPosition = currentStation.GlobalPosition;
            this.trains.Add(newTrain);
        }
    }

    public void SpawnRobots(Robot[] robots)
    {
        foreach (Robot robot in robots)
        {
            RobotScript newRobot = robotScene.Instantiate<RobotScript>();
            if (robot.TrainID != null)
            {
                TrainScript currentTrain = trains.Find(x => x.TrainID == robot.TrainID);
                newRobot.Setup(robot.RobotID, currentTrain, null);
                currentTrain.AddChild(newRobot);
                newRobot.GlobalPosition = currentTrain.GlobalPosition;
            }
            else if (robot.CurrentStationID != null)
            {
                StationScript currentStation = stations.Find(x => x.StationID == robot.CurrentStationID);
                newRobot.Setup(robot.RobotID, null, currentStation);
                currentStation.AddChild(newRobot);
                newRobot.GlobalPosition = currentStation.GlobalPosition;
            }
            this.robots.Add(newRobot);
        }
    }

    public void UpdateTrains(Train[] trains)
    {
        foreach (TrainScript train in this.trains)
        {
            Train data = trains.First(x => train.TrainID == x.TrainID);
            StationScript currentStation = stations.Find(x => data.CurrentStation == x.StationID);
            StationScript nextStation = stations.Find(x => data.NextStation == x.StationID);
            train.UpdateRoute(currentStation, nextStation, data.TravelDistance, data.WaitingTime);
        }
    }

    public void UpdateRobots(Robot[] robots)
    {
        foreach (RobotScript robot in this.robots)
        {
            Robot data = robots.First(x => x.RobotID == robot.RobotID);
            if (robot.Train == null && data.TrainID != null)
            {
                TrainScript currentTrain = trains.Find(x => x.TrainID == data.TrainID);
                robot.Setup(robot.RobotID, currentTrain, null);
                robot.GetParent().RemoveChild(robot);
                currentTrain.AddChild(robot);
                robot.GlobalPosition = currentTrain.GlobalPosition;
            }
            else if (robot.CurrentStation == null && data.CurrentStationID != null)
            {
                StationScript currentStation = stations.First(x => x.StationID == data.CurrentStationID);
                robot.Setup(robot.RobotID, null, currentStation);
                robot.GetParent().RemoveChild(robot);
                currentStation.AddChild(robot);
                robot.GlobalPosition = currentStation.GlobalPosition;
            }
        }
    }
}
