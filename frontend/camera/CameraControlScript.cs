using Godot;

namespace Camera;

public partial class CameraControlScript : Camera3D
{
    private FollowCameraScript followCamera;
    private bool IsFreeCam = false;
    private float MouseSensitivity = 0.002f;
    private float Speed = 5.0f;
    private Vector3 TargetVelocity = new();
    private bool Rotating = false;

    public override void _Ready()
    {
        followCamera = (FollowCameraScript)GetTree().GetFirstNodeInGroup("FollowCamera");
        if (Name.ToString().Contains("Free"))
        {
            IsFreeCam = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Current) return;
        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Right)
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
            if (button.ButtonIndex == MouseButton.Left && button.Pressed)
            {
                Vector2 mousePos = GetViewport().GetMousePosition();
                Vector3 from = ProjectRayOrigin(mousePos);
                Vector3 to = from + ProjectRayNormal(mousePos) * 1000f;

                var spaceState = GetWorld3D().DirectSpaceState;
                var result = spaceState.IntersectRay(new PhysicsRayQueryParameters3D
                {
                    From = from,
                    To = to,
                    CollisionMask = 1,
                });

                if (result.TryGetValue("collider", out var colliderObj))
                {
                    var colliderNode = (Node)colliderObj;
                    if (colliderNode != null)
                    {
                        if (colliderNode is IInteractable interactable)
                        {
                            Node3D obj = interactable.Select();
                            followCamera.SetTarget(obj);
                            followCamera.Camera.Current = true;
                        }
                    }
                }
            }
            if (button.ButtonIndex == MouseButton.WheelUp && button.Pressed)
            {
                Fov = Mathf.Clamp(Fov - 2.0f, 5.0f, 90.0f);
            }
            else if (button.ButtonIndex == MouseButton.WheelDown && button.Pressed)
            {
                Fov = Mathf.Clamp(Fov + 2.0f, 5.0f, 90.0f);
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && Rotating)
        {
            Vector2 relative = mouseMotion.Relative * MouseSensitivity;
            RotateY(-relative.X);

            if (IsFreeCam)
            {
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
            direction += Transform.Basis.Y * (Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up"));
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