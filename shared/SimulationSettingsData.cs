namespace shared;

public class SimulationSettingsData
{
    public int SimulationSpeed { get; set; }
    public float TrainWaitingTimeAtStation { get; set; }
    public List<string> LoadingStationIds { get; set; } = [];
    public List<string> ChargingStationIds { get; set; } = [];
    public int StartPackagesCount { get; set; }
    public int NumberOfPackagesInRobot { get; set; }
    public int NumberOfRobots { get; set; }
}