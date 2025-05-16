using shared;

namespace Z;

public class Transit : TransitData
{
    public Line Line { get; set; }
    
    public Transit(TransitInfoWrapper transitInfoWrapper)
    {
        LineName = transitInfoWrapper.LineName;
        Line = DataManager.FindLineByLineName(LineName);
        StartStationId = transitInfoWrapper.StartStationID;
        DestinationStationId = transitInfoWrapper.DestinationID;
        TravelTime = transitInfoWrapper.TravelTime;
        TravelTimeReverse = transitInfoWrapper.TravelTimeReverse;
    }
}