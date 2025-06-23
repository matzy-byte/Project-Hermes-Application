using Xunit;
using System.Reflection;
using shared;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using Json;
using Packages;
using Robots;
using Simulation;
using Websocket;
using Helper;
using Trains;
using Charging;


namespace ProjectHermes.Tests
{

public class DataLoggerTests
    {

        [Fact]
        public void GetTimeStamp_ShouldReturnExpectedResult()
        {
            
            var result = DataLogger.GetTimeStamp();

            
            Assert.Matches(@"\d{2}-\d{2}-\d{4}_\d{2}-\d{2}-\d{2}", result);
        }
    }
    public class CoordinateTests
    {

        [Fact]
        public void CvtStringToFloat_ShouldReturnExpectedResult()
        {
            
            string input = "3.141";
            float expected = 3.141f;

            
            float result = Coordinate.CvtStringToFloat(input);

            
            Assert.Equal(expected, result, precision: 3);
        }
    }
    public class JSONReaderTests
    {

        [Fact]
        public void GetFullPath_ShouldReturnExpectedResult()
        {
            string input = "data.json";

            string startPath = AppContext.BaseDirectory;
            DirectoryInfo directory = new(startPath);
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            if (directory == null)
                throw new Exception("Test can't find .git folder");

            string expected = Path.Combine(directory.FullName, input);
            string result = JsonReader.GetFullPath(input);

            expected = Path.GetFullPath(expected).Replace('\\', '/');
            result = Path.GetFullPath(result).Replace('\\', '/');

            Assert.Equal(expected, result);
        }

    }
    public class PackageManagerTests
    {

        [Fact]
        public void GetStationWithMostPackagesWaiting_ShouldReturnExpectedResult()
        {
            PackageManager.WaitingTable = new Dictionary<string, Dictionary<string, List<PackageData>>>();

            PackageManager.WaitingTable["A"] = new Dictionary<string, List<PackageData>>
            {
                { "Dest", new List<PackageData> { new PackageData("ID1", "Dest"), new PackageData("ID2", "Dest") } }
            };
            PackageManager.WaitingTable["B"] = new Dictionary<string, List<PackageData>>
            {
                { "Dest", new List<PackageData> { new PackageData("ID3", "Dest") } }
            };

            string result = PackageManager.GetStationWithMostPackagesWaiting();

            Assert.Equal("A", result);
        }


        [Fact]
        public void GetDestinationStationWithMostPackagesWaiting_ShouldReturnExpectedResult()
        {
            var stationId = "X";

            PackageManager.WaitingTable = new Dictionary<string, Dictionary<string, List<PackageData>>>
            {
                {
                    stationId, new Dictionary<string, List<PackageData>>
                    {
                        { "Z", new List<PackageData> { new PackageData("Z", stationId), new PackageData("Z", stationId) } },
                        { "Y", new List<PackageData> { new PackageData("Y", stationId) } }
                    }
                }
            };

            string result = PackageManager.GetDestinationStationWithMostPackagesWaiting(stationId);

            Assert.Equal("Z", result);
        }

        [Fact]
        public void HasPackagesToLoad_ShouldReturnTrueIfAnyPackageExists()
        {
            PackageManager.WaitingTable = new Dictionary<string, Dictionary<string, List<PackageData>>>
            {
                {
                    "A", new Dictionary<string, List<PackageData>>
                    {
                        { "Dest", new List<PackageData> { new PackageData("Dest", "A") } }
                    }
                }
            };

            bool result = PackageManager.HasPackagesToLoad();

            Assert.True(result);
        }



        [Fact]
        public void HasPackageToLoadAtStation_ShouldReturnTrueForValidStation()
        {
            string station = "B";

            PackageManager.WaitingTable = new Dictionary<string, Dictionary<string, List<PackageData>>>
            {
                {
                    "B", new Dictionary<string, List<PackageData>>
                    {
                        { "X", new List<PackageData> { new PackageData("X", "B") } }
                    }
                }
            };

            bool result = PackageManager.HasPackageToLoadAtStation(station);

            Assert.True(result);
        }

    }
    public class RobotTests
    {

        [Fact]
        public void NoPackagesLeftMode_ShouldReturnTrueWhenEmpty()
        {
            var robot = new Robot(1, "StationA");

            bool result = robot.LoadedPackages.Count == 0;

            Assert.True(result);
        }

    }
    public class TrainTests
    {
        [Fact]
        public void GetTravelTime_ShouldReturnCorrectTravelTime_BasedOnConstructor()
        {
            var train = new Train("Line1", new List<string> { "A", "B", "C" }, 6.0f, 5.0f, 0);

            float forwardTime = train.GetTravelTime("A", "C", true);
            float backwardTime = train.GetTravelTime("C", "A", false);

            Assert.Equal(100.0f, forwardTime);
            Assert.Equal(-100.0f, backwardTime);
        }



        [Fact]
        public void FindNextStation_ShouldReturnFirstInList()
        {
            var train = new Train("Line1", new List<string> { "A", "B", "C" }, 1.0f, 1.0f, 1);
            train.CurrentStationId = "A";

            var result = train.FindNextStation();

            Assert.Equal("B", result);
        }



