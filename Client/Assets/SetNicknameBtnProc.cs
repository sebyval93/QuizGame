using UnityEngine;
using UnityEngine.UI;

public class SetNicknameBtnProc : MonoBehaviour {

    GameObject nicknameInputGO;
    InputField nicknameInputCO;

    // Use this for initialization
    void Start () {
        nicknameInputGO = GameObject.Find("ChatMenuNicknameField");
        nicknameInputCO = nicknameInputGO.GetComponent<InputField>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        if (nicknameInputCO.text != "" && nicknameInputCO.text.IndexOf(':') == -1)
            Message.SendMessage(MessageType.SET_NICKNAME, nicknameInputCO.text);
    }
}
