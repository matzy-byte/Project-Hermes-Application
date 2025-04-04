

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;
using TrainLines;
namespace json
{
    public static class JSONReader
    {
        public static void Test()
        {
            Console.WriteLine("Test");
        }

        public static Station[] loadStations(string path)
        {
            //Read the json file into a string
            string jsonPath = getFullJsonPath(path);
            string jsonString = File.ReadAllText(jsonPath);
            //Load data in wrapper list
            List<StationWrapper> stationsWrapper = JsonConvert.DeserializeObject<List<StationWrapper>>(jsonString);

            //Check if actual data is loaded
            if (stationsWrapper == null || stationsWrapper.Count == 0)
                throw new Exception("Failed to load station from: " + jsonPath);

            //Convert wrapper list to object array
            Station[] stations = new Station[stationsWrapper.Count];
            for (int i = 0; i < stationsWrapper.Count; i++)
            {
                stations[i] = new Station(stationsWrapper[i]);
            }

            //Print log and return stations
            Console.WriteLine("Number of loaded Stations: " + stations.Length);
            return stations;
        }

        public static Line[] loadLines(string path, Station[] allStations)
        {
            //Read the json file into a string
            string jsonPath = getFullJsonPath(path);
            string jsonString = File.ReadAllText(jsonPath);
            //Load data in wrapper list
            RootObjectWrapper rootObjectWrapper = JsonConvert.DeserializeObject<RootObjectWrapper>(jsonString);

            //Check if actual data is loaded
            if (rootObjectWrapper.Lines.Count == 0)
                throw new Exception("Failed to load Lines from: " + jsonPath);

            //Loop over all lines and create the line Objects
            Line[] lines = new Line[rootObjectWrapper.Lines.Count];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = new Line(rootObjectWrapper.Lines[i], allStations);
            }

            //Print log and return Lines
            Console.WriteLine("Number of loaded Lines: " + lines.Length);
            return lines;
        }
        public static GeoData[] loadGeoData(string path)
        {
            //Read the json file into a string
            string jsonPath = getFullJsonPath(path);
            string jsonString = File.ReadAllText(jsonPath);
            //Load data in wrapper list
            FeatureCollectionWrapper featureCollectionWrapper = JsonConvert.DeserializeObject<FeatureCollectionWrapper>(jsonString);

            //Check if actual data is loaded
            if (featureCollectionWrapper.Features.Count == 0)
                throw new Exception("Failed to load GeoData from: " + jsonPath);

            //Loop over all Features and create the GeoData Objects
            GeoData[] geoDatas = new GeoData[featureCollectionWrapper.Features.Count];
            for (int i = 0; i < geoDatas.Length; i++)
            {
                geoDatas[i] = new GeoData(featureCollectionWrapper.Features[i].Properties, featureCollectionWrapper.Features[i].Geometry);
            }

            //Print log and return Lines
            Console.WriteLine("Number of loaded GeoData Lines: " + geoDatas.Length);
            return geoDatas;
        }

        public static TransitInfo[] loadTransitInfo(string path)
        {
            //Read the json file into a string
            string jsonPath = getFullJsonPath(path);
            string jsonString = File.ReadAllText(jsonPath);
            //Load data in wrapper list
            List<TransitInfoWrapper> transitInfoWrappes = JsonConvert.DeserializeObject<List<TransitInfoWrapper>>(jsonString);

            //Check if actual data is loaded
            if (transitInfoWrappes == null || transitInfoWrappes.Count == 0)
                throw new Exception("Failed to load GeoData from: " + jsonPath);

            //Loop over all Features and create the GeoData Objects
            TransitInfo[] transitInfos = new TransitInfo[transitInfoWrappes.Count];
            for (int i = 0; i < transitInfos.Length; i++)
            {
                transitInfos[i] = new TransitInfo(transitInfoWrappes[i]);
            }

            //Print log and return Lines
            Console.WriteLine("Number of loaded Transit Infos: " + transitInfoWrappes.Count);
            return transitInfos;
        }

        private static string getFullJsonPath(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return fullPath.Replace('\\', '/');
        }
    }


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
    public class GeometryWrapper
    {
        public string Type { get; set; }
        public List<List<double>> Coordinates { get; set; }
    }

    /// <summary>
    /// Wrapper for loading KVV line geo data Properties Object
    /// </summary>
    public class PropertiesWrapper
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }
    }

    /// <summary>
    /// Wrapper for loading KVV line geo data Feauture Object
    /// </summary>
    public class FeatureWrapper
    {
        public string Type { get; set; }
        public GeometryWrapper Geometry { get; set; }
        public PropertiesWrapper Properties { get; set; }
    }

    /// <summary>
    /// Wrapper for loading KVV line geo data Root Object
    /// </summary>
    public class FeatureCollectionWrapper
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
        public string DestinationStationID { get; set; }
        public string DestinationStationName { get; set; }
        public string LineNameAbreviation { get; set; }
        public int TravelTime { get; set; }
        public int TravelTimeReverse { get; set; }
    }
}

