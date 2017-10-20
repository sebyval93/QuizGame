using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameServer
{
    class InetUtils
    {
        private InetUtils() { }

        /*
        public static void SendInetMessage(string message, Socket socket, byte[] objectData = null)
        {
            Thread.Sleep(100);
            InetMessage inetMsg = new InetMessage();

            if (message == null)
            {
                Console.WriteLine("\n!!Error sending InetMessage: null message!");

                return;
            }

            if (socket == null)
            {
                Console.WriteLine("\n!!Error sending InetMessage: null socket!");

                return;
            }

            inetMsg.message = message;
            inetMsg.objectData = objectData;

            byte[] inetData = SerializationUtils.SerializeInetMessage(inetMsg);
            int messageLen = inetData.Length;
            byte[] dataToSend = new byte[sizeof(int) + inetData.Length];
            byte[] prefixBytes = BitConverter.GetBytes(messageLen);
            Buffer.BlockCopy(prefixBytes, 0, dataToSend, 0, sizeof(int));
            Buffer.BlockCopy(inetData, 0, dataToSend, sizeof(int), inetData.Length);

            if (inetData == null || dataToSend == null)
            {
                Console.WriteLine("\n!!Error sending InetMessage: serialized InetMessage null!");

                return;
            }

            //socket.Send(inetData);
            ServerManager.Instance.Send(socket, dataToSend);

            Console.WriteLine("\nSent message " + inetMsg.message + (inetMsg.objectData == null ? "" : " with additional object data!") + " to socket: " + socket.Handle.ToInt32());

        }
        */
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

            Console.WriteLine("\nReceived message of type " + result);

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
    }
}
