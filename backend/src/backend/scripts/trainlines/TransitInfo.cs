using json;

public class TransitInfo()
{
    public string lineDataName;
    public string lineName;
    public string startStationId;
    public string destinationStartionId;
    public string lineNameAbreviation;
    public int travelTime;
    public int travelTimeReverse;

    public TransitInfo(TransitInfoWrapper transitInfoWrapper) : this()
    {
        lineDataName = transitInfoWrapper.LineDataName;
        lineName = transitInfoWrapper.LineName;
        startStationId = transitInfoWrapper.StartStationID;
        destinationStartionId = transitInfoWrapper.StartStationID;
        lineNameAbreviation = transitInfoWrapper.LineNameAbreviation;
        travelTime = transitInfoWrapper.TravelTime;
        travelTimeReverse = transitInfoWrapper.TravelTimeReverse;
    }
}