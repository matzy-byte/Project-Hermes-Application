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


        //Array with all stations that have all informations needed
        //Stations are sorted and only relevant stations are assign to the lines 
        public static Line[] usableLines;


        //Arrays with all loaded informations 
        private static Line[] allLines;
        private static Station[] allStations;
        private static GeoData[] geoDatas;
        private static TransitInfo[] transitInfos;

        /// <summary>
        /// loads all data from the json files regarding the train lines
        /// </summary>
        public static void initialize()
        {
            Console.WriteLine("Start Initializing Line Data from JSON Files ...");
            loadDataFromJson();
            Console.WriteLine("Initialized Line Data from JSON files");

            //Create the usable lines
            initializeUsableLines();
            Console.WriteLine("Number of Usable Lines: " + usableLines.Length);
            string str = usableLinesToString();
            Console.WriteLine(str);
        }


        /// <summary>
        /// Loads all the data from the json files
        /// </summary>
        private static void loadDataFromJson()
        {
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
        }

        /// <summary>
        /// Method to initialize all the usable lines
        /// </summary>
        private static void initializeUsableLines()
        {
            usableLines = new Line[transitInfos.Length];

            for (int i = 0; i < transitInfos.Length; i++)
            {
                //Create a usabel line 
                usableLines[i] = createUsableLineFromTranistInfo(transitInfos[i]);
                addGeoDataToLine(usableLines[i]);

                //Rearange stations and geoData so that transit info start station is at index 0
                rearangeData(usableLines[i]);
            }
        }

        /// <summary>
        /// Adds the GeoData to the Lines
        /// </summary>
        private static void addGeoDataToLine(Line line)
        {
            //Loops over each geoData object and adds it to the line that has the same name or number
            for (int i = 0; i < geoDatas.Length; i++)
            {
                string geoDataLineName = geoDatas[i].name;
                string lineNumber = line.number;
                string lineDisassembledName = line.disassembledName;
                if (lineDisassembledName == geoDataLineName || lineNumber == geoDataLineName)
                {
                    line.geoData = geoDatas[i];
                    return;
                }
            }
        }


        /// <summary>
        /// Creates line from transit info with relevant stations
        /// </summary>
        private static Line createUsableLineFromTranistInfo(TransitInfo transitInfo)
        {
            foreach (Line line in allLines)
            {
                if (line.name == transitInfo.lineName)
                {
                    //Creates a 1 to 1 copy of the original line
                    Line copyOriginalLine = new Line(line);

                    //Remove stations that are "looping"
                    removeLoopingStations(copyOriginalLine);
                    Station[] relevantStations = getRelevantStations(copyOriginalLine, transitInfo);

                    //Add the new line to list with all usable lines
                    return new Line(copyOriginalLine, transitInfo, relevantStations);
                }
            }

            throw new Exception("Cant create usable line with transit Info: " + transitInfo.lineName);
        }


        /// <summary>
        /// Extracts the relevant stations of a line defined by the transitinfo
        /// </summary>
        private static Station[] getRelevantStations(Line line, TransitInfo transitInfo)
        {
            Station startStation = findStationWithIdInLine(transitInfo.startStationId, line);
            int startStationIndex = Array.IndexOf(line.stations, startStation);
            Station endStation = findStationWithIdInLine(transitInfo.destinationStartionId, line);
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

            List<Station> singleInstaceStations = new List<Station>();
            if (line.stations[0] == line.stations.Last())
            {
                foreach (Station station in line.stations)
                {
                    if (singleInstaceStations.Contains(station) == false)
                    {
                        singleInstaceStations.Add(station);
                    }
                    else
                    {
                        Console.WriteLine("Looping Stations found at: " + line.name);
                        break;
                    }
                }
                line.stations = singleInstaceStations.ToArray();
            }


        }


        /// <summary>
        /// Rearanges the stations and GeoData of a line so that start station is at index 0
        /// </summary>
        private static void rearangeData(Line line)
        {
            Station startStation = findStationWithIdInLine(line.transitInfo.startStationId, line);
            int startStationIndex = Array.IndexOf(line.stations, startStation);
            Station endStation = findStationWithIdInLine(line.transitInfo.destinationStartionId, line);
            int endStationIndex = Array.IndexOf(line.stations, endStation);

            //When startindex is bigger than end index
            if (startStationIndex > endStationIndex)
            {
                line.flipStations();
                line.flipGeoData();
                Console.WriteLine("Rearanged Data for Line: " + line.name);
            }
        }


        /// <summary>
        /// Gets all lines that contain a station
        /// </summary>
        public static List<Line> getLinesWithStation(Station station)
        {
            List<Line> linesWithStation = new List<Line>();
            foreach (Line line in usableLines)
            {
                if (line.stations.Contains(station))
                {
                    linesWithStation.Add(line);
                }
            }

            return linesWithStation;
        }


        /// <summary>
        /// String with all usabel lines as Debug output
        /// </summary>
        /// <returns></returns>
        private static string usableLinesToString()
        {
            string str = "All Usable Lines: ";

            foreach (Line line in usableLines)
            {
                str += "\n" + line.ToString() + "\n";
            }

            return str;
        }


        /// <summary>
        /// Gets the station between a start and a end station in correct order
        /// </summary>
        public static List<Station> getBetweenStation(Station startStation, Station endStation, Line line)
        {
            int enterStationIndex = Array.IndexOf(line.stations, startStation);
            int exitStationIndex = Array.IndexOf(line.stations, endStation);
            bool drivingForward = enterStationIndex < exitStationIndex;

            List<Station> stations = new List<Station>();
            if (drivingForward)
            {
                for (int i = enterStationIndex; i < exitStationIndex + 1; i++)
                {
                    stations.Add(line.stations[i]);
                }
                return stations;
            }

            else
            {
                for (int i = enterStationIndex; i >= exitStationIndex; i--)
                {
                    stations.Add(line.stations[i]);
                }
                return stations;
            }
        }
    }
}