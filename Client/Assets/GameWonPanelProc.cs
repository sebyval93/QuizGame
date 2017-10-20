using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameWonPanelProc : MonoBehaviour {

    // Use this for initialization
    public Text scoreText;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        scoreText.text = UIManager.finalScore.ToString();
	}
}
