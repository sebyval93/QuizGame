using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CreateRoomBtnProc : MonoBehaviour {

    GameObject createRoomInputGO;
    InputField createRoomInputCO;

    // Use this for initialization
    void Start () {
        createRoomInputGO = GameObject.Find("CreateRoomInput");
        createRoomInputCO = createRoomInputGO.GetComponent<InputField>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        if (createRoomInputCO.text != "")
        {
            Message.SendMessage(MessageType.CREATE_ROOM_REQUEST, createRoomInputCO.text);
            StartGameBtnProc.Enable();
        }
    }
}
