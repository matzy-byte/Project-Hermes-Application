namespace shared;

public class SimulationSettingsData
{
    public float SimulationSpeed { get; set; }
    public float TrainWaitingTimeAtStation { get; set; }
    public List<string> LoadingStationIds { get; set; } = [];
    public List<string> ChargingStationIds { get; set; } = [];
    public int StartPackagesCount { get; set; }
    public int NumberOfPackagesInRobot { get; set; }
    public int NumberOfRobots { get; set; }

    public float TotalRobotBatteryCapacity { get; set; }
    public float RobotIdleBatteryConsumption { get; set; }
    public float RobotActionBatteryConsumption { get; set; }
    public float RobotBatteryChargingSpeed { get; set; }
}