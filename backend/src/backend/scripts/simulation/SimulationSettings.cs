using TrainLines;
namespace Simulation
{
    public static class SimulationSettings
    {
        public static int simulationLoopsPerSecond = 60;
        public static float simulationSpeed { get; private set; } = 150f;

        //How many stop times are precomputed
        public static int preComputedStopTimes = 1000;

        //How many seconds is a train waiting at a station
        public static float trainWaitingTimeAtStation = 30f;
        public static string[] loadingStationIds = { "de:08212:1011", "de:08212:302", "de:08212:17" };
        public static int startPackageCount = 200; //How many packages are at each station at the start
        public static int numberOfPackagesInRobot = 15;
        public static int numberOfRobots = 5;

        /// <summary>
        /// Time between data transmision in ms
        /// </summary>
        public static int dataStreamDelay = 120;

        /// <summary>
        /// URL of the WebSocket
        /// </summary>
        public static string webSocketURL = "http://localhost:5000/ws/";



        public static void updateSettings(string settingsJSONstring)
        {
            Console.WriteLine("Settings changed");
        }


        public static void updateSimulationSeed(string newSpeedJSONstring)
        {
            Console.WriteLine("Speed changed");
        }
    }
}