using shared;

namespace Z;

public class Transfer : TransferData
{
    public Transfer(int trainId, List<string> stationIds)
    {
        TrainId = trainId;
        StationIds = stationIds;
    }
}