using Godot;

public static class GeoMapper
{
    // Geographic bounds (A) // Measured: MinLon: 8,28327, MaxLon: 8,50511, MinLat: 48,80172, MaxLat: 49,13373
    private const double MinLon = 8.2;
    private const double MaxLon = 8.7;
    private const double MinLat = 48.7;
    private const double MaxLat = 49.2;

    // Game space bounds (B)
    private const float MinX = 600f;
    private const float MaxX = -600f;
    private const float MinZ = -600;
    private const float MaxZ = 600;

    public static Vector3 LatLonToGameCoords(double lat, double lon)
    {
        float x = Remap((float)lon, (float)MinLon, (float)MaxLon, MinX, MaxX);
        float z = Remap((float)lat, (float)MinLat, (float)MaxLat, MinZ, MaxZ);

        return new Vector3(x, 0f, z);
    }

    private static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}