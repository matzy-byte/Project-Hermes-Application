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
    private TextureButton ControlMenuButton { get; set; }

    public override void _Ready()
    {
        Unfolded = false;
        SimulationSpeedSlider = GetNode<HSlider>("%SimulationSpeedSlider");
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
        ControlMenuButton = GetNode<TextureButton>("%ControlMenuButton");
        ControlMenuButton.Pressed += OnControlMenuPressed;
    }

    private void OnStopSimulationPressed()
    {
        GameManagerScript.StopSimulation();
        GetParent<HUDScript>().StopSimulation();
    }

    private void OnControlMenuPressed()
    {
        GetParent<HUDScript>().OpenControlMenu();
    }

    // TODO: Corrent JSON OBJECT
    private void OnSimulationSpeedChanged(double value)
    {
        WebSocketMessage message = new(102, MessageType.SETSIMULATIONSPEED, JsonConvert.SerializeObject("hi"));
        SessionManager.Instance.Request(message);
    }

    private void OnSimulationPausedPressed()
    {
        GameManagerScript.PauseSimulation(SimulationPausedCheckButton.ButtonPressed);
    }

    private void OnCameraStaticPressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraStatic").Current = true;
    }

    private void OnCameraMovablePressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraMovable").Current = true;
    }

    private void OnCameraFreePressed()
    {
        GetTree().CurrentScene.GetNode("Cameras").GetNode<Camera3D>("CameraFree").Current = true;
    }
}
