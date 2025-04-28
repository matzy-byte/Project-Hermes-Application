using Godot;
using System;
using System.Runtime.Serialization;

public partial class CameraControlScript : Camera3D
{
    private bool IsFreeCam = false;
    private float MouseSensitivity = 0.002f;
    private float Speed = 5.0f;
    private Vector3 TargetVelocity = new();
    private bool Rotating = false;

    public override void _Ready()
    {
        if (Name.ToString().Contains("Free"))
        {
            IsFreeCam = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Current) return;
        if (@event is InputEventMouseButton button && button.ButtonIndex == MouseButton.Right)
        {
            if (button.Pressed)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                Rotating = true;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                Rotating = false;
            }
            return;
        }

        if (@event is InputEventMouseButton wheelEvent)
        {
            if (wheelEvent.ButtonIndex == MouseButton.WheelUp && wheelEvent.Pressed)
            {
                Fov = Mathf.Clamp(Fov - 2.0f, 5.0f, 90.0f); // Zoom in
            }
            else if (wheelEvent.ButtonIndex == MouseButton.WheelDown && wheelEvent.Pressed)
            {
                Fov = Mathf.Clamp(Fov + 2.0f, 5.0f, 90.0f); // Zoom out
            }
        }


        if (@event is InputEventMouseMotion mouseMotion && Rotating)
        {
            Vector2 relative = mouseMotion.Relative * MouseSensitivity;

            // Yaw rotation (left-right) happens in BOTH modes
            RotateY(-relative.X);

            if (IsFreeCam)
            {
                // Pitch rotation (up-down) only when in freecam
                float newPitch = RotationDegrees.X - 50 * relative.Y;
                newPitch = Mathf.Clamp(newPitch, -90f, 90f);
                RotationDegrees = new Vector3(newPitch, RotationDegrees.Y, RotationDegrees.Z);
            }
        }
    }

    
    public override void _PhysicsProcess(double delta)
    {
        if (!Current) return;

        Vector3 direction = Vector3.Zero;
        direction += Transform.Basis.X * (Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"));
        if (IsFreeCam)
        {
            direction += Transform.Basis.Z * (Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward"));
        }
        else
        {
            Vector3 projectionXZ = Transform.Basis.Z;
            projectionXZ.Y = 0;
            direction += projectionXZ * (Input.GetActionStrength("move_backward") - Input.GetActionStrength("move_forward"));
        }

        direction = direction.Normalized() * Speed;

        if (direction.Length() > 0)
        {
            Position += direction;
        }
    }
}
