using Newtonsoft.Json;
using shared;

namespace Z;

public static class TrainManager
{
    private static List<Train> allTrains { get; set; } = [];

    public static void Initialize()
    {
        allTrains = [];
        int trainIndex = 0;
        foreach (Transit transit in DataManager.AllTransits)
        {
            allTrains.Add(new Train(transit, trainIndex));
            trainIndex ++;
        }
    }
    
    public static void UpdateAllTrains()
    {
        foreach (Train train in allTrains)
        {
            train.TrainUpdate();
        }
    }
}