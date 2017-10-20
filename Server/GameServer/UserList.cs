using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class UserList
    {
        private List<User> userList;

        public UserList()
        {
            userList = new List<User>();
        }

        public void Add(User user)
        {
            userList.Add(user);
        }

        public void RemoveUser(Socket socket)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetSocket() == socket)
                    userList.RemoveAt(i);
            }
        }

        public bool NicknameExists(string nickname)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetNickname() == nickname)
                    return true;
            }

            return false;
        }

        public void RemoveUserWithNick(string nick)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetNickname() == nick)
                    userList.RemoveAt(i);
            }
        }

        public void SetNickname(Socket socket, string nick)
        {
            for (int i = 0; i < userList.Count; ++i)
            {
                if (userList[i].GetSocket() == socket)
                    userList[i].SetNickName(nick);
            }
        }

        public int GetCount()
        {
            return userList.Count;
        }

        public User this[int i]
        {
            get { return userList[i]; }
            set { userList[i] = value; }
        }

        public void RemoveAt(int index)
        {
            userList.RemoveAt(index);
        }


    }
}
