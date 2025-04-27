public class Robot
{
    public int RobotID { get; set; }
    public bool OnPath { get; set; }
    public bool OnTrain { get; set; }
    public bool OnStation { get; set; }
    public int? TrainID { get; set; }
    public string? CurrentStationID { get; set; }
    public Travel TotalPath { get; set; }

}

public class Travel
{
    public string StartStationID { get; set; }
    public string EndStationID { get; set; }
    public Transfer[] SubPaths { get; set; }
}

public class Transfer
{
    public int TrainID { get; set; }
    public string[] Stations { get; set; }
}