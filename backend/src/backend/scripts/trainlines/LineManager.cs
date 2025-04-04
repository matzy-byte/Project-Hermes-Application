using json;
namespace TrainLines
{
    public static class LineManager
    {
        //Paths to the json files
        const string PATHSTATIONJSON = "jsondata\\haltestellen_v2.json";
        const string PATHLINEJSON = "jsondata\\lines_v2.json";
        const string PATHGEODATA = "jsondata\\KVVLinesGeoJSON.json";
        const string TRANSITINFOPATH = "jsondata\\KVV_Transit_Information.json";


        public static Line[] allLines;
        public static Line[] usableLines;


        //Array with all stations 
        private static Station[] allStations;
        private static GeoData[] geoDatas;
        private static TransitInfo[] transitInfos;

        /// <summary>
        /// loads all data from the json files regarding the train lines
        /// </summary>
        public static void initialize()
        {
            Console.WriteLine("Start Initializing Line Data from JSON Files ...");
            //Load all the data from the json files
            try
            {
                allStations = JSONReader.loadStations(PATHSTATIONJSON);
                allLines = JSONReader.loadLines(PATHLINEJSON);
                geoDatas = JSONReader.loadGeoData(PATHGEODATA);
                transitInfos = JSONReader.loadTransitInfo(TRANSITINFOPATH);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }

            Console.WriteLine("Initialized Line Data from JSON files");

            //Add geodata and transitinfos to the lines
            addGeoDataToLines();
            createUsableLines();
            Console.WriteLine("Number of Usable Lines: " + usableLines.Length);
        }

        /// <summary>
        /// Adds the GeoData to the Lines
        /// </summary>
        private static void addGeoDataToLines()
        {
            //Loops over each geoData object and adds it to the line that has the same name or number
            for (int i = 0; i < geoDatas.Length; i++)
            {
                string lineName = geoDatas[i].name;

                for (int j = 0; j < allLines.Length; j++)
                {
                    if (allLines[j].disassembledName == lineName || allLines[j].number == lineName)
                    {
                        allLines[j].geoData = geoDatas[i];
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Creates lines where Transitinfo is available
        /// </summary>
        private static void createUsableLines()
        {
            List<Line> lines = new List<Line>();
            for (int i = 0; i < transitInfos.Length; i++)
            {
                foreach (Line line in allLines)
                {
                    if (line.name == transitInfos[i].lineName)
                    {
                        //save the line
                        Line usableLine = line;
                        //Format line so only relevant stations are in line saved
                        usableLine.transitInfo = transitInfos[i];
                        removeLoopingStations(line);
                        Station[] relevantStations = copyStations(line);

                        //Add the new line to list with all usable lines
                        lines.Add(new Line(line, relevantStations));
                        break;
                    }
                }
            }

            usableLines = lines.ToArray();
        }


        /// <summary>
        /// Copies the relevant stations of a line 
        /// </summary>
        private static Station[] copyStations(Line line)
        {
            Station startStation = findStationWithIdInLine(line.transitInfo.startStationId, line);
            int startStationIndex = Array.IndexOf(line.stations, startStation);
            Station endStation = findStationWithIdInLine(line.transitInfo.destinationStartionId, line);
            int endStationIndex = Array.IndexOf(line.stations, endStation);

            bool forward = startStationIndex < endStationIndex;
            List<Station> relevantStation = new List<Station>();
            if (forward)
            {
                for (int i = startStationIndex; i < endStationIndex + 1; i++)
                {
                    relevantStation.Add(line.stations[i]);
                }
                return relevantStation.ToArray();
            }
            else
            {
                for (int i = startStationIndex; i >= endStationIndex; i--)
                {
                    relevantStation.Add(line.stations[i]);
                }
                return relevantStation.ToArray();
            }
        }


        /// <summary>
        /// Finds a Station based on the station id
        /// </summary>
        public static Station getStationFromId(string stationId)
        {
            //Search all stations for name match
            foreach (Station station in allStations)
            {
                if (station.triasID == stationId)
                {
                    return station;
                }
            }

            throw new Exception("Cant find Matching Station: " + stationId);
        }

        /// <summary>
        /// Finds a station by ID inside a line
        /// </summary>
        public static Station findStationWithIdInLine(string stationId, Line line)
        {
            foreach (Station station in line.stations)
            {
                if (station.triasID == stationId)
                {
                    return station;
                }
            }

            throw new Exception("Station with ID " + stationId + " not found in Line with Name: " + line.name);
        }


        /// <summary>
        /// Removes stations from lines where the station list is looping
        /// </summary>
        private static void removeLoopingStations(Line line)
        {
            bool isLooping = line.stations[0].triasID == line.stations.Last().triasID;
            if (isLooping)
            {
                List<Station> singleInstaceStations = new List<Station>();
                foreach (Station station in line.stations)
                {
                    if (singleInstaceStations.Contains(station) == false)
                    {
                        singleInstaceStations.Add(station);
                    }
                    else
                    {
                        line.stations = singleInstaceStations.ToArray();
                        break;
                    }
                }
            }
        }
    }
}