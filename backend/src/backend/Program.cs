using json;
using Microsoft.VisualBasic;
using Pathfinding;
using Simulation;
using TrainLines;
using Trains;
using WS;
using System.Threading;
using System.Threading.Tasks;
using Robots;
LineManager.initialize();
TrainManager.initialize();
RobotManager.initialize();


// Create an instance of the WebSocketManager
WS.WebSocketManager webSocketManager = new WS.WebSocketManager(SimulationSettings.webSocketURL);
// Start the WebSocket server in a separate thread (asynchronous task)
Task.Run(() => webSocketManager.Start());
Console.WriteLine("WebSocket server started on " + SimulationSettings.webSocketURL);

SimulationManager.startSimulation();
Console.WriteLine("Stop");