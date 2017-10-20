using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public bool playedBefore = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Awake()
    {
        UIManager.DisableButtons();
    }

    void OnEnable()
    {
        if (playedBefore)
        {
            UIManager.ClearGameBoard();
            UIManager.ClearScore();
            UIManager.ClearGameLog();
            Awake();
        }

        playedBefore = true;
    }

    public static void StartGame()
    {
        Message.SendMessage(MessageType.START_GAME_REQUEST);
    }

    public static void LoadPlayers(UserInfo[] players)
    {
        UIManager.LoadPlayers(players);
    }

    public static void SetupInitialTurn()
    {
        GameMessageLogProc.PlayerTurn();
        UIManager.InitialTurnNoAttack();
    }

    public static void SetupTurn()
    {
        GameMessageLogProc.PlayerTurn();
        UIManager.SetupTurn();
    }

    public static void EndTurn()
    {
        GameMessageLogProc.PlayerTurnEnded();
        UIManager.EndTurn();
    }

    public static void WinGame(int score)
    {
        UIManager.SwitchToGameWon(score);
    }

    public static void LoseGame()
    {
        UIManager.SwitchToGameLost();
    }

    public static void LeaveGame()
    {
        UIManager.SwitchToChat();
        Message.SendMessage(MessageType.JOIN_ROOM_REQUEST, "Lobby");
    }

    public static void UpdateScore(UserInfo[] scores)
    {
        UIManager.UpdateScore(scores);
    }

    public static void UpdateGameBoard(byte[] board)
    {
        UIManager.UpdateGameBoard(board);
    }

    public static void AskQuestion(QuestionInfo question)
    {
        UIManager.ShowQuestion(question);
    }

    public static void InvalidMove()
    {
        UIManager.InvalidMove();
    }

    public static void CorrectAnswer()
    {
        GameMessageLogProc.CorrectAnswer();
    }

    public static void IncorrectAnswer()
    {
        GameMessageLogProc.WrongAnswer();
    }
}
