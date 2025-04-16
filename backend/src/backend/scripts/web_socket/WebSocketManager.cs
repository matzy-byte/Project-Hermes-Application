using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Robots;
using Simulation;
using Trains;
namespace WS
{
    public class WebSocketManager
    {
        private readonly HttpListener _listener = new HttpListener();

        public WebSocketManager(string url)
        {
            _listener.Prefixes.Add(url); // e.g. "http://localhost:5000/ws/"
        }

        public void Start()
        {
            _listener.Start();
            Task.Run(() => AcceptLoop());
        }


        /// <summary>
        /// Background task to manage connecting clients
        /// </summary>
        private async Task AcceptLoop()
        {
            while (true)
            {
                var context = await _listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleClient(wsContext.WebSocket); // Fire-and-forget
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }


        /// <summary>
        /// When client connects it creates two taks. One for managing
        /// request and one for streaming data
        /// </summary>
        private async Task HandleClient(WebSocket socket)
        {
            var receiveTask = Task.Run(() => HandleIncomingMessages(socket));
            var streamTask = Task.Run(() => StreamLoop(socket));

            await Task.WhenAll(receiveTask, streamTask);
        }


        /// <summary>
        /// Manages incoming messages in a seperate thread
        /// </summary>
        private async Task HandleIncomingMessages(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                catch
                {
                    break; // Client disconnected
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }

                //Convert request to a string
                string request = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //Get the response json string
                string responseJSON = HandleRequest(request);
                //Convert the response string to a buffer
                var responseBytes = Encoding.UTF8.GetBytes(responseJSON);
                //Send the buffer to the client
                await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }


        /// <summary>
        /// Streams data contiunously to client
        /// </summary>
        private async Task StreamLoop(WebSocket socket)
        {
            while (socket.State == WebSocketState.Open)
            {
                string trainPositionsJSON = TrainManager.getTrainPositionsJSON();
                string robotPositionJSON = RobotManager.getRobotDataJSON();
                //Convert the json string to a buffer
                var bufferTrain = Encoding.UTF8.GetBytes(trainPositionsJSON);
                var bufferRobot = Encoding.UTF8.GetBytes(robotPositionJSON);

                try
                {
                    //Send the data
                    await socket.SendAsync(new ArraySegment<byte>(bufferTrain), WebSocketMessageType.Text, true, CancellationToken.None);
                    await Task.Delay(5);
                    await socket.SendAsync(new ArraySegment<byte>(bufferRobot), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                //Delay between transmissions
                await Task.Delay(SimulationSettings.dataStreamDelay);
            }
        }

        private string HandleRequest(string request)
        {
            Console.WriteLine("Request: " + request);
            try
            {
                string response;
                switch (request)
                {
                    case "getUsedStations":
                        response = TrainManager.getUsedStationsJSON();
                        break;
                    case "getTrainLines":
                        response = TrainManager.getTrainLinesJSON();
                        break;
                    case "getTrainStations":
                        response = TrainManager.getTrainStationsJSON();
                        break;
                    case "getTrainGeoData":
                        response = TrainManager.getTrainGeoDataJSON();
                        break;
                    default:
                        response = "Invalid request";
                        break;
                }

                return response;
            }
            catch
            {
                return "Invalid request";
            }
        }
    }
}