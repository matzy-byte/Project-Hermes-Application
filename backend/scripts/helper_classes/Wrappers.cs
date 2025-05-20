namespace Helper ;

/// <summary>
/// Wrapper for loading positions of the Stations
/// </summary>
public struct CoordPositionWGS84Wrapper
{
    public string Lat { get; set; }
    public string Long { get; set; }
}

/// <summary>
/// Wrapper for loading the Stations
/// </summary>
public struct StationWrapper
{
    public string Name { get; set; }
    public string TriasID { get; set; }
    public string TriasName { get; set; }
    public CoordPositionWGS84Wrapper CoordPositionWGS84 { get; set; }
}

/// <summary>
/// Wrapper for loading the Lines
/// </summary>
public struct LineWrapper
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string DisassembledName { get; set; }
    public string Number { get; set; }
    public string IconID { get; set; }
    public List<string> Stations { get; set; }
    public string Color { get; set; }
}


/// <summary>
/// Wrapper for loading the line root object
/// </summary>
public struct RootObjectWrapper
{
    public List<LineWrapper> Lines { get; set; }
}


/// <summary>
/// Wrapper for loading KVV line geo data Geometry Object
/// </summary>
public struct GeometryWrapper
{
    public string Type { get; set; }
    public List<List<double>> Coordinates { get; set; }
}

/// <summary>
/// Wrapper for loading KVV line geo data Properties Object
/// </summary>
public struct PropertiesWrapper
{
    public string Name { get; set; }
    public string ColorCode { get; set; }
}

/// <summary>
/// Wrapper for loading KVV line geo data Feauture Object
/// </summary>
public struct FeatureWrapper
{
    public string Type { get; set; }
    public GeometryWrapper Geometry { get; set; }
    public PropertiesWrapper Properties { get; set; }
}

/// <summary>
/// Wrapper for loading KVV line geo data Root Object
/// </summary>
public struct FeatureCollectionWrapper
{
    public string Type { get; set; }
    public List<FeatureWrapper> Features { get; set; }
}

/// <summary>
/// Wrapper for loading Transit Informations
/// </summary>
public struct TransitInfoWrapper
{
    public string LineDataName { get; set; }
    public string LineName { get; set; }
    public string StartStationID { get; set; }
    public string StartStationName { get; set; }
    public string DestinationID { get; set; }
    public string DestinationName { get; set; }
    public string LineNameAbreviation { get; set; }
    public int TravelTime { get; set; }
    public int TravelTimeReverse { get; set; }
}