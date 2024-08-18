using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using UnityEngine;
using System.Data.Common;
using System.IO;

public static class TcpClientExtentions
{
    public static async Task<Message> SendMessageResponse(this TcpClient client, Message message, CancellationToken token = default)
    {   
        if(client == null || (client != null && (!client.Connected || client.GetStream() == null))) return new Message(MessageType.Error, "Client is not connected");

        try
        {
            client.SendMessage(message, token);
        } 
        catch (TaskCanceledException) 
        {
            return new Message(MessageType.Error, "Task was canceled");
        }

        // Receive the response from the server.
        var timeoutToken = new CancellationTokenSource(5000).Token;
        Console.WriteLine("Waiting for response");
        Message response = await client.ReceiveMessage(timeoutToken);
        Console.WriteLine("Received response");

        return response;
    }

    public static async void SendMessage(this TcpClient client, Message message, CancellationToken token = default)
    {   
        if(client == null || (client != null && (!client.Connected || client.GetStream() == null))) return;

        // Send the message to the server.
        NetworkStream stream = client.GetStream();
        Debug.Log("Sending message: " + message.ToString());
        byte[] msg = Encoding.UTF8.GetBytes(message.Serialize());
        
        try
        {
            await stream.WriteAsync(BitConverter.GetBytes(msg.Length).Concat(msg).ToArray(), 0, msg.Length + 4, token);
        }
        catch (TaskCanceledException) 
        {
            return;
        }
    }

    public static async Task<Message> ReceiveMessage(this TcpClient client, CancellationToken token = default)
    {
        if(client == null || (client != null && (!client.Connected || client.GetStream() == null))) return new Message(MessageType.Error, "Client is not connected");


        // Receive the response from the server.
        NetworkStream stream = client.GetStream();
        byte[] received = new byte[1024];
        int read;

        try
        {
            await stream.ReadAsync(received, 0, 4, token);
            var len = BitConverter.ToInt32(received, 0);
            read = await stream.ReadAsync(received, 4, len, token);
        } 
        catch (TaskCanceledException)
        {
            return new Message(MessageType.Error);
        }

        string data = Encoding.UTF8.GetString(received, 4, read);
        //Debug.Log($"Received: {data}");
        Message? respOpt = Message.Deserialize(data);

        if (respOpt == null) return new Message(MessageType.Error, "Failed to deserialize message");

        Message response = respOpt.Value;

        Debug.Log($"Received: {response}");

        if(read == 0 || (response.IsDisconnect && response.username == "NULL" && response.gameCode == ""))
        {
            GameNetwork.instance.client.DisconnectLocal();
        }

        return response;
    }

    public static void Clear(this MemoryStream source)
    {
        byte[] buffer = source.GetBuffer();
        Array.Clear(buffer, 0, buffer.Length);
        source.Position = 0;
        source.SetLength(0);
    }

}