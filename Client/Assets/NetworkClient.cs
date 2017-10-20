using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;


public sealed class NetworkClient : MonoBehaviour {

    //private static readonly NetworkClient instance = new NetworkClient();
    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
    private byte[] _recieveBuffer = new byte[2048];
    public static string address;
    public static string currentNickname;
    public static byte[] currentAuthToken;

    public Queue<InetMessage> messageQueue = new Queue<InetMessage>();

    private void Start()
    {
        // Connect(address);
    }

    private void OnDisable()
    {
        Disconnect();
    }

    public void Connect(string address)
    {
        Debug.Log("Acetona2!!");
        

        IPAddress ipAddress;

        try
        {
            if (address == "Localhost")
                ipAddress = IPAddress.Parse("127.0.0.1");
            else
                ipAddress = IPAddress.Parse(address);
        }
        catch
        {
            ipAddress = Dns.GetHostAddresses(address)[0];
        }

        StateObject state = new StateObject();

		try
		{
			_clientSocket.BeginConnect(new IPEndPoint(ipAddress, 4400), new AsyncCallback(ConnectCallback), state);
		}
		catch (SocketException ex)
		{
			Debug.Log(ex.Message);
		}

        InvokeRepeating("ProcessMessages", 0.3f, 0.3f);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        _clientSocket.BeginReceive(state.buffer, 0, StateObject.prefixSizeBytes, SocketFlags.None, new AsyncCallback(ReceiveCallback), state);
        
    }

    private void Disconnect()
    {
        Message.SendMessage(MessageType.CLIENT_DISCONNECTION);
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    }

    private class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int bufferSize = 2048;
        // Receive buffer.
        public byte[] buffer = new byte[bufferSize];
        public byte[] msgBuffer;
        public byte[] prefixBuffer = new byte[prefixSizeBytes];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public int messageLenBytes;
        public int numBytesRead;
        public const int prefixSizeBytes = 4;
        // Whether to expect to read a prefix or not
        public bool awaitingPrefix = true;
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        StateObject state = (StateObject)ar.AsyncState;
        Socket current = _clientSocket;
        int bytesRead;

        try
        {
            bytesRead = current.EndReceive(ar);
        }
        catch (SocketException)
        {
            //TODO: handle server disconnection
            Disconnect();
            return;
        }

        // read succeeded

