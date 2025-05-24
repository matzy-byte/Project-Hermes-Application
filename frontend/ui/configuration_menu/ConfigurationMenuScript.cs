using Godot;
using shared;
using Singletons;

namespace UI;

public partial class ConfigurationMenuScript : Panel
{
    private SimulationSettingsData simulationSettingsData { get; set; }
    private SpinBox TrainWaitingTimeSpinBox { get; set; }
    private TextureButton LoadingStationsButton { get; set; }
    private TextureButton ChargingStationsButton { get; set; }
    private SpinBox PaketCountSpinBox { get; set; }
    private HSlider SimulationSpeedSlider { get; set; }
    private SpinBox RobotCountSpinBox { get; set; }
    private SpinBox MaxPaketsRobotSpinBox { get; set; }
    private SpinBox BatteryCapacitySpinBox { get; set; }
    private SpinBox BatteryConsumptionSpinBox { get; set; }
    private Button StartSimulationButton { get; set; }

    public override void _Ready()
    {
        TrainWaitingTimeSpinBox = GetNode<SpinBox>("%TrainWaitingTimeSpinBox");
        TrainWaitingTimeSpinBox.ValueChanged += OnTrainWaitingTimeChanged;
        LoadingStationsButton = GetNode<TextureButton>("%LoadingStationsButton");
        LoadingStationsButton.Pressed += OnLoadingStationButtonPressed;
        ChargingStationsButton = GetNode<TextureButton>("%ChargingStationsButton");
        ChargingStationsButton.Pressed += OnChargingStationButtonPressed;
        PaketCountSpinBox = GetNode<SpinBox>("%PaketCountSpinBox");
        PaketCountSpinBox.ValueChanged += OnPaketCountChanged;
        SimulationSpeedSlider = GetNode<HSlider>("%SimulationSpeedSlider");
        SimulationSpeedSlider.ValueChanged += OnSimulationSpeedChanged;
        RobotCountSpinBox = GetNode<SpinBox>("%RobotCountSpinBox");
        RobotCountSpinBox.ValueChanged += OnRobotCountChanged;
        MaxPaketsRobotSpinBox = GetNode<SpinBox>("%MaxPaketsRobotSpinBox");
        MaxPaketsRobotSpinBox.ValueChanged += OnMaxPaketRobotChanged;
        BatteryCapacitySpinBox = GetNode<SpinBox>("%BatteryCapacitySpinBox");
        BatteryCapacitySpinBox.ValueChanged += OnBatteryCapacityChanged;
        BatteryConsumptionSpinBox = GetNode<SpinBox>("%BatteryConsumptionSpinBox");
        BatteryConsumptionSpinBox.ValueChanged += OnBatteryConsumptionChanged;
        StartSimulationButton = GetNode<Button>("%StartSimulationButton");
        StartSimulationButton.Pressed += StartSimulation;
        simulationSettingsData = new();
    }

    private void StartSimulation()
    {
        GameManagerScript.SetSimulationConfiguration(simulationSettingsData);
        GameManagerScript.StartSimulation();
        GetParent<HUDScript>().StartSimulation();
    }

    private void OnTrainWaitingTimeChanged(double value)
    {
        simulationSettingsData.TrainWaitingTimeAtStation = (float)value;
    }

    private void OnLoadingStationButtonPressed()
    {
        GetParent<HUDScript>().OpenLoadingStationMenu();
    }

    private void OnChargingStationButtonPressed()
    {
        GetParent<HUDScript>().OpenChargingStationMenu();
    }

    private void OnPaketCountChanged(double value)
    {
        simulationSettingsData.StartPackagesCount = (int)value;
    }

    private void OnSimulationSpeedChanged(double value)
    {
        simulationSettingsData.SimulationSpeed = (float)value;
    }

    private void OnRobotCountChanged(double value)
    {
        simulationSettingsData.NumberOfRobots = (int)value;
    }

    private void OnMaxPaketRobotChanged(double value)
    {
        simulationSettingsData.NumberOfPackagesInRobot = (int)value;
    }

    private void OnBatteryCapacityChanged(double value)
    {
    }

    private void OnBatteryConsumptionChanged(double value)
    {
    }
}
