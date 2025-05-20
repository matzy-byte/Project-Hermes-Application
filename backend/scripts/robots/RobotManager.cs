using Trains;
using Simulation;

namespace Robots;

public static class RobotManager
{
    public static List<Robot> AllRobots { get; set; } = [];

    public static void Initialize()
    {
        AllRobots = [];

        Random random = new Random();
        for (int i = 0; i < SimulationSettings.SimulationSettingsParameters.NumberOfRobots; i++)
        {

            AllRobots.Add(new Robot(i, TrainManager.AllStations[random.Next(0, TrainManager.AllStations.Count)]));
        }
        Console.WriteLine($"Number Of Robots Initialized: {AllRobots.Count}");
        DataLogger.AddLog("Number of Robots Initialized: " + AllRobots.Count);

    }

    public static void UpdateAllRobots()
    {
        foreach (Robot robot in AllRobots)
        {
            robot.Update();
        }
    }
}