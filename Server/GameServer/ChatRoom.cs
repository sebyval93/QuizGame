using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class ChatRoom
    {
        private string name;
        //private UserList userList;
        private List<User> userList;
        private Game game;
        private bool gameStarted = false;

        public ChatRoom()
        {
            name = "";
            userList = new List<User>();
            game = null;
        }

        public ChatRoom(string name)
        {
            this.name = name;
            userList = new List<User>();
        }

        public void AddUser(User user)
        {
            userList.Add(user);
        }

        public List<User> GetUserList()
        {
            return userList;
        }

        public UserInfo[] GetUserInfo()
        {
            UserInfo[] users = new UserInfo[userList.Count];
            for (int i = 0; i < userList.Count; ++i)
            {
                users[i].nickname = userList[i].GetNickname();
                users[i].totalScore = userList[i].GetScore();
                users[i].isAI = false;
            }

            return users;
        }

        public UserInfo[] GetPlayerList()
        {
            UserInfo[] players = new UserInfo[3];
            for (int i = 0; i < userList.Count; ++i)
            {
                players[i].nickname = userList[i].GetNickname();
                players[i].totalScore = userList[i].GetScore();
                players[i].isAI = false;
            }

            int aiPlayers = 3 - userList.Count;
            for (int i = 0; i < aiPlayers; ++i)
            {
                players[userList.Count + i].nickname = "AIPlayer" + (i + 1);
                players[userList.Count + i].totalScore = 0;
                players[userList.Count + i].isAI = true;
            }

            return players;
        }

        public string[] GetUserNames()
        {
            if (userList.Count > 0)
            {
                string[] users = new string[userList.Count];
                for (int i = 0; i < userList.Count; ++i)
                {
                    users[i] = userList[i].GetNickname();
                }
                return users;
            }

            return null;
        }

        public RoomInfo GetRoomInfo()
        {
            RoomInfo info = new RoomInfo(name, userList.Count);

            return info;
        }

        public int CountUsers()
        {
            return userList.Count;
        }

        public void NicknameChanged(User user)
        {
            Console.WriteLine(user.GetNickname());
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetUsername().Equals(user.GetUsername()))
                    Console.WriteLine(userList[i].GetNickname());
            }
        }

        public void RemoveUser(Socket socket) 
        {
            int index = userList.FindIndex(x => x.GetSocket() == socket);
            userList.RemoveAt(index);
        }

        public void RemoveUserWithNick(string nickname)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetNickname() == nickname)
                {
                    userList.RemoveAt(i);
                }
            }
        }

        public bool NicknameExists(string nickname)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetNickname() == nickname)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return this.name;
        }

        public User GetUser(Socket socket)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetSocket() == socket)
                {
                    return userList[i];
                }
            }

            return null;
        }

        public User GetUserAtIndex(int i)
        {
            return userList[i];
        }

        public void StartGame(User user)
        {
            gameStarted = true;
            if (game != null)
            {
                Console.WriteLine("Error: Game already started.");

                return;
            }
            else
                game = new Game(userList, this);//, user);
        }

        public bool GameStarted()
        {
            return gameStarted;
        }

        public Game GetGame()
        {
            return game;
        }
    }
}
