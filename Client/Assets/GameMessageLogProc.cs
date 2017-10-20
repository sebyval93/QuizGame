using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameMessageLogProc : MonoBehaviour {

    public GameObject GameMessageLog;
    private static Text text;

    void Awake()
    {
        text = GameMessageLog.GetComponent<Text>();
        GameMessageLog.GetComponent<ScrollRect>().verticalNormalizedPosition = 0.5f;
        text.text += "\nGame started.";
    }
    
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {

	}

    public static void PlayerTurn()
    {
        text.text += "\nYour turn!";
    }

    public static void PlayerTurnEnded()
    {
        text.text += "\nYour turn ended!";
    }

    public static void CorrectAnswer()
    {
        text.text += "\nCorrect!";
    }

    public static void WrongAnswer()
    {
        text.text += "\nIncorrect!";
    }

    public static void DebugMessage(string message)
    {
        text.text += "\nDebug: " + message;
    }

    public static void InvalidMove()
    {
        text.text += "\nInvalid territory!";
    }

    public static void Clear()
    {
        text.text = "";
    }
}
