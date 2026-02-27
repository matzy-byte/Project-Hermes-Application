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
	private const float FollowSpeed = 10f;

	private bool Rotating = false;
	private float Yaw;
	private float Pitch;

	private const float DefaultYaw   = -35f;
	private const float DefaultPitch = -20f;


	public override void _Ready()
	{
		Tilt = GetNode<Node3D>("Tilt");
		Camera = Tilt.GetNode<Camera3D>("Camera3D");

		Yaw = RotationDegrees.Y;
		Pitch = Tilt.RotationDegrees.X;

		Camera.Position = new Vector3(0, 0, Distance);
	}

	public override void _Input(InputEvent @event)
	{
		if (!Camera.Current)
			return;

		if (@event is InputEventMouseButton button)
		{
			if (button.ButtonIndex == MouseButton.Right)
			{
				Input.MouseMode = button.Pressed
					? Input.MouseModeEnum.Captured
					: Input.MouseModeEnum.Visible;

				Rotating = button.Pressed;
				return;
			}

			if (button.ButtonIndex == MouseButton.Left && button.Pressed)
			{
				RaycastSelect();
				return;
			}
		}

		if (@event is InputEventMouseMotion motion && Rotating)
		{
			Vector2 rel = motion.Relative * MouseSensitivity;

			Yaw -= rel.X;
			Pitch -= rel.Y;
			Pitch = Mathf.Clamp(Pitch, -80f, 80f);
		}
	}

	public override void _Process(double delta)
	{
		if (!Camera.Current || Target == null)
			return;
		
		Vector3 desiredPos = Target.GlobalPosition;

		GlobalPosition = GlobalPosition.Lerp(
			desiredPos,
			(float)(FollowSpeed * delta)
		);

		RotationDegrees = new Vector3(0, Yaw, 0);
		Tilt.RotationDegrees = new Vector3(Pitch, 0, 0);
	}

	public void SetTarget(Node3D target)
	{
		Target = target;

		GlobalPosition = target.GlobalPosition;

		Yaw = target.GlobalRotationDegrees.Y + 25f;
		Pitch = -50f;


		RotationDegrees = new Vector3(0, Yaw, 0);
		Tilt.RotationDegrees = new Vector3(Pitch, 0, 0);
	}

	private void RaycastSelect()
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
	}
}
