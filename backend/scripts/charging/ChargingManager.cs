using Simulation;
using Robots;
using Pathfinding;
using Helper;
using Microsoft.AspNetCore.Mvc.Filters;
namespace Charging;

public static class ChargingManager
{
    public static List<string> ChargingStations = [];

    public static void Initialize()
    {
        ChargingStations = SimulationSettings.SimulationSettingsParameters.ChargingStationIds;
        //Every Loading Stations is also a charging stations
        ChargingStations.AddRange(SimulationSettings.SimulationSettingsParameters.LoadingStationIds);
        //Remove dublicates
        ChargingStations.Distinct();
    }


    public static void ChargeRobot(Robot robot)
    {
        //Delta Time in Minutes
        float deltaTimeMinute = SimulationManager.scaledDeltaTime / 60f;
        float newCapacity = robot.BatteryCapacaty + deltaTimeMinute * SimulationSettings.SimulationSettingsParameters.RobotBatteryChargingSpeed;

        //Add Battery Capacaty
        robot.BatteryCapacaty += Math.Clamp(newCapacity, 0, SimulationSettings.SimulationSettingsParameters.TotalRobotBatteryCapacity);
    }
}