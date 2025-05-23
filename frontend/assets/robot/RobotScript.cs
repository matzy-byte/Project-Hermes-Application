using Godot;
using shared;
using System;

namespace Robots;

public partial class RobotScript : StaticBody3D
{
    public RobotData Data { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }
    
    public void Initialize(RobotData data)
    {
        Data = data;
    }

    public void Update(RobotData data)
    {
        Data = data;
    }
}
