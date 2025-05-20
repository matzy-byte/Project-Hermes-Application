namespace shared;

public class LineData
{
    public string LineName { get; set; } = "";
    public List<string> Stations { get; set; } = [];
    public int TrainId { get; set; }

    // public List<CoordinateData> Coordinates { get; set; } = [];
}