using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class QuestionProc : MonoBehaviour {

    public GameObject questionTextGO, answer1, answer2,
        answer3, answer4, timeRemainingGO;

    private static Text questionText;
    private static Text answer1Text, answer2Text, 
        answer3Text, answer4Text, timeRemainingText;

    double timeLeft = 5.0f;
    TimeSpan ts;

    void Awake()
    {
        questionText = questionTextGO.GetComponent<Text>();
        answer1Text = answer1.GetComponentInChildren<Text>();
        answer2Text = answer2.GetComponentInChildren<Text>();
        answer3Text = answer3.GetComponentInChildren<Text>();
        answer4Text = answer4.GetComponentInChildren<Text>();
        timeRemainingText = timeRemainingGO.GetComponentInChildren<Text>();
        timeLeft = 5.0f;
        ts = TimeSpan.FromSeconds(timeLeft);

    }

    void OnEnable()
    {
        timeLeft = 5.0f;
        ts = TimeSpan.FromSeconds(timeLeft);
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0.0f)
        {
            int id = GameQuestionAnswerBtnProc.GetQuestionID();
            QuestionAnswer ans = new QuestionAnswer();
            ans.id = id;
            ans.option = -1;
            Message.SendMessage(MessageType.GAME_QUESTION_ANSWER, ans);
            gameObject.SetActive(false);
        }

        timeRemainingText.text = ts.Seconds.ToString() + ":" + (ts.Milliseconds % 60).ToString();
        ts = TimeSpan.FromSeconds(timeLeft);

    }

    public static void PopulateQuestionFields(QuestionInfo question)
    {
        questionText.text = question.questionText;
        answer1Text.text = question.answer1;
        answer2Text.text = question.answer2;
        answer3Text.text = question.answer3;
        answer4Text.text = question.answer4;

        GameQuestionAnswerBtnProc.SetQuestionID(question.id);
    }
}
