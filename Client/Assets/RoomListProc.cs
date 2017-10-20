using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RoomListProc : MonoBehaviour {

    static Text text;

    // Use this for initialization
    void Start()
    {
        text = gameObject.GetComponent<Text>();
        //InvokeRepeating("DisplayUserList", 0.5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }


    public static void DisplayRoomList(RoomInfo[] roomInfo)
    {
        text.text = "";

        for (int i = 0; i < roomInfo.Length; ++i)
        {
            text.text += "\n" + roomInfo[i].roomName + " (" + roomInfo[i].numUsers + ")";
        }
    }
}
