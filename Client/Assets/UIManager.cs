using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

    // vars that will show up in the inspector
    public GameObject chatPane, loginPane, gamePane, questionPane, gameWonPane, gameLostPane;
    public GameObject player1Label, player2Label, player3Label;
    public GameObject player1ScoreLabel, player2ScoreLabel, player3ScoreLabel;
    public GameObject[] territoryBtn;
    public Button returnToLobbyBtn;

    private static GameObject chatPanel, loginPanel, gamePanel, questionPanel, gameWonPanel, gameLostPanel;
    private static GameObject firstPlayerLabel, secondPlayerLabel, thirdPlayerLabel;
    private static Text player1ScoreText, player2ScoreText, player3ScoreText;
    private static Button[] territory = new Button[8];
    public static Button returnToLobbyButton;

    static int scoreRed = 0, scoreBlue = 0, scoreGreen = 0;
    public static string playerNickname;
    public static Color playerColor;
    public static int finalScore = 0;

    void Awake()
    {
        for (int i = 0; i < 8; ++i)
        {
            territory[i] = territoryBtn[i].GetComponentInChildren<Button>();
        }
    }

	// Use this for initialization
	void Start () {
        chatPanel = chatPane;
        loginPanel = loginPane;
        gamePanel = gamePane;
        questionPanel = questionPane;
        gameWonPanel = gameWonPane;
        gameLostPanel = gameLostPane;

        returnToLobbyButton = returnToLobbyBtn;

        //this is atrocious
        firstPlayerLabel = player1Label;
        secondPlayerLabel = player2Label;
        thirdPlayerLabel = player3Label;

        player1ScoreText = player1ScoreLabel.GetComponent<Text>();
        player2ScoreText = player2ScoreLabel.GetComponent<Text>();
        player3ScoreText = player3ScoreLabel.GetComponent<Text>();

        scoreRed = 0;
        scoreBlue = 0;
        scoreGreen = 0;

        //show only the login panel on start
        loginPane.SetActive(true);
        chatPanel.SetActive(false);
        gamePanel.SetActive(false);
        questionPanel.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void SwitchToGame()
    {
        StartGameBtnProc.Disable();
        gamePanel.SetActive(true);
        chatPanel.SetActive(false);
        questionPanel.SetActive(false);
    }

    public static void SwitchToChat()
    {
        chatPanel.SetActive(true);
        loginPanel.SetActive(false);
        gamePanel.SetActive(false);
        questionPanel.SetActive(false);
        gameWonPanel.SetActive(false);
        gameLostPanel.SetActive(false);
    }

    public static void SwitchToGameWon(int score)
    {
        finalScore = score;
        gameWonPanel.SetActive(true);
        gamePanel.SetActive(false); 
    }

    public static void SwitchToGameLost()
    {
        gameLostPanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    public static void ShowQuestion(QuestionInfo question)
    {
        questionPanel.SetActive(true);
        QuestionProc.PopulateQuestionFields(question);
    }

    public static void HideQuestion()
    {
        questionPanel.SetActive(false);
    }

    public static void SetPlayerNickname(string nickname)
    {
        playerNickname = nickname;
    }

    public static void LoadPlayers(UserInfo[] players)
    {
        firstPlayerLabel.GetComponent<Text>().text = players[0].nickname;
        secondPlayerLabel.GetComponent<Text>().text = players[1].nickname;
        thirdPlayerLabel.GetComponent<Text>().text = players[2].nickname;

        if (players[0].nickname.Equals(playerNickname))
            playerColor = Color.red;
        else if (players[1].nickname.Equals(playerNickname))
            playerColor = Color.blue;
        else
            playerColor = Color.green;
    }

    public static void UpdateGameBoard(byte[] gameBoard)
    {
        Sprite territoryBlack = Resources.Load<Sprite>("Soldier");
        Sprite territoryRed = Resources.Load<Sprite>("SoldierRed");
        Sprite territoryBlue = Resources.Load<Sprite>("SoldierBlue");
        Sprite territoryGreen = Resources.Load<Sprite>("SoldierGreen");

        for (int i = 0; i < 8; ++i)
        {
            switch (gameBoard[i])
            {
                case 0:
                    territory[i].image.sprite = territoryRed;
                    break;

                case 1:
                    territory[i].image.sprite = territoryBlue;
                    break;

                case 2:
                    territory[i].image.sprite = territoryGreen;
                    break;

                case 255:
                    territory[i].image.sprite = territoryBlack;
                    break;
            }

        }

    }

    public static void ClearGameBoard()
    {
        Sprite territoryBlack = Resources.Load<Sprite>("Soldier");

        for (int i = 0; i < 8; ++i)
        {
            territory[i].image.sprite = territoryBlack;
        }
    }

    public static void ClearScore()
    {
        player1ScoreText.text = "Score: " + 0;
        player2ScoreText.text = "Score: " + 0;
        player3ScoreText.text = "Score: " + 0;
    }

    public static void ClearGameLog()
    {
        GameMessageLogProc.Clear();
    }

    public static void UpdateScore(UserInfo[] scores)
    {
        scoreRed = scores[0].totalScore;
        scoreBlue = scores[1].totalScore;
        scoreGreen = scores[2].totalScore;

        player1ScoreText.text = "Score: " + scoreRed.ToString();
        player2ScoreText.text = "Score: " + scoreBlue.ToString();
        player3ScoreText.text = "Score: " + scoreGreen.ToString();

    }

    public static void DisableButtons()
    {
        for (int i = 0; i < 8; ++i)
        {
            territory[i].interactable = false;
        }
    }

    // enable all non self-owned territories
    public static void EnableButtons()
    {
        string spriteName;
        for (int i = 0; i < 8; ++i)
        {
            spriteName = territory[i].image.sprite.name;
            if (playerColor == Color.red && spriteName == "SoldierRed" || playerColor == Color.blue && spriteName == "SoldierBlue" || playerColor == Color.green && spriteName == "SoldierGreen")
                territory[i].interactable = false;
            else
                territory[i].interactable = true;
        }
    }

    public static void InitialTurnNoAttack()
    {
        EnableButtons();
        for (int i = 0; i < 8; ++i)
        {
            Debug.Log(territory[i].image.sprite.name);
            if (territory[i].image.sprite.name != "Soldier")
                territory[i].interactable = false;
        }
    }

    public static void SetupTurn()
    {
        EnableButtons();
    }

    public static void EndTurn()
    {
        DisableButtons();
    }

    public static void InvalidMove()
    {
        GameMessageLogProc.InvalidMove();
    }

    public static void SetChatUserList(UserInfo[] users)
    {
        ConnUsersMenuProc.SetUserList(users);
    }

    public static void AddUserToList(UserInfo user)
    {
        ConnUsersMenuProc.AddToUserList(user);
    }

    public static void RemoveUserFromList(UserInfo user)
    {
        ConnUsersMenuProc.RemoveFromUserList(user);
    }

    public static bool IsChatRoomActive()
    {
        if (chatPanel.activeInHierarchy)
            return true;
        else
            return false;
    }

    public static void RenameUserInList(string newNickname, string oldNickname)
    {
        ChatHistoryTextProc.NicknameChanged(newNickname, oldNickname);
        ConnUsersMenuProc.RenameUser(newNickname, oldNickname);
    }

    public static void SwitchToRoom(string roomName)
    {
        if (roomName.Equals("Lobby"))
            returnToLobbyButton.interactable = false;
        else
            returnToLobbyButton.interactable = true;


        ChatMenuRoomLabelProc.ChangeRoom(roomName);
    }
}