        if (bytesRead > 0)
        {
            // save last bytes read number to serve as an index.
            int prevBytesRead = state.numBytesRead;

            if (state.awaitingPrefix)
            {
                // attempt to read a prefix.
                state.numBytesRead += bytesRead;

                if (state.numBytesRead > StateObject.prefixSizeBytes)
                {
                    Debug.LogFormat("Error: read too many bytes for prefix. Read total {0} bytes, but expected only {1}", state.numBytesRead, StateObject.prefixSizeBytes);
                    return;
                }

                Buffer.BlockCopy(state.buffer, 0, state.prefixBuffer, prevBytesRead, bytesRead);

                if (state.numBytesRead == StateObject.prefixSizeBytes)
                {
                    state.messageLenBytes = BitConverter.ToInt32(state.prefixBuffer, 0);
                    state.awaitingPrefix = false;
                    state.numBytesRead = 0;

                    state.msgBuffer = new byte[state.messageLenBytes];

                    if (state.messageLenBytes <= StateObject.bufferSize)
                    {
                        current.BeginReceive(state.buffer, 0, state.messageLenBytes, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        current.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                }

                // get more prefix data.
                current.BeginReceive(state.buffer, 0, StateObject.prefixSizeBytes - state.numBytesRead, 0,
                    new AsyncCallback(ReceiveCallback), state);

            }
            else
            {
                // we have a prefix, deal with the message.
                state.numBytesRead += bytesRead;

                if (state.numBytesRead > state.messageLenBytes)
                {
                    Debug.LogFormat("Error: Read more bytes than the message itself is supposed to have. Read total {0} bytes, but expected only {1}", state.numBytesRead, state.messageLenBytes);
                    return;
                }

                // read in the data
                Buffer.BlockCopy(state.buffer, 0, state.msgBuffer, prevBytesRead, bytesRead);

                if (state.numBytesRead == state.messageLenBytes)
                {
                    // we are done getting the message.
                    // serialize it, and process it.

                    state.numBytesRead = 0;
                    state.awaitingPrefix = true;
                    state.messageLenBytes = 0;

                    InetMessage inetMsg = SerializationUtils.DeserializeInetMessage(state.msgBuffer);
                    EnqueueMessages(inetMsg);
                }
                else if (state.numBytesRead < state.messageLenBytes)
                {
                    // ask for more

                    if ((state.messageLenBytes - state.messageLenBytes) > StateObject.bufferSize)
                    {
                        current.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                    else
                    {
                        current.BeginReceive(state.buffer, 0, state.messageLenBytes - state.messageLenBytes, 0,
                            new AsyncCallback(ReceiveCallback), state);
                    }
                }

            }
        }
    }

    public void ProcessMessages()
    {
        if (messageQueue.Count == 0)
            return;
        else
        {
            lock(messageQueue)
            {
                for (int i = 0; i < messageQueue.Count; ++i)
                {
                    InetMessage inetMsg = messageQueue.Dequeue();
                    MessageType msgType = (MessageType)inetMsg.id;
                    switch (msgType)
                    {
                        case MessageType.CHAT:
                            {
                                string message = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                ChatHistoryTextProc.PrintChat(message);
                            }
                            break;

                        case MessageType.GAME_INITIAL_TURN:
                            {
                                GameManager.SetupInitialTurn();
                            }
                            break;

                        case MessageType.GAME_TURN:
                            {
                                GameManager.SetupTurn();
                            }
                            break;

                        case MessageType.GAME_TURN_END:
                            {
                                GameManager.EndTurn();
                            }
                            break;

                        case MessageType.GAME_BOARD_UPDATE:
                            {
                                byte[] board = inetMsg.objectData;
                                GameManager.UpdateGameBoard(board);
                            }
                            break;

                        case MessageType.GAME_IN_PROGRESS:
                            {
                                ChatHistoryTextProc.GameInProgress();
                            }
                            break;

                        case MessageType.GAME_WON:
                            {
                                int score = (int)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                GameManager.WinGame(score);
                            }
                            break;

                        case MessageType.GAME_LOST:
                            {
                                GameManager.LoseGame();
                            }
                            break;

                        case MessageType.GAME_QUESTION:
                            {
                                QuestionInfo question = (QuestionInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                GameManager.AskQuestion(question);
                            }
                            break;

                        case MessageType.GAME_INVALID_MOVE:
                            {
                                GameManager.InvalidMove();
                            }
                            break;

                        case MessageType.GAME_STARTED:
                            {
                                UIManager.SwitchToGame();
                            }
                            break;

                        case MessageType.GAME_CORRECT:
                            {
                                GameManager.CorrectAnswer();
                            }
                            break;

                        case MessageType.GAME_INCORRECT:
                            {
                                GameManager.IncorrectAnswer();
                            }
                            break;

                        case MessageType.GAME_SCORE_UPDATE:
                            {
                                UserInfo[] scores = (UserInfo[])SerializationUtils.DeserializeObject(inetMsg.objectData);
                                GameManager.UpdateScore(scores);
                            }
                            break;

                        case MessageType.USERINFO_DATA:
                            {
                                UserInfo[] users = (UserInfo[])SerializationUtils.DeserializeObject(inetMsg.objectData);
                                UIManager.SetChatUserList(users);
                            }
                            break;

                        case MessageType.USER_JOINED_ROOM:
                            {
                                UserInfo user = (UserInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                UIManager.AddUserToList(user);
                                ChatHistoryTextProc.UserJoinedRoom(user);
                            }
                            break;

                        case MessageType.USER_LEFT_ROOM:
                            {
                                UserInfo user = (UserInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                UIManager.RemoveUserFromList(user);
                                ChatHistoryTextProc.UserLeftRoom(user);
                            }
                            break;

                        case MessageType.USER_ALREADY_IN_ROOM:
                            {
                                ChatHistoryTextProc.AlreadyInRoom();
                            }
                            break;

                        case MessageType.ROOM_FULL:
                            {
                                ChatHistoryTextProc.RoomIsFull();
                            }
                            break;

                        case MessageType.ROOM_LIST:
                            {
                                if (UIManager.IsChatRoomActive())
                                    RoomListProc.DisplayRoomList((RoomInfo[])SerializationUtils.DeserializeObject(inetMsg.objectData));
                            }
                            break;

                        case MessageType.ROOM_ALREADY_EXISTS:
                            {
                                ChatHistoryTextProc.RoomAlreadyExists();
                            }
                            break;

                        case MessageType.ROOM_DOES_NOT_EXIST:
                            {
                                ChatHistoryTextProc.RoomDoesntExist();
                            }
                            break;

                        case MessageType.GAME_PLAYER_LIST:
                            {
                                GameManager.LoadPlayers((UserInfo[])SerializationUtils.DeserializeObject(inetMsg.objectData));
                            }
                            break;

                        case MessageType.MOVE_TO_ROOM:
                            {
                                RoomInfo roomInfo = (RoomInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                if (roomInfo.roomName.Equals("Lobby"))
                                {
                                    UIManager.SwitchToChat();
                                    UIManager.SwitchToRoom(roomInfo.roomName);
                                    StartGameBtnProc.Disable();
                                }
                                else
                                {
                                    UIManager.SwitchToRoom(roomInfo.roomName);
                                    StartGameBtnProc.Enable();
                                }

                                //UIManager.AddUserToList(currentUserName);
                            }
                            break;

                        case MessageType.NICKNAME_CHANGED:
                            {
                                string[] args = (string[])SerializationUtils.DeserializeObject(inetMsg.objectData);
                                string oldNickname = args[0];
                                string newNickname = args[1];
                                UIManager.RenameUserInList(oldNickname, newNickname);
                            }
                            break;

                        case MessageType.NICKNAME_CHANGED_SUCCESS:
                            {
                                string newNickname = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                UIManager.SetPlayerNickname(newNickname);
                                ChatHistoryTextProc.NicknameChangeSuccessful();
                            }
                            break;

                        case MessageType.USER_DETAILS:
                            {
                                UserInfo userInfo = (UserInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                                UIManager.SetPlayerNickname(userInfo.nickname);
                            }
                            break;

                        default:
                            Debug.Log("Invalid message type!");
                            break;
                    }

                }
            }
        }
    }

    public static void SendData(byte[] data)
    {
        SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
        socketAsyncData.SetBuffer(data,0,data.Length);
        _clientSocket.SendAsync(socketAsyncData);
    }
 
    private void EnqueueMessages(InetMessage inetMsg)
    {
        if (Enum.IsDefined(typeof(MessageType), inetMsg.id))
        {
            // we have a valid message
            Debug.Log("Received a " + ((MessageType)inetMsg.id).ToString() + " message!");
            lock(messageQueue)
                messageQueue.Enqueue(inetMsg);
        }
    }
 
    private void ProcessChatMessage(string recString)
    {
        Debug.Log(recString);
    }

    public static string GetMessageID(string fullMessage)
    {
        int delimPos1 = -1;
        int delimPos2 = 0;

        string result;

        delimPos1 = fullMessage.IndexOf(':', (delimPos1 + 1));
        if (delimPos1 == -1)
        {
            return "";
        }

        delimPos2 = fullMessage.IndexOf(':', (delimPos1 + 1));

        result = fullMessage.Substring(delimPos1 + 1, delimPos2 - delimPos1 - 1);

        return result;
    }

    public static string GetUserNickname(string fullMessage)
    {
        int delimPos1 = -1;

        string result;

        delimPos1 = fullMessage.IndexOf(':', (delimPos1 + 1));

        result = fullMessage.Substring(0, delimPos1);
        return result;
    }

    public static string GetMessageContent(string fullMessage)
    {
        int delimPos1 = -1;

        string result;
        for (int i = 0; i < 2; ++i)
        {
            delimPos1 = fullMessage.IndexOf(':', (delimPos1 + 1));
            if (delimPos1 == -1)
            {
                return "";
            }

        }

        result = fullMessage.Substring(delimPos1 + 1);

        return result;
    }

    public static string hashPassword(string password, string salt)
    {
        SHA256Managed crypto = new SHA256Managed();
        string saltedPass = password + salt;
        StringBuilder hash = new StringBuilder();
        byte[] cryptoBytes = crypto.ComputeHash(Encoding.UTF8.GetBytes(saltedPass), 0, Encoding.UTF8.GetByteCount(saltedPass));
        foreach (byte theByte in cryptoBytes)
        {
            hash.Append(theByte.ToString("x2"));
        }
        return hash.ToString();
    }
}
 