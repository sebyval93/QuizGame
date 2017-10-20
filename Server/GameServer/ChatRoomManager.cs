using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class ChatRoomManager
    {

        private static readonly ChatRoomManager instance = new ChatRoomManager();
        private static List<ChatRoom> chatRoomList;

        private ChatRoomManager()
        {
            chatRoomList = new List<ChatRoom>();
        }

        public static ChatRoomManager Instance
        {
            get
            {
                return instance;
            }
        }

        public void CreateLobby()
        {
            ChatRoom room = new ChatRoom("Lobby");

            if (RoomExists("Lobby"))
            {
                Console.WriteLine("\nError: Attempted to create an additional Lobby!");

                return;
            }

            chatRoomList.Add(room);

            Console.WriteLine("Lobby created!");
        }

        public void StartGame(Socket current)
        {
            User user = GetUser(current);
            ChatRoom room = GetRoomOfUser(user);

            if (room.GetGame() != null)
            {
                Console.WriteLine("Game already started!");

                return;
            }
            else
            {

                room.StartGame(user);


            }
        }

        public ChatRoom CreateRoom(string roomName, User user)
        {
            ChatRoom room = new ChatRoom(roomName);

            if (user != null)
            {
                if (RoomExists(roomName))
                {
                    Console.WriteLine("\nRoom " + roomName + " already exists!");

                    return null;
                }

                chatRoomList.Add(room);
                ChangeRoom(roomName, user);


                Console.WriteLine("\nRoom " + roomName + " created!");

                return room;
            }

            return null;
        }

        public void AddUserToRoom(User user, string roomName)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetName() == roomName)
                {
                    chatRoomList[i].AddUser(user);
                }
            }
        }

        public void RemoveRoom(string roomName)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetName() == roomName)
                {
                    chatRoomList.RemoveAt(i);
                }
            }
        }

        public ChatRoom GetRoom(string roomName)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetName() == roomName)
                {
                    return chatRoomList[i];
                }
            }

            return null;
        }

        public User GetUser(Socket socket)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetUser(socket) != null)
                {
                    return chatRoomList[i].GetUser(socket);
                }
            }

            Console.WriteLine("\nCould not get user!");
            return null;
        }

        public ChatRoom GetRoomOfUser(User user)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetUser(user.GetSocket()) != null)
                {
                    return chatRoomList[i];
                }
            }

            Console.WriteLine("\n*CRITICAL* Could not get chatroom of user!");
            return null;
        }

        public string GetRoomNameOfUser(User user)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetUser(user.GetSocket()) != null)
                {
                    return chatRoomList[i].GetName();
                }
            }

            Console.WriteLine("\n*CRITICAL* Could not get chatroom of user!");
            return null;
        }

        public bool RemoveUserFromRoom(string roomName, User user)
        {
            if (!RoomExists(roomName))
            {
                Console.WriteLine("\nError removing user from room. Room does not exist!");
                return false;
            }

            ChatRoom room = GetRoom(roomName);
            room.RemoveUser(user.GetSocket());

            if (room.CountUsers() == 0 && room.GetName() != "Lobby")
            {
                RemoveRoom(room.GetName());
            }

            return true;
        }

        public bool RoomExists(string roomName)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].GetName() == roomName)
                    return true;
            }

            return false;
        }

        public bool NicknameExists(string nickname)
        {
            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                if (chatRoomList[i].NicknameExists(nickname))
                    return true;
            }

            return false;
        }

        public bool ChangeRoom(string roomName, User user)
        {
            ChatRoom newRoom, roomOfUser;
            //check if room exists
            if (!RoomExists(roomName))
            {
                Console.WriteLine("\nUser attempted to switch to room \"" + roomName + "\". The specified room does not exist.");
                return false;
            }

            //find user
            roomOfUser = GetRoomOfUser(user);
            RemoveUserFromRoom(roomOfUser.GetName(), user);

            if (!roomOfUser.GetName().Equals("Lobby") && roomOfUser.CountUsers() == 0)
            {
                roomOfUser = null;
                Message.BroadcastRoomInfos();
            }

            newRoom = GetRoom(roomName);

            AddUserToRoom(user, newRoom.GetName());

            Console.WriteLine("\nUser switched to room " + roomName + " successfully");

            return true;
        }

        public void JoinRoom(string roomName, Socket current)
        {
            List<string> connUserList = new List<string>();

            User user = GetUser(current);

            ChangeRoom(roomName, user);
        }

        public RoomInfo[] GetRoomInfos()
        {
            RoomInfo[] roomInfos = new RoomInfo[chatRoomList.Count];

            for (int i = 0; i < chatRoomList.Count; ++i)
            {
                roomInfos[i].roomName = chatRoomList[i].GetName();
                roomInfos[i].numUsers = chatRoomList[i].CountUsers();
            }

            return roomInfos;
        }

    }
}
