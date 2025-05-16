using Newtonsoft.Json;

namespace Z;

public static class RobotManager
{
    public static List<Robot> AllRobots { get; set; } = [];

    public static void Initialize()
    {
        AllRobots = [];
        string currentStationId = TrainManager.AllTrains[0].StationIds[0];
        for (int i = 0; i < SimulationSettings.SimulationSettingsParameters.NumberOfRobots; i++)
        {
            AllRobots.Add(new Robot(i, currentStationId));
        }
        Console.WriteLine($"Number Of Robots Initialized: {AllRobots.Count}");
    }

    public static void UpdateAllRobots()
    {
        foreach (Robot robot in AllRobots)
        {
            robot.Update();
        }
    }
}