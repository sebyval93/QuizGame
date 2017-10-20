using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class ChatHistoryTextProc : MonoBehaviour {

    private static ScrollRect scrollRect;
    static Text text;

    private const string serverMessageMarker = ">> ";

	// Use this for initialization
	void Start () {
        text = gameObject.GetComponent<Text>();
        text.text = "";
        
        scrollRect = gameObject.transform.parent.gameObject.GetComponent<ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0.0f;

        UIManager.returnToLobbyButton.interactable = false;
	}

    public static void PrintChat(string message)
    {
        text.text += "\n" + message;
        scrollRect.verticalNormalizedPosition = 0.0f;
        Canvas.ForceUpdateCanvases();
    }

    public static void NicknameChanged(string newNickname, string oldNickname)
    {
        text.text += "\n" + serverMessageMarker + oldNickname + " is now known as " + newNickname;
    }

    public static void NicknameChangeSuccessful()
    {
        text.text += "\n" + serverMessageMarker + "nickname change successful!";
    }

    public static void NicknameExists(string nickname)
    {
        text.text += "\n" + serverMessageMarker + "the nickname \"" + nickname + "\" is already in use";
    }

    public static void UserJoinedRoom(UserInfo user)
    {
        text.text += "\n" + serverMessageMarker + user.nickname + " joined the room";
    }

    public static void UserLeftRoom(UserInfo user)
    {
        text.text += "\n" + serverMessageMarker + user.nickname + " left the room";
    }

    public static void RoomDoesntExist()
    {
        text.text += "\n" + serverMessageMarker + "that room doesn't exist!";
    }

    public static void RoomAlreadyExists()
    {
        text.text += "\n" + serverMessageMarker + "that room already exists!";
    }

    public static void AlreadyInRoom()
    {
        text.text += "\n" + serverMessageMarker + "you are already in that room!";
    }

    public static void RoomIsFull()
    {
        text.text += "\n" + serverMessageMarker + "that room is full!";
    }

    public static void GameInProgress()
    {
        text.text += "\n" + serverMessageMarker + "A game is in progress.";
    }
}
