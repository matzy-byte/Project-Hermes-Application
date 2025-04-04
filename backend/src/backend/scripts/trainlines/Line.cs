using json;
namespace TrainLines
{
    public class Line
    {
        public string id;
        public string name;
        public string disassembledName;
        public string number;
        public string iconID;
        public Station[] stations;
        public string color;

        public GeoData geoData = null;
        public TransitInfo transitInfo = null;

        public Line(LineWrapper lineWrapper, Station[] allStations)
        {
            id = lineWrapper.Id;
            name = lineWrapper.Name;
            disassembledName = lineWrapper.DisassembledName;
            number = lineWrapper.Number;
            iconID = lineWrapper.IconID;
            stations = extracUsedStations(allStations, lineWrapper.Stations);
            color = lineWrapper.Color;
        }

        private Station[] extracUsedStations(Station[] allStations, List<string> usedStationsIds)
        {
            Station[] usedStations = new Station[usedStationsIds.Count];

            //Find all the sations used and add to array
            for (int i = 0; i < usedStationsIds.Count; i++)
            {
                //Search all stations for name match
                foreach (Station station in allStations)
                {
                    if (station.triasID == usedStationsIds[i])
                    {
                        usedStations[i] = station;
                        break;
                    }

                    if (station == allStations.Last())
                    {
                        throw new Exception("Cant find Matching Station: " + usedStationsIds[i] + " from Line: " + name);
                    }
                }
            }

            return usedStations;
        }
    }
}