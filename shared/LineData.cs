namespace shared;

public class LineData
{
    public int TrainId { get; set; }
    public string LineName { get; set; } = "";
    public List<string> Stations { get; set; } = [];
}