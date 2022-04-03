using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;





//Client to interface with the above server
public class Client
{
    public IPEndPoint sep;
    readonly TcpClient client; //receives moves and sends connection messages
    AsyncMessageClient asyncClient;

    string Username => GameNetwork.instance.Username;
    public bool gameJoined = false;
    readonly CancellationTokenSource cts;

    public Client(string username)
    {
        
        cts = new CancellationTokenSource();
        bool ipv6 = Socket.OSSupportsIPv6;
        try
        {   
            foreach (IPAddress ip in Dns.GetHostAddresses("wordserver.servegame.com"))
            {
                if (ip.AddressFamily == (ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork))
                {
                    sep = new IPEndPoint(ip, 25048);
                    break;
                }
            }

            client = new TcpClient(new IPEndPoint(ipv6 ? IPAddress.IPv6Any : IPAddress.Any, 0));
            client.Connect(sep);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.Log("Falling back to IPV4");
            try
            {
                ipv6 = false;
                foreach (IPAddress ip in Dns.GetHostAddresses("wordserver.servegame.com"))
                {
                    if (ip.AddressFamily == (ipv6 ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork))
                    {
                        sep = new IPEndPoint(ip, 25048);
                        break;
                    }
                }

                client = new TcpClient(new IPEndPoint(ipv6 ? IPAddress.IPv6Any : IPAddress.Any, 0));
                client.Connect(sep);
            }
            catch
            {
                Debug.Log(e.Message);
                Debug.Log ("Couldn't Connect to Server");
                GameNetwork.instance.OnDisconnected();
                return;
            }
        }
        
        asyncClient = new AsyncMessageClient(HandleGameMessage, sep, ipv6, cts.Token);
        asyncClient.BeginReceive();
        GameNetwork.instance.online = true;
    }

    public async void SendMove(Move move, int gameCode)
    {
        Message response = await client.SendMessageResponse(new Message(Username, gameCode, Message.MessageType.Move, move.Serialize()), cts.Token);
        if (!response.Successful)
        {
            throw new System.Exception(response.data);
        }
        else
        {
            Debug.Log("Move response: " + response.data);
        }
    }

    public async Task<bool> HostGame(int gameCode)
    {
        Message response = await client.SendMessageResponse(new Message(Username, gameCode, Message.MessageType.Host), cts.Token);
        
        Debug.Log(response.data);
        if (response.Successful)
        {
            TileTray.instance.ReplenishTiles();
            JoinGame(gameCode);
            return true;
        }

        return false;
    }

    public async void JoinGame (int gameCode)
    {
        TileTray.instance.Load();
        Message response = await client.SendMessageResponse(new Message(Username, gameCode, Message.MessageType.Join), cts.Token);
        Debug.Log(response.data);
        
        if(response.Successful)
        {
            gameJoined = true;
        }
    }

    public void DeleteGame(int gameCode)
    {
        Message response = client.SendMessageResponse(new Message(Username, gameCode, Message.MessageType.Delete), cts.Token).Result;

        if(response.IsDelete)
        {
            Debug.LogWarning("Implenmet Game deletion");
        }

        Debug.Log(response.data);
    }

    public void SendTilebagToServer(TileBag tileBag, int gameCode)
    {
        asyncClient.BeginSendMessage(new Message(Username, gameCode, Message.MessageType.TilebagState, tileBag.Serialize()));
    }

    public void HandleGameMessage(Message message, AsyncMessageClient asyncClient)
    {
        Debug.Log("Received message from Async: " + message.ToString());
        if (message.IsMove) //another player has just made a move
        {
            Dispatcher.RunOnMainThread
            (
                () => {
                    Move move = Move.Deserialize(message.data);
                    GameManager.instance.HandleOtherPlayerMove(move);
                }
            );
            return;
        }

        if (message.IsTurn) //another player is now online
        {
            Dispatcher.RunOnMainThread
            (
                () => {
                    GameManager.instance.SetPlayerTurn(message.data);
                }
            );
            return;
        }

        if (message.IsTilebagState) //another player has used tiles from the bag
        {
            Dispatcher.RunOnMainThread
            (
                () => {
                    TileTray.instance.bag.Deserialize(message.data);
                    TileTray.instance.ReplenishTiles();
                }
            );
            return;
        }

        if (message.IsJoin) //another player is now online
        {
            Dispatcher.RunOnMainThread
            (
                () => {
                    GameNetwork.instance.PlayerOnline(message.data);
                }
            );
            return;
        }
        
        if (message.IsDisconnect && message.username != "NULL" && message.gameCode != -1) //another player has gone inactive
        {
            Dispatcher.RunOnMainThread
            (
                () => {
                    GameNetwork.instance.PlayerOffline(message.data);
                }
            );
            return;
        }
        
        Debug.LogWarning($"Above message of type: {message.messageType} was left unhandled!");
    }

    public void BeginFullDisconnect()
    {
        client.SendMessage(new Message(Username, Message.MessageType.Disconnect));
    }

    bool disconnected = false;
    public async void DisconnectLocal()
    {
        if(disconnected) return;

        disconnected = true;
        
        Debug.Log("Disconnecting...");
        
        cts.Cancel();
        await Task.Delay(500);

        if(client.Connected)
        {
            client.Close();
            client.Dispose();
        }
    }
}