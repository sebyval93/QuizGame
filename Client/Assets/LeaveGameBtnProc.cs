﻿using UnityEngine;
using System.Collections;

public class LeaveGameBtnProc : MonoBehaviour {

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
