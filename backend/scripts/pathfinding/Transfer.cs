using shared;

namespace Pathfinding;

/// <summary>
/// Represents a train transfer route consisting of a train ID and a list of stations on its path.
/// Inherits from <see cref="TransferData"/>.
/// </summary>
public class Transfer : TransferData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Transfer"/> class.
    /// </summary>
    public Transfer(int trainId, List<string> stationIds)
    {
        TrainId = trainId;
        StationIds = stationIds;
    }
}
