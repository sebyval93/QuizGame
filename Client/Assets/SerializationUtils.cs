using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

}
