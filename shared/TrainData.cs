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

    public TrainData Json()
    {
        return new TrainData
        {
            TrainId = TrainId,
            Driving = Driving,
            InStation = InStation,
            DrivingForward = DrivingForward,
            CurrentStationId = CurrentStationId,
            NextStationId = NextStationId,
            TravelDistance = TravelDistance,
            WaitingTime = WaitingTime
        };
    }
}