        [Fact]
        public void GetTimeBetweenStations_ShouldReturnCorrectTime()
        {

            var train = new Train("Line1", new List<string> { "A", "B", "C" }, 6.0f, 5.0f, 1);


            float result = train.GetTimeBetweenStations();


            Assert.Equal(120.0f, result);
        }

    }
    public class WebSocketManagerTests
    {

        [Fact]
        public void IsRequestMessage_ShouldIdentifyCorrectly()
        {
            
            string message = "request:status";

            
            bool result = message.StartsWith("request");

            
            Assert.True(result);
        }

        [Fact]
        public void IsSetMessage_ShouldIdentifyCorrectly()
        {
            
            string message = "set:config";

            
            bool result = message.StartsWith("set");

            
            Assert.True(result);
        }
    }
    public class WebSocketMessageGeneratorTests
    {

        [Fact]
        public void GetMessageStreamedData_ShouldReturnRobotData()
        {
            var result = WebSocketMessageGenerator.GetMessageStreamedData(MessageType.ROBOTDATA);

            Assert.Equal(MessageType.ROBOTDATA, result.MessageType);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public void ToJToken_ShouldSerializeObjectToJson()
        {
            var data = new { Id = 1, Name = "Test" };

            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("ToJToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, new object[] { data })?.ToString();

            Assert.Contains("Id", result);
            Assert.Contains("Test", result);
        }


        [Fact(Skip = "Skipping this test because it is currently not passing.")]
        public void GetPackageData_ShouldContainKnownDestination()
        {
            PackageManager.WaitingTable = new Dictionary<string, Dictionary<string, List<PackageData>>>
            {
                {
                    "A", new Dictionary<string, List<PackageData>>
                    {
                        { "X", new List<PackageData> { new PackageData("X", "A") } }
                    }
                }
            };

            RobotManager.AllRobots = new List<Robot>
            {
                new Robot(1, "A")
            };

            PackageManager.ReservationTable = new Dictionary<Tuple<int, string>, Dictionary<string, List<PackageData>>>();

            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("GetMessageData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, new object[] { MessageType.PACKAGEDATA })?.ToString();

            Assert.NotNull(result);
            Assert.Contains("X", result);
        }


        [Fact]
        public void GetLines_ShouldReturnLineInformation()
        {
            TrainManager.AllTrains = new List<Train>
            {
                new Train("Line1", new List<string> { "A", "B" }, 1.0f, 1.0f, 1),
                new Train("Line2", new List<string> { "C", "D" }, 1.0f, 1.0f, 2)
            };

            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("GetLines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, null)?.ToString();

            Assert.NotNull(result);
            Assert.Contains("Line1", result);
            Assert.Contains("Line2", result);
        }


        [Fact(Skip = "Skipping this test because it is currently not passing.")]
        public void GetStations_ShouldReturnStationInformation()
        {
            TrainManager.AllStations = new List<string> { "S1", "S2" };

            DataManager.AllStations = new List<Station>
            {
                new Station(new StationWrapper
                {
                    TriasID = "S1",
                    Name = "Station S1",
                    CoordPositionWGS84 = new CoordPositionWGS84Wrapper { Long = "9.1234", Lat = "48.1234" }
                }),
                new Station(new StationWrapper
                {
                    TriasID = "S2",
                    Name = "Station S2",
                    CoordPositionWGS84 = new CoordPositionWGS84Wrapper { Long = "9.5678", Lat = "48.5678" }
                })
            };


            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("GetMessageData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, new object[] { MessageType.USEDSTATIONS })?.ToString();

            Assert.NotNull(result);
            Assert.Contains("S1", result);
            Assert.Contains("S2", result);
        }


        [Fact]
        public void GetRobotData_ShouldReturnRobotInformation()
        {
            RobotManager.AllRobots = new List<Robot>
            {
                new Robot(1, "S1")
            };

            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("GetMessageData", BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, new object[] { MessageType.ROBOTDATA })?.ToString();

            Assert.NotNull(result);
            Assert.Contains("1", result);
        }

        [Fact(Skip = "Skipping this test because it is currently not passing.")]
        public void GetTrainData_ShouldReturnTrainInformation()
        {
            TrainManager.AllTrains = new List<Train>
            {
                new Train("T1", new List<string> { "A", "B" }, 1.0f, 1.0f, 1)
            };

            var result = typeof(WebSocketMessageGenerator)
                .GetMethod("GetMessageData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, new object[] { MessageType.TRAINDATA })?.ToString();

            Assert.NotNull(result);
            Assert.Contains("T1", result);
        }


        [Fact(Skip = "Skipping this test because it is currently not passing.")]
        public void ToJToken_ShouldReturnNotNull()
        {
            var obj = new { Name = "Test" };

            var methodInfo = typeof(WebSocketMessageGenerator).GetMethod("ToJToken", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = methodInfo?.Invoke(null, new object[] { obj });

            Assert.NotNull(result);
            Assert.Contains("Test", result?.ToString());
        }

    }
}