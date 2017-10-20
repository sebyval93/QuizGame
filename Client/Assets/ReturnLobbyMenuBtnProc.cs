using UnityEngine;
using System.Collections;

public class ReturnLobbyMenuBtnProc : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        Message.SendMessage(MessageType.JOIN_ROOM_REQUEST, "Lobby");
        Debug.Log("click!");
    }
}
