using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class Message
    {
        private Message() { }

        private static DBManager dbManager = DBManager.Instance;
        private static UserManager userManager = UserManager.Instance;
        private static ChatRoomManager chatRoomManager = ChatRoomManager.Instance;
        private static QuestionManager questionManager = QuestionManager.Instance;

        /*
        public enum MessageType
        {
            // server sends and receives
            CHAT,

            // server sends
            GAME_INITIAL_TURN,
            GAME_TURN,
            GAME_TURN_END,
            GAME_BOARD_UPDATE,
            GAME_IN_PROGRESS,
            GAME_WON,
            GAME_LOST,
            GAME_QUESTION,
            GAME_QUESTION_DATA,
            GAME_INVALID_MOVE,
            GAME_STARTED,
            GAME_CORRECT,
            GAME_INCORRECT,
            USERLIST_DATA,
            USER_JOINED_ROOM,
            USER_ALREADY_IN_ROOM,
            ROOM_FULL,
            ROOM_EXISTS,
            GAME_PLAYER_LIST,
            MOVE_TO_ROOM,
            NICKNAME_CHANGED,

            //server receives
            LOGIN_REQUEST,
            SET_NICKNAME,
            CREATE_ROOM_REQUEST,
            JOIN_ROOM_REQUEST,
            JOIN_LOBBY_REQUEST,
            START_GAME_REQUEST,
            GAME_QUESTION_ANSWER,
            GAME_TERRITORY_ATTACKED,
            CLIENT_DISCONNECTION

        }*/

        public static void ProcessMessage(InetMessage inetMsg, Socket clientSocket)
        {
            MessageType type = (MessageType)inetMsg.id;
            switch (type)
            {
                case MessageType.CHAT:
                    {
                        User user = userManager.FindUser(x => x.GetSocket().Equals(clientSocket));
                        if (user != null)
                        {
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            string message = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);


                            if (message.Length <= 0)
                                return;

                            if (message[0] == '/')
                            {
                                int delimPos = message.IndexOf(' ');
                                string command;
                                string content = null;
                                if (delimPos > 0)
                                {
                                    // get command
                                    command = message.Substring(1, delimPos - 1);

                                    // get content
                                    delimPos = message.IndexOf(' ', delimPos);
                                    if (delimPos > 0)
                                        content = message.Substring(delimPos + 1);
                                }
                                else
                                    command = message.Substring(1);

                                // check if we have a non-empty command
                                if (command.Length > 0 && message.Length >= (command.Length + 1))
                                {
                                    switch (command.ToLower())
                                    {
                                        case "w":
                                        case "whisper":
                                            {
                                                if (content != null)
                                                {
                                                    Console.WriteLine(content);
                                                }

                                            }
                                            break;
                                        case "h":
                                        case "help":
                                            {
                                                // print help message
                                                string helpMessage = "Server: Available commands:\n" +
                                                    "/help - prints this message\n" +
                                                    "/whisper - sends a private message to the specified user\n" +
                                                    "\tusage: /whisper toNickname message\n";
                                                SendChatMessage(clientSocket, helpMessage);
                                            }
                                            break;



                                        default:
                                            SendMessage(clientSocket, MessageType.INVALID_COMMAND);
                                            break;
                                    }

                                }
                                else
                                    SendMessage(clientSocket, MessageType.INVALID_COMMAND);
                            }
                            else
                            {
                                string fullMessage = user.GetNickname() + ": " + message;
                                byte[] data = SerializationUtils.SerializeObject(fullMessage);
                                for (int i = 0; i < room.CountUsers(); ++i)
                                {
                                    SendMessage(room.GetUserAtIndex(i).GetSocket(), MessageType.CHAT, data);
                                }
                            }
                        }
                    }
                    break;
                case MessageType.CREATE_USER_REQUEST:
                    {
                        LoginInfo loginInfo = (LoginInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                        if (dbManager.UserExists(loginInfo.username))
                        {
                            SendMessage(clientSocket, MessageType.USER_ALREADY_EXISTS);
                        }
                        else
                        {
                            dbManager.AddUser(loginInfo.username, loginInfo.hash);
                            DBManager.UserInfo userInfo = dbManager.GetUserInfo(loginInfo.username);
                            User user = new User(userInfo.userName, userInfo.score, userInfo.nickName, clientSocket);

                            userManager.AddUser(user);
                            ChatRoom room = chatRoomManager.GetRoom("Lobby");
                            chatRoomManager.AddUserToRoom(user, "Lobby");
                            UserInfo[] currentUsers = room.GetUserInfo();
                            

                            // move to lobby
                            SendMoveToRoomMessage(clientSocket, room);
                            Thread.Sleep(400);
                            SendUserInfoMessage(clientSocket, currentUsers);
                            BroadcastRoomInfos();
                            SendUserJoinedRoomMessage(room, clientSocket, user.GetInfo());
                        }
                    }
                    break;
                case MessageType.LOGIN_REQUEST:
                    {
                        LoginInfo loginInfo = (LoginInfo)SerializationUtils.DeserializeObject(inetMsg.objectData);
                        string username = loginInfo.username;
                        string hashedPass = loginInfo.hash;

                        bool userExists = dbManager.UserExists(username);
                        if (!userExists)
                        {
                            SendMessage(clientSocket, MessageType.LOGIN_FAILURE);
                            return;
                        }

                        bool validated = dbManager.ValidateUser(username, hashedPass);
                        if (validated && !userManager.UserLoggedIn(username))
                        {
                            DBManager.UserInfo userInfo = dbManager.GetUserInfo(username);
                            User user = new User(userInfo.userName, userInfo.score, userInfo.nickName, clientSocket);

                            userManager.AddUser(user);
                            ChatRoom room = chatRoomManager.GetRoom("Lobby");
                            chatRoomManager.AddUserToRoom(user, "Lobby");
                            UserInfo[] currentUsers = room.GetUserInfo();


                            // move to lobby
                            SendUserDetailsMessage(clientSocket, user.GetInfo());
                            SendMoveToRoomMessage(clientSocket, room);
                            Thread.Sleep(400);
                            SendUserInfoMessage(clientSocket, currentUsers);
                            BroadcastRoomInfos();
                            SendUserJoinedRoomMessage(room, clientSocket, user.GetInfo());
                        }
                    }
                    break;
                case MessageType.SET_NICKNAME:
                    {
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            // user logged in, allow it to change their nickname.
                            string newNickname = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);
                            string oldNickname = user.GetNickname();
                            if(dbManager.ChangeUserNickname(user.GetUsername(), newNickname))
                            {
                                ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                                user.SetNickName(newNickname);
                                //room.NicknameChanged(user);
                                SendNicknameChangedMessage(room, newNickname, oldNickname);
                                SendNicknameChangeSuccessMessage(clientSocket, oldNickname);
                            }
                            else
                            {
                                SendNicknameNotAvailableMessage(clientSocket);
                            }

                        }

                    }
                    break;
                case MessageType.CREATE_ROOM_REQUEST:
                    {
                        User user = userManager.GetUser(clientSocket);
                        ChatRoom currentRoom = chatRoomManager.GetRoomOfUser(user);
                        if (user != null)
                        {
                            string roomName = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);
                            // attempt to create the room and move the user
                            ChatRoom room = chatRoomManager.CreateRoom(roomName, user);
                            if (room != null)
                            {
                                BroadcastRoomInfos();
                                SendMoveToRoomMessage(clientSocket, room);
                                SendUserInfoMessage(clientSocket, room.GetUserInfo());
                            }
                            else
                            {
                                SendRoomAlreadyExistsMessage(clientSocket);
                            }

                            if (currentRoom != null && currentRoom.CountUsers() > 0)
                            {
                                SendUserLeftRoomMessage(currentRoom, user.GetInfo());
                            }
                        }
                    }
                    break;
                case MessageType.JOIN_ROOM_REQUEST:
                    {
                        User user = userManager.GetUser(clientSocket);
                        ChatRoom currentRoom = chatRoomManager.GetRoomOfUser(user);
                        if (user != null)
                        {
                            string roomName = (string)SerializationUtils.DeserializeObject(inetMsg.objectData);
                            if (chatRoomManager.RoomExists(roomName))
                            {
                                if (currentRoom != null && currentRoom.GetGame() != null)
                                {
                                    // player left a game
                                    Game game = currentRoom.GetGame();
                                    game.ReplacePlayerWithAI(user.GetSocket());
                                }

                                ChatRoom room = chatRoomManager.GetRoom(roomName);

                                if (room.GetGame() != null)
                                {
                                    SendGameInProgressMessage(clientSocket);
                                    return;
                                }
                                // check if the user is already in the room
                                else if (chatRoomManager.GetRoomOfUser(user).GetName().Equals(roomName))
                                {
                                    SendAlreadyInRoomMessage(clientSocket);
                                }
                                else if (roomName.Equals("Lobby") || (room.CountUsers() < 3) )
                                {
                                    chatRoomManager.ChangeRoom(roomName, user);
                                    BroadcastRoomInfos();
                                    UserInfo[] userInfo = room.GetUserInfo();
                                    SendMoveToRoomMessage(clientSocket, room);
                                    SendUserInfoMessage(clientSocket, userInfo);
                                    SendUserJoinedRoomMessage(room, clientSocket, user.GetInfo());
                                }
                                else
                                {
                                    SendRoomFullMessage(clientSocket);
                                }

                                if (currentRoom != null && currentRoom.CountUsers() > 0)
                                {
                                    SendUserLeftRoomMessage(currentRoom, user.GetInfo());
                                }
                            }
                            else
                                SendRoomDoesNotExistMessage(clientSocket);
                        }
                    }
                    break;
                case MessageType.JOIN_LOBBY_REQUEST:
                    {
                        // use join room req
                        /*
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            if (!room.GetName().Equals("Lobby"))
                            {
                                string[] userNames = room.GetUserNames();
                                chatRoomManager.ChangeRoom("Lobby", user);

                                SendMessage(clientSocket, MessageType.MOVE_TO_ROOM, SerializationUtils.SerializeString("Lobby"));
                                SendMessage(clientSocket, MessageType.USERLIST_DATA, SerializationUtils.SerializeStringArray(userNames))
                            }
                            else
                                SendMessage(clientSocket, MessageType.USER_ALREADY_IN_ROOM);
                        }
                        */
                    }
                    break;
                case MessageType.START_GAME_REQUEST:
                    {
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            SendGameStartedMessage(room);
                            //SendGamePlayerListMessage(room);
                            room.StartGame(user);
                        }
                    }
                    break;
                case MessageType.GAME_QUESTION_ANSWER:
                    {
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            Game game = room.GetGame();

                            QuestionAnswer ans = (QuestionAnswer)SerializationUtils.DeserializeObject(inetMsg.objectData);

                            game.ProcessQuestion(ans, user);
                        }
                    }
                    break;
                case MessageType.GAME_TERRITORY_ATTACKED:
                    {
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            byte move = inetMsg.objectData[0];
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            Game game = room.GetGame();
                            game.PerformMoveRequest(user, move);
                        }
                    }
                    break;
                case MessageType.CLIENT_DISCONNECTION:
                    {
                        User user = userManager.GetUser(clientSocket);
                        if (user != null)
                        {
                            ChatRoom room = chatRoomManager.GetRoomOfUser(user);
                            room.RemoveUser(user.GetSocket());
                            SendUserLeftRoomMessage(room, user.GetInfo());
                            userManager.RemoveUser(user);
                            // try close?
                            clientSocket.Dispose();
                            ServerManager._clientSockets.Remove(clientSocket);
                        }
                        else
                        {
                            ServerManager._clientSockets.Remove(clientSocket);
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Invalid message type!");
                    break;
            }

        }

        public static void SendMessage(Socket clientSocket, MessageType msgType, byte[] data = null)
        {
            InetMessage inetMsg = new InetMessage();
            inetMsg.id = (uint)msgType;
            inetMsg.objectData = data;

            byte[] dataToSend = ProcessMsgToSend(inetMsg);
            
            // try to make it multithreaded?
            // but maybe we already are in another thread.
            // TODO: find out
            ServerManager.Instance.Send(clientSocket, dataToSend);

            Console.WriteLine("\nSent message " + ((MessageType)inetMsg.id).ToString() + (inetMsg.objectData == null ? "" : " with additional object data!") + " to socket: " + clientSocket.Handle.ToInt32());
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

        public static void BroadcastToUsersInRoom(ChatRoom room, MessageType msgType, byte[] data = null)
        {
            if (room != null)
            {
                List<User> users = room.GetUserList();
                for (int i = 0; i < users.Count(); ++i)
                {
                    SendMessage(users[i].GetSocket(), msgType, data);
                }
            }
        }

        public static void BroadcastToUsersInRoomExceptCurrent(ChatRoom room, Socket current, MessageType msgType, byte[] data = null)
        {
            if (room != null)
            {
                List<User> users = room.GetUserList();
                for (int i = 0; i < users.Count(); ++i)
                {
                    if (!users[i].GetSocket().Equals(current))
                        SendMessage(users[i].GetSocket(), msgType, data);
                }
            }
        }


        public static void SendChatMessage(Socket socket, string message)
        {
            SendMessage(socket, MessageType.CHAT, SerializationUtils.SerializeObject(message));
        }

        public static void SendChatMessageToRoom(ChatRoom room, string message)
        {
            BroadcastToUsersInRoom(room, MessageType.CHAT, SerializationUtils.SerializeObject(message));
        }

        public static void SendInitialTurnMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_INITIAL_TURN);
        }

        public static void SendTurnMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_TURN);
        }

        public static void SendTurnEndMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_TURN_END);
        }

        public static void SendBoardUpdateToPlayers(ChatRoom room, byte[] board)
        {
            BroadcastToUsersInRoom(room, MessageType.GAME_BOARD_UPDATE, board);
        }

        public static void SendGameInProgressMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_IN_PROGRESS);
        }

        public static void SendGameWonMessage(Socket socket, int scoreDelta)
        {
            SendMessage(socket, MessageType.GAME_WON, SerializationUtils.SerializeObject(scoreDelta));
        }

        public static void SendGameLostMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_LOST);
        }

        public static void SendGameQuestionMessage(Socket socket, QuestionInfo question)
        {
            SendMessage(socket, MessageType.GAME_QUESTION, SerializationUtils.SerializeObject(question));
        }

        public static void SendInvalidMoveMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_INVALID_MOVE);
        }

        public static void SendGameStartedMessage(ChatRoom room)
        {
            BroadcastToUsersInRoom(room, MessageType.GAME_STARTED);
        }

        public static void SendGameCorrectMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_CORRECT);
        }

        public static void SendGameIncorrectMessage(Socket socket)
        {
            SendMessage(socket, MessageType.GAME_INCORRECT);
        }

        public static void SendUserInfoMessage(Socket socket, UserInfo[] userInfos)
        {
            SendMessage(socket, MessageType.USERINFO_DATA, SerializationUtils.SerializeObject(userInfos));
        }

        public static void SendUserJoinedRoomMessage(ChatRoom broadcastRoom, Socket current, UserInfo userinfo)
        {
            BroadcastToUsersInRoomExceptCurrent(broadcastRoom, current, MessageType.USER_JOINED_ROOM, SerializationUtils.SerializeObject(userinfo));
            //SendMessage(socket, MessageType.USER_JOINED_ROOM, SerializationUtils.SerializeUserInfo(userinfo));
        }

        public static void SendUserLeftRoomMessage(ChatRoom broadcastRoom, UserInfo userinfo)
        {
            BroadcastToUsersInRoom(broadcastRoom, MessageType.USER_LEFT_ROOM, SerializationUtils.SerializeObject(userinfo));
            //SendMessage(socket, MessageType.USER_LEFT_ROOM, SerializationUtils.SerializeObject(userinfo));
        }

        public static void SendAlreadyInRoomMessage(Socket socket)
        {
            SendMessage(socket, MessageType.USER_ALREADY_IN_ROOM);
        }

        public static void SendRoomFullMessage(Socket socket)
        {
            SendMessage(socket, MessageType.ROOM_FULL);
        }

        public static void SendRoomAlreadyExistsMessage(Socket socket)
        {
            SendMessage(socket, MessageType.ROOM_ALREADY_EXISTS);
        }

        public static void SendRoomDoesNotExistMessage(Socket socket)
        {
            SendMessage(socket, MessageType.ROOM_DOES_NOT_EXIST);
        }

        public static void SendGamePlayerListMessage(List<User> players)
        {
            UserInfo[] infos = new UserInfo[players.Count];
            for (int i = 0; i < players.Count; ++i)
            {
                infos[i] = players[i].GetInfo();
            }
            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].GetUsername() == null || players[i].GetSocket() == null)
                    continue;
                SendMessage(players[i].GetSocket(), MessageType.GAME_PLAYER_LIST, SerializationUtils.SerializeObject(infos));
            }
            //BroadcastToUsersInRoom(room, MessageType.GAME_PLAYER_LIST, SerializationUtils.SerializeObject(players));
        }

        public static void SendMoveToRoomMessage(Socket socket, ChatRoom room)
        {
            RoomInfo info = room.GetRoomInfo();
            SendMessage(socket, MessageType.MOVE_TO_ROOM, SerializationUtils.SerializeObject(info));
        }

        public static void SendScoreUpdate(UserInfo[] userInfos, ChatRoom room)
        {
            BroadcastToUsersInRoom(room, MessageType.GAME_SCORE_UPDATE, SerializationUtils.SerializeObject(userInfos));
            //SendMessage(socket, MessageType.GAME_SCORE_UPDATE, SerializationUtils.SerializeObject(userInfos));
        }

        public static void SendUserDetailsMessage(Socket socket, UserInfo userInfo)
        {
            SendMessage(socket, MessageType.USER_DETAILS, SerializationUtils.SerializeObject(userInfo));
        }

        public static void SendNicknameChangedMessage(ChatRoom room, string oldNickname, string newNickname)
        {
            string[] args = new string[] { oldNickname, newNickname };
            BroadcastToUsersInRoom(room, MessageType.NICKNAME_CHANGED, SerializationUtils.SerializeObject(args));
        }

        public static void SendNicknameChangeSuccessMessage(Socket socket, string newNickname)
        {
            SendMessage(socket, MessageType.NICKNAME_CHANGED_SUCCESS, SerializationUtils.SerializeObject(newNickname));
        }

        public static void SendNicknameNotAvailableMessage(Socket socket)
        {
            SendMessage(socket, MessageType.NICKNAME_NOT_AVAILABLE);
        }

        public static void BroadcastRoomInfos()
        {
            List<User> userList = userManager.GetUserList();
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetSocket() != null)
                    SendMessage(userList[i].GetSocket(), MessageType.ROOM_LIST, SerializationUtils.SerializeObject(chatRoomManager.GetRoomInfos()));
            }
        }

        public static void SendInvalidCommandMessage(Socket socket)
        {
            SendMessage(socket, MessageType.INVALID_COMMAND);
        }
    }
}
