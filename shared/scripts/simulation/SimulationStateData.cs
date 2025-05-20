namespace shared;

public class SimulationStateData : SimulationSettingsData
{
    public bool SimulationRunning { get; set; }
    public bool SimulationPaused { get; set; }
    public float SimulationTotalTime { get; set; }
    public float SimulationTotalTimeScaled { get; set; }
}