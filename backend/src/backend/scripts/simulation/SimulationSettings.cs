using TrainLines;
namespace Simulation
{
    public static class SimulationSettings
    {
        public static int simulationLoopsPerSecond = 60;
        public static float simulationSpeed { get; private set; } = 50f;

        //How many seconds is a train waiting at a station
        public static float trainWaitingTimeAtStation = 30f;
        public static int numberOfLoadingStations = 2;
        public static string[] loadingStationIds;

        public static int numberOfRobots = 2;


        /// <summary>
        /// Change the loading Stations with Ids
        /// </summary>
        public static void setLoadingStationsWithID(string[] newLoadingStationIds)
        {
            loadingStationIds = newLoadingStationIds;
        }

        /// <summary>
        /// Change the loading Stations with station Objects
        /// </summary>
        public static void setLoadingStationsWithStation(Station[] newLoadingStations)
        {
            loadingStationIds = new string[newLoadingStations.Length];
            for (int i = 0; i < loadingStationIds.Length; i++)
            {
                loadingStationIds[i] = newLoadingStations[i].triasID;
            }
        }


        /// <summary>
        /// Method to change the simultion speed
        /// </summary>
        public static void changeSimulationSpeed(float newSimulationSpeed)
        {
            simulationSpeed = newSimulationSpeed;
        }
    }
}