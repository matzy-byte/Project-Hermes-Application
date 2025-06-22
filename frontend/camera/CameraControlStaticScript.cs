using CommandLine;
using Godot;
using Interface;
using System.Linq;

namespace Camera;

public partial class CameraControlStaticScript : Camera3D
{
    private FollowCameraScript followCamera;

    public override void _Ready()
    {
        followCamera = (FollowCameraScript)GetTree().GetFirstNodeInGroup("FollowCamera");
    }

    public override void _Input(InputEvent @event)
    {
        if (!Current) return;
        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left && button.Pressed)
            {
                Vector2 mousePos = GetViewport().GetMousePosition();
                Vector3 from = ProjectRayOrigin(mousePos);
                Vector3 to = from + ProjectRayNormal(mousePos) * 10000f;

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
                            GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(75, 75, 75));
                            GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 37.5f);
                            Node3D obj = interactable.Select();
                            followCamera.SetTarget(obj);
                            followCamera.Camera.Current = true;
                        }
                    }
                }
                return;
            }
        }
    }
}
