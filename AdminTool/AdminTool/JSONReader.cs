using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;

namespace AdminTool
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
            catch (FileNotFoundException ex)
            {
                //GenerateFile
                return false;
            }
        }

        public static void GenerateFile()
        {
            string defaultFile = 
                @"{
                    ""db_connection"":""server = 127.0.0.1; uid = root; pwd = 12345; database = licenta_quiz;"",
                    ""message_size_limit"":5120
                }";

            File.WriteAllText(".\\settings.json", defaultFile);
        }
    }
}
