using json;
namespace TrainLines
{
    public class GeoData
    {
        public string name;
        public Coordinate[] coordinates;


        public GeoData(PropertiesWrapper propertiesWrapper, GeometryWrapper geometryWrapper)
        {
            name = propertiesWrapper.Name;
            coordinates = extractCoordiantes(geometryWrapper);
        }


        //Extract the Coodiantes from the json wrapper
        // idex [0] = Longetude and index[i]= latitude
        private Coordinate[] extractCoordiantes(GeometryWrapper geometryWrapper)
        {
            Coordinate[] coordiantes = new Coordinate[geometryWrapper.Coordinates.Count];

            for (int i = 0; i < geometryWrapper.Coordinates.Count; i++)
            {
                coordiantes[i] = new Coordinate(geometryWrapper.Coordinates[i][1], geometryWrapper.Coordinates[i][0]);
            }

            return coordiantes;
        }


        public string geoDatJSON()
        {
            string str = "\"GeoData\" : [";

            foreach (Coordinate coordinate in coordinates)
            {
                str += "\n{\n";
                str += "\"Long\" : " + coordinate.longetude.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",\n";
                str += "\"Lat\" : " + coordinate.latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\n";

                str += "}";
                if (coordinate != coordinates.Last())
                {
                    str += ",";
                }
                str += "\n";
            }
            str += "]\n";

            return str;
        }
    }
}
