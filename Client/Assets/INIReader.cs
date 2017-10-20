using System;
using System.IO;
using System.Net;
using UnityEngine;

static class INIReader
{
    private static string path;
    private static string iniContent;
    private static string ipString;

    public static string ReadIP()
    {
        path = Path.GetFullPath(".") + "//config.ini";

        try
        {
            iniContent = File.ReadAllText(path);
            if (iniContent.Contains("[Server]\r\nIP="))
            {
                int ipPos = iniContent.IndexOf("IP=") + 3;

                ipString = iniContent.Substring(ipPos);

                IPAddress ipAddress;
                if (IPAddress.TryParse(ipString, out ipAddress))
                {
                    return ipString;
                }
                else
                    throw new Exception();
            }
            return iniContent;
        }
        catch
        {
            File.Create(path).Dispose();
            string contents = "[Server]\r\nIP=127.0.0.1";
            File.WriteAllText(path, contents);

            return "127.0.0.1";
        }
    }

    private static bool IsIPValid(string ip)
    {
        return false;
    }
}