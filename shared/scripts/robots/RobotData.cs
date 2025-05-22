namespace shared;

public class RobotData
{
    public int RobotId { get; set; }
    public bool OnTrain { get; set; }
    public bool OnStation { get; set; }
    public int TrainId { get; set; }
    public string CurrentStationId { get; set; } = "";
    public List<TransferData> TotalPath { get; set; } = [];

    public RobotData Json()
    {
        return new RobotData
        {
            RobotId = RobotId,
            OnTrain = OnTrain,
            OnStation = OnStation,
            TrainId = TrainId,
            CurrentStationId = CurrentStationId,
            TotalPath = TotalPath
        };
    }
}