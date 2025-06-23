namespace shared;

public class TrainData
{
    public int TrainId { get; set; }
    public bool Driving { get; set; }
    public bool InStation { get; set; }
    public bool DrivingForward { get; set; }
    public string CurrentStationId { get; set; } = "";
    public string NextStationId { get; set; } = "";
    public float TravelDistance { get; set; }
    public float WaitingTime { get; set; }
}