namespace Z;

public static class Pathfinder
{
    private static Dictionary<Train, List<Train>> directConnectionTable = [];

    public static void Initialize()
    {
        directConnectionTable = [];

        foreach (Train train in TrainManager.AllTrains)
        {
            directConnectionTable.Add(train, GetConnectedTrains(train));
        }
    }

    public static List<Transfer> GetTransfers(string startStationId, string destinationStationId, float enterTime)
    {
        Dictionary<List<Transfer>, float> allTransfers = [];
        List<List<Train>> relevantTrains = GetRelevantTrains(startStationId, destinationStationId);

        foreach (List<Train> trains in relevantTrains)
        {
            List<Transfer> transfers = CreateTransfers(trains, startStationId, destinationStationId);
            if (transfers.Count != 0)
            {
                allTransfers.Add(transfers, GetTravelTime(enterTime, transfers));
            }
        }

        return allTransfers.Aggregate((x, y) => x.Value < y.Value ? x : y).Key;
    }

    private static float GetTravelTime(float enterTime, List<Transfer> transfers)
    {
        float totalTavelTime = 0f;
        Dictionary<int, float> transferTimeTable = [];

        for (int i = 0; i < transfers.Count; i++)
        {
            Transfer transfer = transfers[i];
            if (i == 0)
            {
                transferTimeTable.Add(i, CalculateTravelTime(enterTime, transfer));
                totalTavelTime += transferTimeTable[i];
                continue;
            }

            transferTimeTable.Add(i, CalculateTravelTime(transferTimeTable[i - 1], transfer));
            totalTavelTime += transferTimeTable[i];
        }

        return totalTavelTime;
    }

    private static float CalculateTravelTime(float enterTime, Transfer transfer)
    {
        Train train = TrainManager.AllTrains.Find(x => x.TrainId == transfer.TrainId);
        int enterStationIndex = train.StationIds.IndexOf(transfer.StationIds.First());
        int exitStationIndex = train.StationIds.IndexOf(transfer.StationIds.Last());
        bool drivingForward = enterStationIndex < exitStationIndex;

        float timeWaitingForTrain = train.NextPickupTime(transfer.StationIds.First(), drivingForward, enterTime);
        float timeDrivingTrain = train.GetTravelTime(transfer.StationIds.First(), transfer.StationIds.Last(), drivingForward);

        float travelTime = timeWaitingForTrain + timeDrivingTrain;
        return enterTime + travelTime;
    }

    private static List<Transfer> CreateTransfers(List<Train> trains, string startStationId, string destinationStationId)
    {
        List<Transfer> transfers = [];
        if (trains.Count == 1)
        {
            List<string> stations = trains[0].GetBetweenStation(startStationId, destinationStationId);
            return [new Transfer(trains[0].TrainId, stations)];
        }

        List<string> transferStations = GetTransferStations(trains, startStationId, destinationStationId);

        int index = 0;
        foreach (Train train in trains)
        {
            string enter = transferStations[index];
            string exit = transferStations[index + 1];

            //Remove paths where staions are dublicate (backtravel) or enter and exit are the same
            if (enter == exit || transferStations.Count != transferStations.Distinct().Count())
            {
                return [];
            }

            // Make sure both stations are on the line (optional: for debug)
            if (!train.StationIds.Contains(enter) || !train.StationIds.Contains(exit))
            {
                throw new Exception($"Station mismatch: {enter} or {exit} not on {train}");
            }

            List<string> stations = train.GetBetweenStation(enter, exit);
            transfers.Add(new Transfer(train.TrainId, stations));
        }

        return transfers;
    }

    private static List<string> GetTransferStations(List<Train> trains, string startStationId, string destinationStationId)
    {
        List<string> transferStations = [];

        for (int i = 0; i < trains.Count - 1; i++)
        {
            Train train = trains[i];
            Train nextTrain = trains[i + 1];

            string bestTransfer = "";
            int minCombinedIndex = int.MaxValue;

            foreach (string id in train.StationIds)
            {
                if (nextTrain.StationIds.Contains(id))
                {
                    int stationIndex = train.StationIds.IndexOf(id);
                    int nextStationIndex = nextTrain.StationIds.IndexOf(id);
                    int totalIndex = stationIndex + nextStationIndex;

                    if (totalIndex < minCombinedIndex)
                    {
                        minCombinedIndex = totalIndex;
                        bestTransfer = id;
                    }
                }
            }

            if (bestTransfer == "")
            {
                throw new Exception($"No transfer station between {train} and {nextTrain}");
            }

            transferStations.Add(bestTransfer);
        }

        transferStations.Add(destinationStationId);
        return transferStations;
    }

    private static List<List<Train>> GetRelevantTrains(string startStationId, string destinationStationId)
    {
        List<List<Train>> relevantTrains = [];
        List<Train> startTrains = [];
        List<Train> destinationTrains = [];
        foreach (Train train in TrainManager.AllTrains)
        {
            if (train.StationIds.Contains(startStationId))
                startTrains.Add(train);

            if (train.StationIds.Contains(destinationStationId))
                destinationTrains.Add(train);
        }

        foreach (Train train in startTrains)
        {
            if (destinationTrains.Contains(train))
                relevantTrains.Add([train]);
        }

        int maxTrains = 3;

        foreach (Train startTrain in startTrains)
        {
            foreach (Train destinationTrain in destinationTrains)
            {
                if (startTrain == destinationTrain)
                    continue;

                Queue<List<Train>> queue = new();
                queue.Enqueue([startTrain]);

                while (queue.Count > 0)
                {
                    List<Train> currentTrains = queue.Dequeue();
                    Train lastTrain = currentTrains.Last();

                    if (lastTrain == destinationTrain)
                    {
                        relevantTrains.Add(currentTrains);
                        continue;
                    }

                    if (currentTrains.Count >= maxTrains)
                    {
                        continue;
                    }

                    if (!directConnectionTable.TryGetValue(lastTrain, out List<Train>? connectedTrains))
                    {
                        continue;
                    }

                    foreach (Train nextTrain in connectedTrains)
                    {
                        if (!currentTrains.Contains(nextTrain))
                        {
                            currentTrains.Add(nextTrain);
                            queue.Enqueue(currentTrains);
                        }
                    }
                }
            }
        }

        return [.. relevantTrains.OrderBy(x => x.Count)];
    }

    private static List<Train> GetConnectedTrains(Train train)
    {
        List<Train> connectedTrains = [];

        foreach (string id in train.StationIds)
        {
            foreach (Train TrainToCheck in TrainManager.AllTrains)
            {
                if (TrainToCheck == train || connectedTrains.Contains(TrainToCheck))
                    continue;

                if (TrainToCheck.StationIds.Contains(id))
                    connectedTrains.Add(TrainToCheck);
            }
        }

        return connectedTrains;
    }
}