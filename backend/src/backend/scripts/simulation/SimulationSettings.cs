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
        public static int numberOfLoadingStations = 2;
        public static string[] loadingStationIds;
        public static int numberOfRobots = 2;

        /// <summary>
        /// Time between data transmision in ms
        /// </summary>
        public static int dataStreamDelay = 50;

        /// <summary>
        /// URL of the WebSocket
        /// </summary>
        public static string webSocketURL = "http://localhost:5000/ws/";
    }
}