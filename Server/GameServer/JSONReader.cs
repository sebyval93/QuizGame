using System;
using System.IO;
using LitJson;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class JSONReader
    {
        private static JsonData settingsJSON;

        public static bool ReadFile()
        {
            try
            {
                string strJSON = File.ReadAllText(".\\settings.json");
                settingsJSON = JsonMapper.ToObject(strJSON);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("JSONReader Exception - ReadFile: " + e.Message);
                return false;
            }
        }

        public static string GetDBString()
        {
            if (settingsJSON != null)
                return (string)settingsJSON["db_connection"];
            else
                return "";
        }

        public static int GetMaxMessageSize()
        {
            if (settingsJSON != null)
                return (int)settingsJSON["message_size_limit"];
            else
                return -1;
        }
    }
}
