using Trains;
using Simulation;

namespace Robots;

/// <summary>
/// Manages the collection of all robots in the simulation.
/// </summary>
public static class RobotManager
{
    /// <summary>
    /// List containing all robot instances in the simulation.
    /// </summary>
    public static List<Robot> AllRobots { get; set; } = [];

    /// <summary>
    /// Initializes the robot collection by creating the configured number of robots
    /// and assigning them to random starting stations.
    /// </summary>
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

    /// <summary>
    /// Updates the state of all robots by calling their Update methods.
    /// </summary>
    public static void UpdateAllRobots()
    {
        foreach (Robot robot in AllRobots)
        {
            robot.Update();
        }
    }
}
