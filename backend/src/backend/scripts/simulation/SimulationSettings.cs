using System.Threading.Tasks;
using Newtonsoft.Json;
using TrainLines;
namespace Simulation
{
    public static class SimulationSettings
    {
        /// <summary>
        /// How fast is the simulation Running
        /// </summary>
        public static float simulationSpeed { get; private set; } = 150f;

        /// <summary>
        /// How many seconds is a train waiting at a station
        /// </summary>
        public static float trainWaitingTimeAtStation { get; private set; } = 30f;

        /// <summary>
        /// What stations are loading stations
        /// </summary>
        public static string[] loadingStationIds { get; private set; } = { "de:08212:1011", "de:08212:302", "de:08212:17" };

        /// <summary>
        /// What stations are loading stations
        /// </summary>
        public static string[] chargingStationsIds { get; private set; } = { "de:08212:1011", "de:08212:302", "de:08212:17" };

        /// <summary>
        /// How many packages are at each loading station at the begin of the simulation
        /// </summary>
        public static int startPackageCount { get; private set; } = 200;

        /// <summary>
        /// How many packages fit in each robot
        /// </summary>
        public static int numberOfPackagesInRobot { get; private set; } = 15;

        /// <summary>
        /// How many robots are there
        /// </summary>
        public static int numberOfRobots { get; private set; } = 5;



        public static async Task updateSettings(string settingsJSONstring)
        {
            if (SimulationManager.isSimulationRunning)
            {
                //Stop the simulation 
                SimulationManager.stopSimulation();
            }

            //update the settings
            try
            {
                updateSettingsFromJSON(settingsJSONstring);
            }
            catch (Exception e)
            {
                return;
            }

            //Restart the simulation
            SimulationManager.startSimulation();
        }


        /// <summary>
        /// Method for changing the speed of the simulation
        /// </summary>
        public static void updateSimulationSeed(string newSpeedJSONstring)
        {
            try
            {
                SimulationSpeed speedSettings = JsonConvert.DeserializeObject<SimulationSpeed>(newSpeedJSONstring);

                simulationSpeed = speedSettings.simulationSpeed;
            }
            catch (Exception e)
            {
                return;
            }
        }


        /// <summary>
        /// Method for updating the settings in the simulation from a json string
        /// </summary>
        private static void updateSettingsFromJSON(string settingsJSON)
        {
            try
            {
                Settings settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);

                //update the settings
                simulationSpeed = settings.simulationSpeed;
                trainWaitingTimeAtStation = settings.trainWaitingTimeAtStation;
                loadingStationIds = settings.loadingStationIds;
                chargingStationsIds = settings.chargingStationsIds;
                startPackageCount = settings.startPackageCount;
                numberOfPackagesInRobot = settings.numberOfPackagesInRobot;
                numberOfRobots = settings.numberOfRobots;
            }
            catch (Exception e)
            {
                throw new Exception("Cant deserialize the json string");
            }
        }


        /// <summary>
        /// Wrapper to load the json string into a struct
        /// </summary>
        private struct Settings
        {
            public float simulationSpeed { get; set; }
            public float trainWaitingTimeAtStation { get; set; }
            public string[] loadingStationIds { get; set; }
            public string[] chargingStationsIds { get; set; }
            public int startPackageCount { get; set; }
            public int numberOfPackagesInRobot { get; set; }
            public int numberOfRobots { get; set; }
        }


        /// <summary>
        /// Wrapper for changing simulation speed
        /// </summary>
        private struct SimulationSpeed
        {
            public float simulationSpeed { get; set; }
        }
    }
}