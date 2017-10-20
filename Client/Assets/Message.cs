using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

/*
public enum MessageType : uint
{
    // client sends and receives
    CHAT,

    // client receives
    GAME_INITIAL_TURN,
    GAME_TURN,
    GAME_TURN_END,
    GAME_BOARD_UPDATE,
    GAME_IN_PROGRESS,
    GAME_WON,
    GAME_LOST,
    GAME_QUESTION,
    GAME_INVALID_MOVE,
    GAME_STARTED,
    GAME_CORRECT,
    GAME_INCORRECT,
    USERLIST_DATA,
    USER_JOINED_ROOM,
    USER_ALREADY_IN_ROOM,
    ROOM_FULL,
    ROOM_ALREADY_EXISTS,
    ROOM_DOES_NOT_EXIST,
    GAME_PLAYER_LIST,
    MOVE_TO_ROOM,
    NICKNAME_CHANGED,
    NiCKNAME_CHANGED_SUCCESS,

    //client sends
    LOGIN_REQUEST,
    SET_NICKNAME,
    CREATE_ROOM_REQUEST,
    JOIN_ROOM_REQUEST,
    JOIN_LOBBY_REQUEST,
    START_GAME_REQUEST,
    GAME_QUESTION_ANSWER,
    GAME_TERRITORY_ATTACKED,
    CLIENT_DISCONNECTION

}
*/

class Message
{
    private Message() { }

    private static NetworkClient networkClient = GameObject.Find("Networking").GetComponent<NetworkClient>();

    public static void SendMessage(MessageType msgType, byte[] data)
    {
        InetMessage inetMsg = new InetMessage();
        inetMsg.id = (uint)msgType;
        inetMsg.objectData = data;

        byte[] dataToSend = ProcessMsgToSend(inetMsg);

        NetworkClient.SendData(dataToSend);
    }

    public static void SendMessage(MessageType msgType, params string[] args)
    {
        byte[] data = null;

        if (args.Length == 1)
            data = SerializationUtils.SerializeObject(args[0]);
        else if (args.Length > 1)
            data = SerializationUtils.SerializeObject(args);


        InetMessage inetMsg = new InetMessage();
        inetMsg.id = (uint)msgType;
        inetMsg.objectData = data;

        byte[] dataToSend = ProcessMsgToSend(inetMsg);

        NetworkClient.SendData(dataToSend);
    }

    public static void SendMessage(MessageType msgType, QuestionAnswer ans)
    {
        byte[] data = SerializationUtils.SerializeObject(ans);

        InetMessage inetMsg = new InetMessage();
        inetMsg.id = (uint)msgType;
        inetMsg.objectData = data;

        byte[] dataToSend = ProcessMsgToSend(inetMsg);

        NetworkClient.SendData(dataToSend);
    }

    public static byte[] ProcessMsgToSend(InetMessage inetMsg)
    {
        byte[] inetData = SerializationUtils.SerializeInetMessage(inetMsg);
        int messageLen = inetData.Length;
        byte[] dataToSend = new byte[sizeof(int) + inetData.Length];
        byte[] prefixBytes = BitConverter.GetBytes(messageLen);
        Buffer.BlockCopy(prefixBytes, 0, dataToSend, 0, sizeof(int));
        Buffer.BlockCopy(inetData, 0, dataToSend, sizeof(int), inetData.Length);

        if (inetData == null || dataToSend == null)
        {
            Console.WriteLine("\n!!Error sending InetMessage: serialized InetMessage null!");

            return null;
        }

        return dataToSend;
    }
}
