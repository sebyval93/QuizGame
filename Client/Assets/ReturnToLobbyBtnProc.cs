using UnityEngine;
using System.Collections;

public class ReturnToLobbyBtnProc : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        GameManager.LeaveGame();
    }
}
