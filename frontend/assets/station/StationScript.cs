using Godot;
using helper;
using shared;
using System;

namespace Stations;

public partial class StationScript : StaticBody3D
{
    public StationData Data { get; set; }

    public void Initialize(StationData data)
    {
        Data = data;
        Position = GeoMapper.LatLonToGameCoords(Data.Latitude, Data.Longitude);
    }
}
