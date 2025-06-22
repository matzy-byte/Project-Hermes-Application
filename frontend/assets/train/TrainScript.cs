using System.Collections.Generic;
using System.Linq;
using Godot;
using Interface;
using Robots;
using shared;
using Singletons;
using Stations;
using UI;

namespace Trains;

public partial class TrainScript : StaticBody3D, IInteractable
{
    public TrainData Data { get; set; }
    private string lineName;
    private Sprite3D iconSpriteName;
    private Label iconName;
    private TextureRect iconCircle;
    private StationScript currentStation;
    private StationScript nextStation;

    private readonly Vector3 robotStartPosition = new(0, 4.0f, -45.0f);
    private List<RobotScript> robots = [];

    public override void _Ready()
    {
        iconSpriteName = GetNode<Sprite3D>("%IconName");
        iconCircle = GetNode<TextureRect>("%Circle");
        iconName = GetNode<Label>("%Label");
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = new Vector3(
        Mathf.Lerp(currentStation.GlobalPosition.X, nextStation.GlobalPosition.X, Data.TravelDistance),
        0,
        Mathf.Lerp(currentStation.GlobalPosition.Z, nextStation.GlobalPosition.Z, Data.TravelDistance)
        );

        if (GlobalPosition.DistanceTo(nextStation.GlobalPosition) > 0.001f)
        {
            LookAt(nextStation.GlobalPosition, Vector3.Up);
        }
    }

    public override void _MouseEnter()
    {
        iconSpriteName.Visible = true;
    }

    public override void _MouseExit()
    {
        iconSpriteName.Visible = false;
    }

    public void Initialize(TrainData data, string lineName, string lineColor)
    {
        Data = data;
        this.lineName = lineName;
        iconName.Text = lineName;
        GradientTexture2D gradTex = (GradientTexture2D)iconCircle.Texture.Duplicate(true);
        iconCircle.Texture = gradTex;
        gradTex.Gradient.SetColor(1, new Color(lineColor, 1.0f));
        gradTex.Gradient.SetColor(2, new Color(lineColor, 1.0f));
        currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
        nextStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId);
    }

    public void Update(TrainData data)
    {
        Data = data;
        if (currentStation.Data.StationId != Data.CurrentStationId || nextStation.Data.StationId != Data.NextStationId)
        {
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
            nextStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId);
        }
    }

    public Node3D Select()
    {
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.ShowInfo(GetInfo(), this);
        return this;
    }

    public Dictionary<string, string> GetInfo()
    {
        string robots = string.Join(", ", GetChildren().OfType<RobotScript>().Select(r => r.Data.RobotId.ToString()));
        string nextStationName = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.NextStationId).Data.StationName;
        return new Dictionary<string, string>
        {
            ["Train ID"] = Data.TrainId.ToString(),
            ["Line"] = lineName,
            ["Next Station"] = nextStationName,
            ["Travel Progress"] = Data.TravelDistance.ToString(),
            ["Robots"] = robots
        };
    }

    public void RobotEnterTrain(RobotScript robot)
    {
        if (robots.Contains(robot))
        {
            return;
        }
        robot.GlobalPosition = GlobalPosition;
        Vector3 pos = robotStartPosition + new Vector3(0, 0, 25 * robots.Count);
        robot.Position = pos;
        robot.GlobalRotation = GlobalRotation;
        robots.Add(robot);
    }

    public void RobotExitTrain(RobotScript robot)
    {
        robots.Remove(robot);
        for (int i = 0; i < robots.Count; i++)
        {
            Vector3 pos = robotStartPosition + new Vector3(0, 0, 25 * i);
            robots[i].Position = pos;
        }
    }
}
