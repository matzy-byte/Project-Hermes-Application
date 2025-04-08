using json;
using Microsoft.VisualBasic;
using Pathfinding;
using Simulation;
using TrainLines;
using Trains;

LineManager.initialize();
TrainManager.initialize();


PathfindingManager.initializePathFinding();

Station startStation = LineManager.getStationFromId("de:08212:18");
Station endStation = LineManager.getStationFromId("de:08212:1001");
var path = PathfindingManager.getAllTravelPaths(startStation, endStation, 20);
var path2 = PathfindingManager.getAllTravelPaths(startStation, endStation, 1000);

SimulationManager.startSimulation();
Console.WriteLine("Stop");