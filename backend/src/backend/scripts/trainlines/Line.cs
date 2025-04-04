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

        public Line(LineWrapper lineWrapper)
        {
            id = lineWrapper.Id;
            name = lineWrapper.Name;
            disassembledName = lineWrapper.DisassembledName;
            number = lineWrapper.Number;
            iconID = lineWrapper.IconID;
            stations = extracUsedStations(lineWrapper.Stations);
            color = lineWrapper.Color;
        }

        public Line(Line originalLine, Station[] newStations)
        {
            id = originalLine.id;
            name = originalLine.name;
            disassembledName = originalLine.disassembledName;
            number = originalLine.number;
            iconID = originalLine.iconID;
            stations = newStations;
            color = originalLine.color;
            transitInfo = originalLine.transitInfo;
            geoData = originalLine.geoData;
        }

        private Station[] extracUsedStations(List<string> usedStationsIds)
        {
            Station[] usedStations = new Station[usedStationsIds.Count];

            //Find all the sations used and add to array
            for (int i = 0; i < usedStationsIds.Count; i++)
            {
                //Search all stations for name match
                usedStations[i] = LineManager.getStationFromId(usedStationsIds[i]);
            }

            return usedStations;
        }


        /// <summary>
        /// Flips the stations for lines where the train begins in the end station
        /// </summary>
        public void flipLine()
        {
            Array.Reverse(stations);
        }
    }
}