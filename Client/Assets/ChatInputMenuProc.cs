using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatInputMenuProc : MonoBehaviour
{
    GameObject inputFieldGo, chatHistoryBackgroundGo;
    InputField inputFieldCo;
    GameObject chatHistoryText;

    public void Start()
    {
        inputFieldGo = GameObject.Find("ChatInputMenu");
        inputFieldCo = inputFieldGo.GetComponent<InputField>();

        chatHistoryBackgroundGo = GameObject.Find("ChatHistoryBackground");
        chatHistoryText = GameObject.Find("ChatHistoryText");
    }

    public void ProcessChat()
    {
        if (inputFieldCo.text != "" && Input.GetKey(KeyCode.Return))
        {
            Message.SendMessage(MessageType.CHAT, inputFieldCo.text);
            inputFieldCo.text = "";
        }
    }
}
