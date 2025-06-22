using System.Collections.Generic;
using System.Linq;
using Godot;
using Interface;
using shared;
using Singletons;
using Stations;
using Trains;
using UI;

namespace Robots;

public partial class RobotScript : StaticBody3D, IInteractable
{
    public RobotData Data { get; set; }
    private readonly Color green = new("#00c735", 1.0f);
    private readonly Color yellow = new("#c7b600", 1.0f);
    private readonly Color red = new("#c71100", 1.0f);
    private StationScript currentStation;
    private TrainScript currentTrain;
    private Sprite3D iconSpriteName;
    private Label iconName;
    private TextureRect iconCircle;
    private GradientTexture2D gradTex;
    private float totalBatteryCapacity;

    public override void _Ready()
    {
        iconSpriteName = GetNode<Sprite3D>("%IconName");
        iconCircle = GetNode<TextureRect>("%Circle");
        iconName = GetNode<Label>("%RobotLabel");
    }

    public override void _MouseEnter()
    {
        iconSpriteName.Visible = true;
    }

    public override void _MouseExit()
    {
        iconSpriteName.Visible = false;
    }

    public void Initialize(RobotData data)
    {
        Data = data;
        currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.CurrentStationId);
        currentStation.AddChild(this);
        currentStation.RobotEnterStation(this);
        gradTex = (GradientTexture2D)iconCircle.Texture.Duplicate(true);
        iconCircle.Texture = gradTex;
        gradTex.Gradient.SetColor(1, green);
        gradTex.Gradient.SetColor(2, green);
        totalBatteryCapacity = GameManagerScript.Instance.SimulationSettings.TotalRobotBatteryCapacity;
        iconName.Text = "Robot #" + data.RobotId.ToString();
    }

    public void Update(RobotData data)
    {
        if (Data.OnStation == data.OnStation && Data.OnTrain == data.OnTrain)
        {
            Data = data;
            return;
        }

        if (data.TrainId == -1)
        {
            currentTrain?.RobotExitTrain(this);
            currentTrain = null;
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == data.CurrentStationId);
            GetParent().RemoveChild(this);
            currentStation.AddChild(this);
            currentStation.RobotEnterStation(this);
        }
        else
        {
            currentStation?.RobotExitStation(this);
            currentTrain = GameManagerScript.Instance.Trains.Find(train => train.Data.TrainId == data.TrainId);
            GetParent().RemoveChild(this);
            currentTrain.AddChild(this);
            currentTrain.RobotEnterTrain(this);
        }

        if (currentStation.Data.StationName != data.CurrentStationId)
        {
            currentStation = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == data.CurrentStationId);
        }
        Data = data;
        CheckBatteryStatus();
    }

    public Node3D Select()
    {
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.ShowInfo(GetInfo(), this);
        return this;
    }

    public Dictionary<string, string> GetInfo()
    {
        int loadedPackages = 0;
        foreach (List<PackageData> packages in Data.LoadedPackages.Values)
        {
            loadedPackages += packages.Count;
        }
        string destinationStationName = "";
        if (Data.TotalPath.Count >= 1)
        {
            destinationStationName = GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == Data.TotalPath.Last().StationIds.Last()).Data.StationName;
        }
        return new Dictionary<string, string>
        {
            ["Robot ID"] = Data.RobotId.ToString(),
            ["Battery Capacity"] = Data.BatteryCapacity.ToString(),
            ["Loaded Packages"] = loadedPackages.ToString(),
            ["Destination Station"] = destinationStationName,
            ["Current Train"] = currentTrain == null ? "None" : Data.TrainId.ToString(),
            ["Current / Latest Station"] = currentStation == null ? "None" : currentStation.Data.StationName
        };
    }

    private void CheckBatteryStatus()
    {
        float level = Data.BatteryCapacity / totalBatteryCapacity;
        Color targetColor = level < 0.2f ? red : level < 0.5f ? yellow : green;

        if (gradTex.Gradient.GetColor(1) != targetColor)
        {
            gradTex.Gradient.SetColor(1, targetColor);
            gradTex.Gradient.SetColor(2, targetColor);
        }
    }
}
