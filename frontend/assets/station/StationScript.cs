using Godot;
using helper;
using Robots;
using shared;
using System;
using System.Collections.Generic;

namespace Stations;

public partial class StationScript : StaticBody3D
{
    public StationData Data { get; set; }
    public List<RobotScript> Robots { get; set; } = [];
    private Sprite3D iconSprite;
    private Sprite3D iconSpriteName;
    private TextureRect icon;
    private Label iconName;

    private float radius = 75;
    private float stepDeg = 12;

    private bool extraLoading = false;
    private bool extraCharging = false;

    public override void _Ready()
    {
        iconSprite = GetNode<Sprite3D>("%Icon");
        iconSpriteName = GetNode<Sprite3D>("%IconName");
        icon = GetNode<TextureRect>("%StationIcon");
        iconName = GetNode<Label>("%Label");
    }

    public override void _MouseEnter()
    {
        iconSpriteName.Visible = true;
    }

    public override void _MouseExit()
    {
        iconSpriteName.Visible = false;
    }

    public void Initialize(StationData data)
    {
        Data = data;
        iconName.Text = data.StationName;
        Position = GeoMapper.LatLonToGameCoords(Data.Latitude, Data.Longitude);
    }

    public void RobotEnterStation(RobotScript robot)
    {
        if (Robots.Contains(robot))
        {
            return;
        }
        robot.GlobalPosition = GlobalPosition;
        float radian = Mathf.DegToRad(stepDeg * Robots.Count);
        Vector3 pos = new(Mathf.Cos(radian) * radius, 2.6f, Mathf.Sin(radian) * radius);
        robot.Position = pos;
        robot.LookAt(GlobalPosition, Vector3.Up);
        Robots.Add(robot);
    }

    public void RobotExitStation(RobotScript robot)
    {
        Robots.Remove(robot);
        for (int i = 0; i < Robots.Count; i++)
        {
            float radian = Mathf.DegToRad(stepDeg * i + 20);
            Vector3 pos = new(Mathf.Cos(radian) * radius, 2.6f, Mathf.Sin(radian) * radius);
            Robots[i].Position = pos;
            Robots[i].LookAt(GlobalPosition, Vector3.Up);
        }
    }

    public void EnableExtraLoading()
    {
        iconSprite.Visible = true;
        GetNode<Node3D>("%LoadingStations").Visible = true;
        GetNode<Node3D>("%ChargingStations").Visible = true;
        icon.Texture = ResourceLoader.Load<Texture2D>("res://assets/loading_station/T_Load_Icon.png");
        extraLoading = true;
    }

    public void DisableExtraLoading()
    {
        GetNode<Node3D>("%LoadingStations").Visible = false;
        if (!extraCharging)
        {
            iconSprite.Visible = false;
            GetNode<Node3D>("%ChargingStations").Visible = false;
        }
        else
        {
            icon.Texture = ResourceLoader.Load<Texture2D>("res://assets/charging_station/T_Charge_Icon.png");
        }
        extraLoading = false;
    }

    public void EnableExtraCharging()
    {
        iconSprite.Visible = true;
        GetNode<Node3D>("%ChargingStations").Visible = true;
        if (!extraLoading)
        {
            icon.Texture = ResourceLoader.Load<Texture2D>("res://assets/charging_station/T_Charge_Icon.png");
        }
        extraCharging = true;
    }

    public void DisableExtraCharging()
    {
        if (!extraLoading)
        {
            iconSprite.Visible = false;
            GetNode<Node3D>("%ChargingStations").Visible = false;
        }
        extraCharging = false;
    }
}
