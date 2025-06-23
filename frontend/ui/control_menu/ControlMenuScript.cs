using System.Linq;
using CommandLine;
using Godot;
using Newtonsoft.Json;
using shared;
using Singletons;

namespace UI;

public partial class ControlMenuScript : HBoxContainer
{
    public bool Unfolded { get; set; }

    private HSlider SimulationSpeedSlider { get; set; }
    private CheckButton SimulationPausedCheckButton { get; set; }
    private Button CameraStaticButton { get; set; }
    private Button CameraMovableButton { get; set; }
    private Button CameraFreeButton { get; set; }
    private Button StopSimulationButton { get; set; }
    private Button NewSimulationButton { get; set; }
    private TextureButton ControlMenuButton { get; set; }

    public override void _Ready()
    {
        Unfolded = false;
        SimulationSpeedSlider = GetNode<HSlider>("%SimulationSpeedSlider");
        SimulationSpeedSlider.Value = GameManagerScript.Instance.SimulationSettings.SimulationSpeed;
        SimulationSpeedSlider.ValueChanged += OnSimulationSpeedChanged;
        SimulationPausedCheckButton = GetNode<CheckButton>("%SimulationPausedCheckButton");
        SimulationPausedCheckButton.Pressed += OnSimulationPausedPressed;
        CameraStaticButton = GetNode<Button>("%CameraStaticButton");
        CameraStaticButton.Pressed += OnCameraStaticPressed;
        CameraMovableButton = GetNode<Button>("%CameraMovableButton");
        CameraMovableButton.Pressed += OnCameraMovablePressed;
        CameraFreeButton = GetNode<Button>("%CameraFreeButton");
        CameraFreeButton.Pressed += OnCameraFreePressed;
        StopSimulationButton = GetNode<Button>("%StopSimulationButton");
        StopSimulationButton.Pressed += OnStopSimulationPressed;
        NewSimulationButton = GetNode<Button>("%NewSimulationButton");
        NewSimulationButton.Pressed += OnNewSimulationPressed;
        ControlMenuButton = GetNode<TextureButton>("%ControlMenuButton");
        ControlMenuButton.Pressed += OnControlMenuPressed;
    }

    public void UpdateSpeedSlider()
    {
        SimulationSpeedSlider.Value = GameManagerScript.Instance.SimulationSettings.SimulationSpeed;
    }

    private void OnStopSimulationPressed()
    {
        GameManagerScript.Instance.StopSimulation();
        NewSimulationButton.Disabled = false;
        StopSimulationButton.Disabled = true;
    }

    private void OnNewSimulationPressed()
    {
        GetParent<HUDScript>().NewSimulation();
        NewSimulationButton.Disabled = true;
        StopSimulationButton.Disabled = false;

        OnCameraStaticPressed();
        GameManagerScript.Instance.Reset(true);
    }

    private void OnControlMenuPressed()
    {
        GetParent<HUDScript>().OpenControlMenu();
    }

    private void OnSimulationSpeedChanged(double value)
    {
        WebSocketMessage message = new(
            201,
            MessageType.SETSIMULATIONSPEED,
            JsonConvert.SerializeObject(new SimulationSpeedWrapper() { SimulationSpeed = (float)value })
        );
        SessionManager.Instance.Request(message);
        GameManagerScript.Instance.SimulationSettings.SimulationSpeed = (float)value;
    }

    private void OnSimulationPausedPressed()
    {
        GameManagerScript.PauseSimulation(!SimulationPausedCheckButton.ButtonPressed);
    }

    private void OnCameraStaticPressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraStatic").Current = true;
        GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(550, 550, 550));
        GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 275f);
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
    }

    private void OnCameraMovablePressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraMovable").Current = true;
        GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(150, 150, 150));
        GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 75);
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
    }

    private void OnCameraFreePressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraFree").Current = true;
        GetTree().GetNodesInGroup("Sprite").ToList().ForEach(x => x.Cast<Sprite3D>().Scale = new Vector3(75, 75, 75));
        GetTree().GetNodesInGroup("SpriteCollider").ToList().ForEach(x => ((SphereShape3D)x.Cast<CollisionShape3D>().Shape).Radius = 37.5f);
        ((HUDScript)GetTree().GetFirstNodeInGroup("HUD")).ObjectInfo.Stop();
    }
}
