using UnityEngine;
using System.Collections;

public class GameQuestionAnswerBtnProc : MonoBehaviour {

    private static int questionID = -1;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    public void Answer1Click()
    {
        QuestionAnswer ans = new QuestionAnswer();
        ans.id = questionID;
        ans.option = 1;
        Message.SendMessage(MessageType.GAME_QUESTION_ANSWER, ans);
    }

    public void Answer2Click()
    {
        QuestionAnswer ans = new QuestionAnswer();
        ans.id = questionID;
        ans.option = 2;
        Message.SendMessage(MessageType.GAME_QUESTION_ANSWER, ans);
    }

    public void Answer3Click()
    {
        QuestionAnswer ans = new QuestionAnswer();
        ans.id = questionID;
        ans.option = 3;
        Message.SendMessage(MessageType.GAME_QUESTION_ANSWER, ans);
    }

    public void Answer4Click()
    {
        QuestionAnswer ans = new QuestionAnswer();
        ans.id = questionID;
        ans.option = 4;
        Message.SendMessage(MessageType.GAME_QUESTION_ANSWER, ans);
    }

    public static void SetQuestionID(int id)
    {
        questionID = id;
    }

    public static int GetQuestionID()
    {
        return questionID;
    }
}
