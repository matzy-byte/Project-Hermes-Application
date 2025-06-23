using Godot;
using Interface;

namespace Camera;

public partial class FollowCameraScript : Node3D
{
    public Camera3D Camera;
    private Node3D Target;
    private Node3D Tilt;
    private const float MouseSensitivity = 0.02f;
    private const float Distance = 300f;
    private bool Rotating = false;
    private float Yaw;
    private float Pitch;

    public override void _Ready()
    {
        Tilt = GetNode<Node3D>("Tilt");
        Camera = Tilt.GetNode<Camera3D>("Camera3D");
        Yaw = Rotation.Y;
        Pitch = Tilt.RotationDegrees.X;
        Camera.Position = new Vector3(0, 0, Distance);
    }

    public override void _Input(InputEvent @event)
    {
        if (!Camera.Current) return;
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
                Vector3 from = Camera.ProjectRayOrigin(mousePos);
                Vector3 to = from + Camera.ProjectRayNormal(mousePos) * 10000f;

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
                            SetTarget(obj);
                            Camera.Current = true;
                        }
                    }
                }
                return;
            }
        }

        if (@event is InputEventMouseMotion mouseMotion && Rotating)
        {
            Vector2 relative = mouseMotion.Relative * MouseSensitivity;

            Yaw -= relative.X;
            Pitch -= relative.Y;
            Pitch = Mathf.Clamp(Pitch, -80f, 80f);
        }
    }

    public override void _Process(double delta)
    {
        if (!Camera.Current)
        {
            return;
        }
        if (Target == null)
        {
            return;
        }

        RotationDegrees = new Vector3(0, Yaw, 0);
        Tilt.RotationDegrees = new Vector3(Pitch, 0, 0);
    }

    public void SetTarget(Node3D target)
    {
        GetParent()?.RemoveChild(this);
        Target = target;
        Target.AddChild(this);
        Transform = new Transform3D(Basis.Identity, Vector3.Zero);
    }
}