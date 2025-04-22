using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static ClientWebSocket clientWebSocket = new ClientWebSocket();
    private static int requestId = 0;


    /// <summary>
    /// Program entry point
    /// </summary>
    static async Task Main(string[] args)
    {
        string url = "ws://localhost:5000/ws/";
        await StartWebSocketClient(url);
    }


    /// <summary>
    /// connects client with server
    /// </summary>
    public static async Task StartWebSocketClient(string serverUri)
    {
        try
        {
            await clientWebSocket.ConnectAsync(new Uri(serverUri), CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server.");

            //Start thread for streamed dat and requesting data
            Task streamTask = Task.Run(() => StreamData());
            Task requestDataTask = Task.Run(() => RequestDataLoop());

            await Task.WhenAll(streamTask, requestDataTask);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        //Close the connection
        finally
        {
            if (clientWebSocket.State == WebSocketState.Open)
            {
                await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                Console.WriteLine("WebSocket connection closed.");
            }
        }
    }


    /// <summary>
    /// Thread for working with streamed data
    /// </summary>
    private static async Task StreamData()
    {
        var buffer = new byte[1024];
        StringBuilder messageBuilder = new StringBuilder(); // To accumulate the message as it arrives

        while (clientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count)); // Append the received data to the builder

                if (result.EndOfMessage) // If the message is complete
                {
                    string completeMessage = messageBuilder.ToString();
                    messageBuilder.Clear(); // Clear the builder for the next message

                    // Handle as streamed data
                    if (completeMessage.Contains("TrainPositions"))
                    {
                        //Streamed Train positions
                        //Console.WriteLine("Streamed Train Positons \t Message Length: " + completeMessage.Length);
                    }
                    else if (completeMessage.Contains("TrainLines"))
                    {
                        //train line Request
                        Console.WriteLine("Requested TrainLine \t Message Length: " + completeMessage.Length);
                    }
                    else if (completeMessage.Contains("StationsInLine"))
                    {
                        //Stations in line
                        Console.WriteLine("Requested StationInLine \t Message Length: " + completeMessage.Length);
                    }
                    else if (completeMessage.Contains("UsedStations"))
                    {
                        //Used Stations
                        Console.WriteLine("Requested UsedStations \t Message Length: " + completeMessage.Length);
                    }
                    else if (completeMessage.Contains("TrainGeoData"))
                    {
                        //Geo Data
                        Console.WriteLine("Requested TrainGeoData \t Message Length: " + completeMessage.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StreamData: {ex.Message}");
            }

            await Task.Delay(1); // Allow for other tasks to execute
        }
    }




    /// <summary>
    /// Thread to request data 
    /// </summary>
    private static async Task RequestDataLoop()
    {
        while (clientWebSocket.State == WebSocketState.Open)
        {
            try
            {
                switch (requestId)
                {
                    case 0:
                        await RequestUsedStations();
                        break;
                    case 1:
                        await RequestTrainLines();
                        break;
                    case 2:
                        await RequestTrainStations();
                        break;
                    case 3:
                        await RequestTrainGeoData();
                        break;
                }

                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RequestDataLoop: {ex.Message}");
            }

            requestId++;
            requestId = requestId >= 5 ? 0 : requestId;
        }
    }



    /// <summary>
    /// Request usedStations
    /// </summary>
    private static async Task RequestUsedStations()
    {
        try
        {
            string requestMessage = "getUsedStations"; // Send only the command
            byte[] messageBytes = Encoding.UTF8.GetBytes(requestMessage);

            await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent request: {requestMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RequestUsedStations: {ex.Message}");
        }
    }




    /// <summary>
    /// Request Train Lines
    /// </summary>
    private static async Task RequestTrainLines()
    {
        try
        {
            string requestMessage = $"getTrainLines";
            byte[] messageBytes = Encoding.UTF8.GetBytes(requestMessage);

            await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent request: {requestMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RequestTrainLines: {ex.Message}");
        }
    }


    /// <summary>
    /// Request Train Stations
    /// </summary>
    private static async Task RequestTrainStations()
    {
        try
        {
            string requestMessage = $"getTrainStations";
            byte[] messageBytes = Encoding.UTF8.GetBytes(requestMessage);

            await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent request: {requestMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RequestTrainLines: {ex.Message}");
        }
    }


    /// <summary>
    /// Request Train geo data
    /// </summary>
    private static async Task RequestTrainGeoData()
    {
        try
        {
            string requestMessage = $"getTrainGeoData";
            byte[] messageBytes = Encoding.UTF8.GetBytes(requestMessage);

            await clientWebSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Sent request: {requestMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RequestTrainLines: {ex.Message}");
        }
    }
}
