namespace Simulation
{
    public static class SimulationSettingsGlobal
    {
        /// <summary>
        /// Simulation tics per second
        /// </summary>
        public static readonly int simulationLoopsPerSecond = 60;

        /// <summary>
        /// How many stop times are precomputed
        /// </summary>
        public static readonly int preComputedStopTimes = 1000;

        /// <summary>
        /// Time between streamed Data transmisions in ms
        /// </summary>
        public static readonly int dataStreamDelay = 120;

        /// <summary>
        /// Time between train data and robotData in the stream loop in ms
        /// </summary>
        public static readonly int streamDelayBetweenMessages = 10;

        /// <summary>
        /// URL of the WebSocket
        /// </summary>
        public static readonly string webSocketURL = "http://localhost:5000/ws/";

    }
}