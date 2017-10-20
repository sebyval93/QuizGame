using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class User
    {
        private int score;
        private string username;
        private string nickname;
        private Socket socket;
        private string authToken;

        public User()
        {
            nickname = null;
            score = 0;
            socket = null;
            authToken = null;
        }

        public User(string username, int score, string nickname = null, Socket socket = null)
        {
            this.username = username;
            this.score = score;
            this.nickname = nickname;
            this.socket = socket;
        }

        public void SetNickName(string nickname)
        {
            this.nickname = nickname;
        }

        public void SetSocket(Socket socket)
        {
            this.socket = socket;
        }

        public string GetNickname()
        {
            return nickname;
        }

        public string GetUsername()
        {
            return username;
        }

        public UserInfo GetInfo()
        {
            return new UserInfo(nickname, score, false);
        }

        public Socket GetSocket()
        {
            return socket;
        }

        public void SetToken(string newToken)
        {
            authToken = newToken;
        }

        public bool ValidateToken(string receivedToken)
        {
            if (authToken == receivedToken)
                return true;
            else
                return false;
        }

        public int GetScore()
        {
            return score;
        }

    }
}
