namespace shared;

public class RobotData
{
    public int RobotId { get; set; }
    public bool OnTrain { get; set; }
    public bool OnStation { get; set; }
    public int TrainId { get; set; }
    public string CurrentStationId { get; set; } = "";
    public float BatteryCapacaty { get; set; }
    public bool IsCharging { get; set; }
    public List<TransferData> TotalPath { get; set; } = [];
}