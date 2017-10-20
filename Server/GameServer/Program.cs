using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        private const int _PORT = 4400;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
        //private static UserList _globalUsersList = new UserList();
        private static ServerManager serverManager = ServerManager.Instance;
        /*
                [Serializable]
                public struct InetMessage
                {
                    public string message;
                    public byte[] objectData;
                }
        */

        static void Main()
        {
            Console.Title = "GameServer";
            serverManager.ReadSettings();
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
