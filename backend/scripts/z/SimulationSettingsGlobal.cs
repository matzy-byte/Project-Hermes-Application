namespace Z;

public static class SimulationSettingsGlobal
{
    /// <summary>
    /// Simulation tics per second
    /// </summary>
    public static readonly int SimulationLoopsPerSecond = 60;

    /// <summary>
    /// How many stop times are precomputed
    /// </summary>
    public static readonly int PreComputedStopTimes = 1000;

    /// <summary>
    /// Time between streamed Data transmisions in ms
    /// </summary>
    public static readonly int DataStreamDelay = 5;

    /// <summary>
    /// Time between train data and robotData in the stream loop in ms
    /// </summary>
    public static readonly int StreamDelayBetweenMessages = 10;

    /// <summary>
    /// URL of the WebSocket
    /// </summary>
    public static readonly string WebSocketURL = "http://localhost:5000/ws/";
}