using Simulation;
using Json;
using Trains;
using Robots;
using Logs;

DataLogger.Initialize();

// Create an instance of the WebSocketManager
Websocket.WebSocketManager webSocketManager = new Websocket.WebSocketManager(SimulationSettingsGlobal.WebSocketURL);
// Start the WebSocket server in a separate thread (asynchronous task)
Task.Run(() => webSocketManager.Start());
Console.WriteLine("WebSocket server started on " + SimulationSettingsGlobal.WebSocketURL);
DataLogger.AddLog("WebSocket server started on " + SimulationSettingsGlobal.WebSocketURL);

DataManager.LoadDataFromJson();
SimulationSettings.Initialize();
TrainManager.Initialize();
RobotManager.Initialize();
SimulationManager.SimulationLoop();