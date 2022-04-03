using System;
using UnityEngine;
using Newtonsoft.Json;

public struct Message
{
    public enum MessageType
    {
        Host, //a client is starting to host a game - serverside
        Join, //a client wants to join a game - serverside
        Turn, // tell clients whose turn it is
        
        Delete, //a game has ended and needs to be deleted - serverside

        Confirm, //a client has joined a game

        Move, //a client is sending or receiving a move

        TilebagState, //a client is sending or receiving a tilebag state

        Disconnect, //a client has disconnected
        Error, //a client has sent an invalid message

        Socket //secondary socket connection
    }

    public string username;
    public int gameCode;
    public MessageType messageType;
    public string data;

    #region Message Types
    [JsonIgnore]public bool Successful => messageType != MessageType.Error;
    [JsonIgnore]public bool IsHost => messageType == MessageType.Host;
    [JsonIgnore]public bool IsJoin => messageType == MessageType.Join;
    [JsonIgnore]public bool IsTurn => messageType == MessageType.Turn;
    [JsonIgnore]public bool IsDelete => messageType == MessageType.Delete;
    [JsonIgnore]public bool IsConfirm => messageType == MessageType.Confirm;
    [JsonIgnore]public bool IsMove => messageType == MessageType.Move;
    [JsonIgnore]public bool IsTilebagState => messageType == MessageType.TilebagState;
    [JsonIgnore]public bool IsDisconnect => messageType == MessageType.Disconnect;
    [JsonIgnore]public bool IsSocket => messageType == MessageType.Socket;
    #endregion

    #region Constructors
    public Message(MessageType messageType)
    {
        this.username = "";
        this.gameCode = 0;
        this.messageType = messageType;
        data = "";
    }

    public Message(MessageType messageType, string data)
    {
        this.username = "";
        this.gameCode = 0;
        this.messageType = messageType;
        this.data = data;
    }

    public Message(string username, MessageType messageType)
    {
        this.username = username;
        this.gameCode = 0;
        this.messageType = messageType;
        data = "";
    }

    public Message(string username, int gameCode, MessageType messageType)
    {
        this.username = username;
        this.gameCode = gameCode;
        this.messageType = messageType;
        data = "";
    }

    public Message(string username, int gameCode, MessageType messageType, string data)
    {
        this.username = username;
        this.gameCode = gameCode;
        this.messageType = messageType;
        this.data = data;
    }

    public Message(string username, MessageType messageType, string data)
    {
        this.username = username;
        this.gameCode = 0;
        this.messageType = messageType;
        this.data = data;
    }
    #endregion

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static Message Deserialize(string message)
    {
        return JsonConvert.DeserializeObject<Message>(message);
    }

    public override string ToString()
    {
        return $"Type:{messageType} - Name:{username} - Game:{gameCode} - Data:{data}";
    }

}