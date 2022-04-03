using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public delegate void HandleMessageCallback(Message message, AsyncMessageClient context);


public class ClientContext
{
    public TcpClient Client;
    public Stream Stream;
    public byte[] Buffer = new byte[4];
    public MemoryStream Message = new MemoryStream();

    public void Disconnect()
    {
        Debug.Log("Disconnected from server!");
        Client.Close();
        Stream.Dispose();
        Message.Dispose();
        Dispatcher.RunOnMainThread(() => GameNetwork.instance.OnDisconnected());
    }
}


public class AsyncMessageClient
{
    HandleMessageCallback handleMessage;
    IPEndPoint endPoint;
    CancellationToken token;
    ClientContext context;
    bool ipv6;
    string Username => GameNetwork.instance.Username;
    public AsyncMessageClient(HandleMessageCallback handleMessage, IPEndPoint endPoint, bool ipv6 = false, CancellationToken token = default)
    {
        this.handleMessage = handleMessage;
        this.endPoint = endPoint;
        this.token = token;
        this.ipv6 = ipv6;
    }

    public bool BeginReceive()
    {
        try
        {
            TcpClient asyncClient = new TcpClient(new IPEndPoint(ipv6 ? IPAddress.IPv6Any : IPAddress.Any, 0));
            context = new ClientContext();
            context.Client = asyncClient;
            asyncClient.BeginConnect(endPoint.Address, endPoint.Port, new AsyncCallback(MovesConnectCallback), null);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            GameNetwork.instance.OnDisconnected();
            return false;
        }

        return true;
    }

    private async void MovesConnectCallback(IAsyncResult ar)
    {
        context.Client.EndConnect(ar);
        Message response = await context.Client.SendMessageResponse(new Message(Username, Message.MessageType.Socket, Username), token);
        Debug.Log(response.data);
        context.Stream = context.Client.GetStream();
        context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, new AsyncCallback(MovesReadCallback), null);
    }

    private void MovesReadCallback(IAsyncResult ar)
    {
        try
        {
            int read = context.Stream.EndRead(ar);
            if(read == 0)
            {
                context.Disconnect();
                return;
            }
            context.Message.Clear();
            context.Message.Write(context.Buffer, 0, read);

            int length = BitConverter.ToInt32(context.Buffer, 0);
            if(length == 0)
            {
                context.Disconnect();
                return;
            }

            byte[] buffer = new byte[128];
            while (length > 0)
            {
                read = context.Stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                if(read == 0)
                {
                    context.Disconnect();
                    return;
                }

                context.Message.Write(buffer, 0, read);
                length -= read;
            }

            Message m = Message.Deserialize(Encoding.UTF8.GetString(context.Message.GetBuffer(), 4, context.Message.GetBuffer().Length - 4));
            Dispatcher.RunOnMainThread(() => handleMessage.Invoke(m, this));

            context.Stream.BeginRead(context.Buffer, 0, context.Buffer.Length, MovesReadCallback, context);
        }
        catch (Exception e)
        {
            Dispatcher.RunOnMainThread(() => Debug.Log(e.Message));
            context.Disconnect();
        }
    }

    public void BeginSendMessage(Message m)
    {
        if(context.Stream == null) return;

        Console.WriteLine($"Sending message: {m}");

        byte[] buffer = Encoding.UTF8.GetBytes(m.Serialize());

        try
        {
            context.Stream.BeginWrite(BitConverter.GetBytes(buffer.Length).Concat(buffer).ToArray(), 0, buffer.Length + 4, OnClientWrite, null);
        }
        catch (Exception e)
        {
            Dispatcher.RunOnMainThread(() => Debug.Log("Error sending message: " + e.Message));
            context.Disconnect();
        }
    }

    private void OnClientWrite(IAsyncResult ar)
    {
        if (context.Client == null) return;
        if (context.Stream == null) return;

        try
        {
            context.Stream.EndWrite(ar);
        }
        catch (Exception e)
        {
            Dispatcher.RunOnMainThread(() => Debug.Log("Error sending message: " + e.Message));
            context.Disconnect();
        }
    }
}