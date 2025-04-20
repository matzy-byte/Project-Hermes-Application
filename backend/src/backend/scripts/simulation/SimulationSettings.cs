using TrainLines;
namespace Simulation
{
    public static class SimulationSettings
    {
        public static int simulationLoopsPerSecond = 60;
        public static float simulationSpeed { get; private set; } = 50f;

        //How many stop times are precomputed
        public static int preComputedStopTimes = 1000;

        //How many seconds is a train waiting at a station
        public static float trainWaitingTimeAtStation = 30f;
        public static string[] loadingStationIds = { "de:08212:1011", "de:08212:302", "de:08212:17" };
        public static int startPackageCount = 1500; //How many packages are at each station at the start
        public static int numberOfPackagesInRobot = 50;
        public static int numberOfRobots = 1;

        /// <summary>
        /// Time between data transmision in ms
        /// </summary>
        public static int dataStreamDelay = 120;

        /// <summary>
        /// URL of the WebSocket
        /// </summary>
        public static string webSocketURL = "http://localhost:5000/ws/";
    }
}