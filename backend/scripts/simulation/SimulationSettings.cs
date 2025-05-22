using Newtonsoft.Json;
using shared;

namespace Simulation;

public static class SimulationSettings
{
    public static SimulationSettingsData SimulationSettingsParameters { get; set; } = new();

    public static void Initialize()
    {
        SimulationSettingsParameters.SimulationSpeed = 300f;
        SimulationSettingsParameters.TrainWaitingTimeAtStation = 30f;
        SimulationSettingsParameters.LoadingStationIds = ["de:08212:1011", "de:08212:302", "de:08212:17"];
        SimulationSettingsParameters.ChargingStationIds = ["de:08212:1011", "de:08212:302", "de:08212:17"];
        SimulationSettingsParameters.StartPackagesCount = 200;
        SimulationSettingsParameters.NumberOfPackagesInRobot = 15;
        SimulationSettingsParameters.NumberOfRobots = 5;
    }

    public static async Task UpdateSettings(string settingsJSONstring)
    {
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
            }
        }
        catch (Exception e)
        {
            return;
        }

        //Restart the simulation
        SimulationManager.StartSimulation();
    }

    public static void UpdateSimulationSeed(string newSpeedJSONstring)
    {
        try
        {
            SimulationSpeed speedSettings = JsonConvert.DeserializeObject<SimulationSpeed>(newSpeedJSONstring);

            SimulationSettingsParameters.SimulationSpeed = speedSettings.SimulationSpeedParameter;
        }
        catch (Exception e)
        {
            return;
        }
    }

    /// <summary>
    /// Wrapper for changing simulation speed
    /// </summary>
    private struct SimulationSpeed
    {
        public float SimulationSpeedParameter { get; set; }
    }
}