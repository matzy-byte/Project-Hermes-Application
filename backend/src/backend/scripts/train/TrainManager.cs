using TrainLines;
namespace Trains
{
    public static class TrainManager
    {
        public static Train[] allTrains;


        /// <summary>
        /// Initializes all trains
        /// </summary>
        public static void initialize()
        {
            List<Train> usableTrains = new List<Train>();

            for (int i = 0; i < LineManager.usableLines.Length; i++)
            {
                usableTrains.Add(new Train(LineManager.usableLines[i]));

            }

            allTrains = usableTrains.ToArray();
            Console.WriteLine("Number of Trains initialized: " + allTrains.Length);
        }


        /// <summary>
        /// Updates all trains
        /// </summary>
        public static void updateAllTrains()
        {
            foreach (Train train in allTrains)
            {
                train.trainUpdate();
            }
        }
    }
}