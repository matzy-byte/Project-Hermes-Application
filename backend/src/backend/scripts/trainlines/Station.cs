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
    }
}