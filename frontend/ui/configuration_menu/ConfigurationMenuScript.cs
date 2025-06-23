using System.Linq;
using Godot;
using Singletons;

namespace UI;

public partial class ConfigurationMenuScript : Panel
{
    private SpinBox TrainWaitingTimeSpinBox { get; set; }
    private TextureButton LoadingStationsButton { get; set; }
    private TextureButton ChargingStationsButton { get; set; }
    private SpinBox PaketCountSpinBox { get; set; }
    private HSlider SimulationSpeedSlider { get; set; }
    private SpinBox RobotCountSpinBox { get; set; }
    private SpinBox MaxPaketsRobotSpinBox { get; set; }
    private SpinBox BatteryCapacitySpinBox { get; set; }
    private SpinBox BatteryConsumptionIdleSpinBox { get; set; }
    private SpinBox BatteryConsumptionActionSpinBox { get; set; }
    private SpinBox BatteryRechargeSpinBox { get; set; }
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
        BatteryConsumptionIdleSpinBox = GetNode<SpinBox>("%BatteryConsumptionIdleSpinBox");
        BatteryConsumptionIdleSpinBox.ValueChanged += OnBatteryConsumptionIdleChanged;
        BatteryConsumptionActionSpinBox = GetNode<SpinBox>("%BatteryConsumptionActionSpinBox");
        BatteryConsumptionActionSpinBox.ValueChanged += OnBatteryConsumptionActionChanged;
        BatteryRechargeSpinBox = GetNode<SpinBox>("%BatteryRechargeSpinBox");
        BatteryRechargeSpinBox.ValueChanged += OnBatteryRechargeChanged;
        StartSimulationButton = GetNode<Button>("%StartSimulationButton");
        StartSimulationButton.Pressed += StartSimulation;

        GameManagerScript.Instance.SimulationSettings.SimulationSpeed = (float)SimulationSpeedSlider.Value;
        GameManagerScript.Instance.SimulationSettings.TrainWaitingTimeAtStation = (float)TrainWaitingTimeSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.LoadingStationIds = ["de:08212:1011", "de:08212:302", "de:08212:17"];
        GameManagerScript.Instance.SimulationSettings.ChargingStationIds = ["de:08212:521", "de:08212:1004", "de:08212:45", "de:08212:409", "de:08212:1208", "de:08215:1902"];
        GameManagerScript.Instance.SimulationSettings.StartPackagesCount = (int)PaketCountSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.NumberOfPackagesInRobot = (int)MaxPaketsRobotSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.NumberOfRobots = (int)RobotCountSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.TotalRobotBatteryCapacity = (float)BatteryCapacitySpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.RobotIdleBatteryConsumption = (float)BatteryConsumptionIdleSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.RobotActionBatteryConsumption = (float)BatteryConsumptionActionSpinBox.Value;
        GameManagerScript.Instance.SimulationSettings.RobotBatteryChargingSpeed = (float)BatteryRechargeSpinBox.Value;
    }

    private void StartSimulation()
    {
        SetLoadingAndChargingStations();
        GameManagerScript.SetSimulationConfiguration(GameManagerScript.Instance.SimulationSettings);
        GameManagerScript.Instance.StartSimulation();
        GetParent<HUDScript>().StartSimulation();
    }

    private void SetLoadingAndChargingStations()
    {
        GameManagerScript.Instance.SimulationSettings.LoadingStationIds = [.. GetParent<HUDScript>().LoadingStationsMenu.GetLoadingStations().Distinct()];
        GameManagerScript.Instance.SimulationSettings.ChargingStationIds = [.. GetParent<HUDScript>().ChargingStationsMenu.GetChargingStations().Distinct()];
    }

    private void OnTrainWaitingTimeChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.TrainWaitingTimeAtStation = (float)value;
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
        GameManagerScript.Instance.SimulationSettings.StartPackagesCount = (int)value;
    }

    private void OnSimulationSpeedChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.SimulationSpeed = (float)value;
    }

    private void OnRobotCountChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.NumberOfRobots = (int)value;
    }

    private void OnMaxPaketRobotChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.NumberOfPackagesInRobot = (int)value;
    }

    private void OnBatteryCapacityChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.TotalRobotBatteryCapacity = (float)value;
    }

    private void OnBatteryConsumptionIdleChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.RobotIdleBatteryConsumption = (float)value;
    }

    private void OnBatteryConsumptionActionChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.RobotActionBatteryConsumption = (float)value;
    }

    private void OnBatteryRechargeChanged(double value)
    {
        GameManagerScript.Instance.SimulationSettings.RobotBatteryChargingSpeed = (float)value;
    }
}
