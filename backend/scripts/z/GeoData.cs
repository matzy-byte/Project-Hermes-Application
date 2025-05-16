namespace Z;

public class GeoData(PropertiesWrapper propertiesWrapper, GeometryWrapper geometryWrapper)
{
    public string Name { get; set; } = propertiesWrapper.Name;
    public List<Coordinate> Coordinates { get; set; } = ExtractCoordiantes(geometryWrapper);


    //Extract the Coodiantes from the json wrapper
    // idex [0] = Longetude and index[i]= latitude
    private static List<Coordinate> ExtractCoordiantes(GeometryWrapper geometryWrapper)
    {
        List<Coordinate> coordinates = [];
        foreach (List<double> entry in geometryWrapper.Coordinates)
        {
            coordinates.Add(new Coordinate(entry[1], entry[0]));
        }

        return coordinates;
    }
}