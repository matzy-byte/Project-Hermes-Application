using json;
namespace TrainLines
{
    public class Station
    {
        public string name;
        public string triasID;
        public string triasName;
        public Coordinate coordPositionWGS84;

        public Station(StationWrapper stationWrapper)
        {
            name = stationWrapper.Name;
            triasID = stationWrapper.TriasID;
            triasName = stationWrapper.TriasName;
            coordPositionWGS84 = new Coordinate(stationWrapper.CoordPositionWGS84);
        }


        public string getStationJSON()
        {
            string str = "{\n";
            str += "\"StationID\" : " + "\"" + triasID + "\",\n";
            str += "\"StationName\" : " + "\"" + name + "\",\n";
            str += "\"Long\" : " + coordPositionWGS84.longetude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
            str += "\"Lat\" : " + coordPositionWGS84.latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";
            str += "}";
            return str;
        }
    }
}