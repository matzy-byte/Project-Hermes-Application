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
    }
}
