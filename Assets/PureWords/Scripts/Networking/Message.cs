using System;
using Newtonsoft.Json;

public enum MessageType
{
    Host, // a client is starting to host a game - serverside
    Join, // a client wants to join a game - serverside
    Turn, // tell clients whose turn it is
    
    Delete, // a game has ended and needs to be deleted - serverside

    Confirm, // a client has joined a game

    Move, // a client is sending or receiving a move

    TilebagState, // a client is sending or receiving a tilebag state

    Disconnect, // a client has disconnected
    Error, // a client has sent an invalid message

    Connect // secondary socket connection
}

public struct Message
{

    public string username;
    public string gameCode;
    public string data;
    public MessageType messageType;

    #region Message Types
    [JsonIgnore]public readonly bool Successful => messageType != MessageType.Error;
    [JsonIgnore]public readonly bool IsHost => messageType == MessageType.Host;
    [JsonIgnore]public readonly bool IsJoin => messageType == MessageType.Join;
    [JsonIgnore]public readonly bool IsTurn => messageType == MessageType.Turn;
    [JsonIgnore]public readonly bool IsDelete => messageType == MessageType.Delete;
    [JsonIgnore]public readonly bool IsConfirm => messageType == MessageType.Confirm;
    [JsonIgnore]public readonly bool IsMove => messageType == MessageType.Move;
    [JsonIgnore]public readonly bool IsTilebagState => messageType == MessageType.TilebagState;
    [JsonIgnore]public readonly bool IsDisconnect => messageType == MessageType.Disconnect;
    [JsonIgnore]public readonly bool IsConnect => messageType == MessageType.Connect;
    #endregion

    #region Constructors
    public Message(MessageType messageType)
    {
        this.username = "";
        this.gameCode = "";
        this.messageType = messageType;
        data = "";
    }

    public Message(MessageType messageType, string data)
    {
        this.username = "";
        this.gameCode = "";
        this.messageType = messageType;
        this.data = data;
    }

    public Message(string username, MessageType messageType)
    {
        this.username = username;
        this.gameCode = "";
        this.messageType = messageType;
        data = "";
    }

    public Message(string username, string gameCode, MessageType messageType)
    {
        this.username = username;
        this.gameCode = gameCode;
        this.messageType = messageType;
        data = "";
    }

    public Message(string username, string gameCode, MessageType messageType, string data)
    {
        this.username = username;
        this.gameCode = gameCode;
        this.messageType = messageType;
        this.data = data;
    }

    public Message(string username, MessageType messageType, string data)
    {
        this.username = username;
        this.gameCode = "";
        this.messageType = messageType;
        this.data = data;
    }
    #endregion

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static Message? Deserialize(string message)
    {
        return JsonConvert.DeserializeObject<Message>(message);
    }

    public override string ToString()
    {
        return $"Type:{messageType} - Name:{username} - Game:{gameCode} - Data:{data}";
    }

    public static Message Error => new Message(MessageType.Error);

}