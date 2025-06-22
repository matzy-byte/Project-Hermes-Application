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
    private Sprite3D iconSpriteName;
    private Label iconName;

    private List<RobotScript> robots = [];
    private float radius = 75;
    private float stepDeg = 12;

    public override void _Ready()
    {
        iconSpriteName = GetNode<Sprite3D>("%IconName");
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
        if (robots.Contains(robot))
        {
            return;
        }
        robot.GlobalPosition = GlobalPosition;
        float radian = Mathf.DegToRad(stepDeg * robots.Count);
        Vector3 pos = new(Mathf.Cos(radian) * radius, 2.6f, Mathf.Sin(radian) * radius);
        robot.Position = pos;
        robot.LookAt(GlobalPosition, Vector3.Up);
        robots.Add(robot);
    }

    public void RobotExitStation(RobotScript robot)
    {
        robots.Remove(robot);
        for (int i = 0; i < robots.Count; i++)
        {
            float radian = Mathf.DegToRad(stepDeg * i + 20);
            Vector3 pos = new(Mathf.Cos(radian) * radius, 2.6f, Mathf.Sin(radian) * radius);
            robots[i].Position = pos;
            robots[i].LookAt(GlobalPosition, Vector3.Up);
        }
    }
}
