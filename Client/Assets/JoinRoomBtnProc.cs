using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JoinRoomBtnProc : MonoBehaviour {

    GameObject joinRoomInputGO;
    InputField joinRoomInputCO;

    // Use this for initialization
    void Start () {
        joinRoomInputGO = GameObject.Find("JoinRoomInput");
        joinRoomInputCO = joinRoomInputGO.GetComponent<InputField>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        if (joinRoomInputCO.text != "")
            Message.SendMessage(MessageType.JOIN_ROOM_REQUEST, joinRoomInputCO.text);
    }
}
