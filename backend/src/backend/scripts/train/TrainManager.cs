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
                usableTrains.Add(new Train(LineManager.usableLines[i], i));
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

        public static string getTrainPositionsJSON()
        {
            string str = "{\n";
            str += "\"TrainPositions\" : [";
            foreach (Train train in allTrains)
            {
                str += "\n";
                str += train.getTrainPostionJson();
                if (train != allTrains.Last())
                {
                    str += ",";
                }
            }
            str += "\n]\n}";

            return str;
        }

        public static string getTrainLinesJSON()
        {
            string str = "{\n";
            str += "\"TrainLines\" : [\n";

            foreach (Train train in allTrains)
            {
                str += "{\n";
                str += "\"TrainID\" : " + train.id.ToString() + ",\n";
                str += "\"LineName\" : " + "\"" + train.line.name.ToString() + "\"" + "\n";
                str += "}";
                if (train != allTrains.Last())
                {
                    str += ",";
                }
                str += "\n";
            }
            str += "]\n";
            str += "}";
            return str;
        }

        public static string getTrainStationsJSON()
        {
            string str = "{\n";
            str += "\"StationsInLine\" : [";
            foreach (Train train in allTrains)
            {
                str += "\n";
                str += train.getTrainStationsJSON();
                if (train != allTrains.Last())
                {
                    str += ",";
                }
            }
            str += "\n]\n}";
            return str;
        }

        public static string getTrainGeoDataJSON()
        {
            string str = "{\n";
            str += "\"TrainGeoData\" : [";
            foreach (Train train in allTrains)
            {
                str += "\n";
                str += train.getTrainGeoDataJSON();
                if (train != allTrains.Last())
                {
                    str += ",";
                }
            }
            str += "\n]\n}";
            return str;
        }


        public static string getUsedStationsJSON()
        {
            //Find all used Stations
            List<Station> usedStations = new List<Station>();
            foreach (Train train in allTrains)
            {
                foreach (Station station in train.line.stations)
                {
                    if (usedStations.Contains(station) == false)
                    {
                        usedStations.Add(station);
                    }
                }
            }

            string str = "{\n";
            str += "\"UsedStations\" : [\n";
            foreach (Station station in usedStations)
            {
                str += "\n";
                str += station.getStationJSON();
                if (station != usedStations.Last())
                {
                    str += ",";
                }
            }
            str += "\n]\n}";
            return str;
        }
        public static Train getTrainFromLine(Line line)
        {
            foreach (Train train in allTrains)
            {
                if (train.line == line)
                {
                    return train;
                }
            }
            throw new Exception("Cant Find Train with Line: " + line.name);
        }
    }
}