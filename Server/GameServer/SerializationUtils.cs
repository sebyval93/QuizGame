using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class SerializationUtils
    {
        public static byte[] SerializeInetMessage(InetMessage inetMsg)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, inetMsg);

            return ms.ToArray();
        }

        public static InetMessage DeserializeInetMessage(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (InetMessage)formatter.Deserialize(stream);
        }

        public static byte[] SerializeObject(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        public static object DeserializeObject(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return formatter.Deserialize(stream);
        }

        /*
        public static byte[] SerializeQuestion(QuestionInfo question)
        {

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, question);

            return ms.ToArray();
        }

        public static byte[] SerializeConnUserList(List<string> connUserList)
        {
            if (connUserList == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, connUserList);

            return ms.ToArray();
        }

        public static byte[] SerializePlayerList(List<string> playerList)
        {
            if (playerList == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, playerList);

            return ms.ToArray();
        }

        public static byte[] SerializeString(string arr)
        {
            //byte[] bytes = arr.Select(s => Convert.ToByte(s, 16)).ToArray();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, arr);

            return ms.ToArray();
        }

        public static string DeserializeString(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (string)formatter.Deserialize(stream);
        }

        public static byte[] SerializeStringArray(string[] arr)
        {
            //byte[] bytes = arr.Select(s => Convert.ToByte(s, 16)).ToArray();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, arr);

            return ms.ToArray();
        }

        public static string[] DeserializeStringArray(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (string[])formatter.Deserialize(stream);
        }

        public static byte[] SerializeQuestionAnswer(QuestionAnswer ans)
        {
            //byte[] bytes = arr.Select(s => Convert.ToByte(s, 16)).ToArray();
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, ans);

            return ms.ToArray();
        }

        public static QuestionAnswer DeserializeQuestionAnswer(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (QuestionAnswer)formatter.Deserialize(stream);
        }

        public static byte[] SerializeUserInfo(UserInfo userInfo)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, userInfo);

            return ms.ToArray();
        }

        public static UserInfo DeserializeUserInfo(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (UserInfo)formatter.Deserialize(stream);
        }

        public static byte[] SerializeUserInfoArray(UserInfo[] userInfo)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, userInfo);

            return ms.ToArray();
        }

        public static UserInfo[] DeserializeUserInfoArray(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (UserInfo[])formatter.Deserialize(stream);
        }

        public static byte[] SerializeRoomInfo(RoomInfo roomInfo)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, roomInfo);

            return ms.ToArray();
        }

        public static RoomInfo DeserializeRoomInfo(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (RoomInfo)formatter.Deserialize(stream);
        }

        public static byte[] SerializeRoomInfoArray(RoomInfo[] roomInfo)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, roomInfo);

            return ms.ToArray();
        }

        public static RoomInfo[] DeserializeRoomInfoArray(byte[] arr)
        {
            MemoryStream stream = new MemoryStream(arr);
            IFormatter formatter = new BinaryFormatter();
            stream.Position = 0;

            return (RoomInfo[])formatter.Deserialize(stream);
        }

    */
    }
}
