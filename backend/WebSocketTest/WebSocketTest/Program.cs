using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Uri serverUri = new Uri("ws://localhost:5000/ws/"); // <- your server address
            using var client = new ClientWebSocket();

            Console.WriteLine("Connecting to server...");
            await client.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected!");

            // Start receiving streamed messages in the background
            _ = Task.Run(() => ReceiveMessages(client));

            // Test all the Request MessageTypes
            await SendRequest(client, 100, MessageType.TRAINLINES);
            await Task.Delay(500);
            await SendRequest(client, 101, MessageType.TRAINSTATIONSINLINE);
            await Task.Delay(500);
            await SendRequest(client, 102, MessageType.USEDSTATIONS);
            await Task.Delay(500);
            await SendRequest(client, 103, MessageType.TRAINGEODATA);
            await Task.Delay(500);
            await SendRequest(client, 104, MessageType.PACKAGEDATA);
            await Task.Delay(500);
            await SendRequest(client, 105, MessageType.SIMULATIONSTATE);
            await Task.Delay(500);

            // Also test simple control commands: Start, Pause, Continue, Stop
            // await SendControlCommand(client, 200, MessageType.STARTSIMULATION);
            await Task.Delay(1000); // Let some simulation happen
                                    //  await SendControlCommand(client, 201, MessageType.PAUSESIMULATION);
            await Task.Delay(1000);
            //  await SendControlCommand(client, 202, MessageType.CONTINUESIMULATION);
            await Task.Delay(1000);
            //  await SendControlCommand(client, 203, MessageType.STOPSIMULATION);

            Console.WriteLine("Testing done. Press any key to exit...");
        }

        static async Task SendRequest(ClientWebSocket client, int id, MessageType type)
        {
            var message = new WebSocketMessage
            {
                id = id,
                messageType = type,
                data = JsonDocument.Parse("{}").RootElement.Clone()
            };

            string json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"Sent request: {type}");
        }

        static async Task SendControlCommand(ClientWebSocket client, int id, MessageType type)
        {
            var message = new WebSocketMessage
            {
                id = id,
                messageType = type,
                data = JsonDocument.Parse("{}").RootElement.Clone()
            };

            string json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"Sent control command: {type}");
        }

        static async Task ReceiveMessages(ClientWebSocket client)
        {
            var buffer = new byte[4096];

            while (client.State == WebSocketState.Open)
            {
                try
                {
                    var messageBuffer = new List<byte>();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            Console.WriteLine("Server closed connection");
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                            return;
                        }

                        messageBuffer.AddRange(buffer.Take(result.Count));

                    } while (!result.EndOfMessage);

                    // Full message assembled
                    string messageString = Encoding.UTF8.GetString(messageBuffer.ToArray());

                    WebSocketMessage message = convertIncomingMessage(messageString);

                    if (message.messageType != MessageType.TRAINDATA && message.messageType != MessageType.ROBOTDATA)
                    {

                        Console.WriteLine($"Received complete message: {messageString}");

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }



        private static WebSocketMessage convertIncomingMessage(string message)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<WebSocketMessage>(message, options);
        }


        public enum MessageType
        {
            //Streamed Data
            ROBOTDATA,
            TRAINDATA,

            //Requested Data
            TRAINLINES,
            TRAINSTATIONSINLINE,
            USEDSTATIONS,
            TRAINGEODATA,
            PACKAGEDATA,
            SIMULATIONSTATE,

            //Set Data
            SETTINGS,
            SETSIMULATIONSPEED,
            STARTSIMULATION,
            STOPSIMULATION,
            PAUSESIMULATION,
            CONTINUESIMULATION
        }

        public class WebSocketMessage
        {
            public int id { get; set; }
            public MessageType messageType { get; set; }
            public JsonElement data { get; set; }
        }
    }
}
