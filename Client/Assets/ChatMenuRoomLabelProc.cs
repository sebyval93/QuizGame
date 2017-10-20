using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatMenuRoomLabelProc : MonoBehaviour {

    static Text text;

    // Use this for initialization
    void Start () {
        text = gameObject.GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void ChangeRoom(string roomName)
    {
        if (text != null)
            text.text = "Room: " + roomName;
    }
}
