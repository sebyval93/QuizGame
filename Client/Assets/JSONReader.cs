using LitJson;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class JSONReader
{
    private static JsonData settingsJSON;

    public static bool ReadFile()
    {
        try
        {
            string path = Path.GetFullPath(".") + "//settings.json";
            string strJSON = File.ReadAllText(path);
            settingsJSON = JsonMapper.ToObject(strJSON);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("JSONReader Exception - ReadFile: " + e.Message);
            GenerateFile();
            return false;
        }
    }

    public static List<string> GetUrlList()
    {
        List<string> urlList = new List<string>();
        if (settingsJSON != null)
        {
            for (int i = 0; i < settingsJSON["servers"].Count; ++i)
                urlList.Add((string)settingsJSON["servers"][i]);
        }

        return urlList;
    }

    public static void GenerateFile()
    {
        string path = Path.GetFullPath(".") + "//settings.json";

        string servers = @"{
    ""servers"": [

    ]
}";

        File.WriteAllText(path, servers);
    }
}

