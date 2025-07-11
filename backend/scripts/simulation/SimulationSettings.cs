using Logs;
using Newtonsoft.Json;
using shared;

namespace Simulation;

public static class SimulationSettings
{
    public static SimulationSettingsData SimulationSettingsParameters { get; set; } = new();

    /// <summary>
    /// Initializes the simulation settings with default values.
    /// </summary>
    public static void Initialize()
    {
        SimulationSettingsParameters.SimulationSpeed = 300f;
        SimulationSettingsParameters.TrainWaitingTimeAtStation = 30f;
        SimulationSettingsParameters.LoadingStationIds = ["de:08212:1011", "de:08212:302", "de:08212:17"];
        SimulationSettingsParameters.ChargingStationIds = ["de:08212:521", "de:08212:1004", "de:08212:45", "de:08212:409", "de:08212:1208", "de:08215:1902"];
        SimulationSettingsParameters.StartPackagesCount = 100;
        SimulationSettingsParameters.NumberOfPackagesInRobot = 15;
        SimulationSettingsParameters.NumberOfRobots = 5;
        SimulationSettingsParameters.TotalRobotBatteryCapacity = 1000;
        SimulationSettingsParameters.RobotIdleBatteryConsumption = 0.5f;
        SimulationSettingsParameters.RobotActionBatteryConsumption = 10f;
        SimulationSettingsParameters.RobotBatteryChargingSpeed = 4f;
    }

    /// <summary>
    /// Updates simulation settings from a JSON string and restarts simulation.
    /// </summary>
    public static async Task UpdateSettings(string settingsJSONstring)
    {
        DataLogger.AddLog("Start Updateting Settings");

        if (SimulationManager.SimulationState.SimulationRunning)
        {
            //Stop the simulation 
            await SimulationManager.StopSimulation();
        }

        //update the settings
        try
        {
            SimulationSettingsData data = JsonConvert.DeserializeObject<SimulationSettingsData>(settingsJSONstring);
            if (data != null)
            {
                SimulationSettingsParameters = data;
                DataLogger.AddLog("Settings Updated");
            }
        }
        catch (Exception e)
        {
            return;
        }

        //Restart the simulation
        SimulationManager.StartSimulation();
    }

    /// <summary>
    /// Updates the simulation speed from a JSON string.
    /// </summary>
    public static void UpdateSimulationSeed(string newSpeedJSONstring)
    {
        try
        {
            SimulationSpeedWrapper speedSettings = JsonConvert.DeserializeObject<SimulationSpeedWrapper>(newSpeedJSONstring);

            SimulationSettingsParameters.SimulationSpeed = speedSettings.SimulationSpeed;
            DataLogger.AddLog("Simulation Speed Changed to: " + SimulationSettingsParameters.SimulationSpeed);

        }
        catch (Exception e)
        {
            return;
        }
    }
}