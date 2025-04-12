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


        /// <summary>
        /// Copies all metadata from original line except transit info and stations
        /// </summary>
        public Line(Line originalLine, TransitInfo transitInfo, Station[] newStations)
        {
            id = originalLine.id;
            name = originalLine.name;
            disassembledName = originalLine.disassembledName;
            number = originalLine.number;
            iconID = originalLine.iconID;
            stations = newStations;
            color = originalLine.color;
            this.transitInfo = transitInfo;
            geoData = originalLine.geoData;
        }

        /// <summary>
        /// Creates a copy of the original line
        /// </summary>
        public Line(Line originalLine)
        {
            id = originalLine.id;
            name = originalLine.name;
            disassembledName = originalLine.disassembledName;
            number = originalLine.number;
            iconID = originalLine.iconID;
            stations = originalLine.stations;
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
        /// Flips the stations
        /// </summary>
        public void flipStations()
        {
            Array.Reverse(stations);
        }

        /// <summary>
        /// Flips geo data 
        /// </summary>
        public void flipGeoData()
        {
            geoData.coordinates.Reverse();
        }


        public override string ToString()
        {
            string str = "Name: " + name;
            str += "\nStart Station: " + LineManager.getStationFromId(transitInfo.startStationId).name;
            str += "\nEnd Station: " + LineManager.getStationFromId(transitInfo.destinationStartionId).name;
            str += "\nStations: ";
            foreach (Station station in stations)
            {
                str += "\n\t" + station.name;
            }
            return str;
        }
    }
}