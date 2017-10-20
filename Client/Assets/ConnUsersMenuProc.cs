using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ConnUsersMenuProc : MonoBehaviour {

    static Text text;
    private static List<UserInfo> userList;

    // Use this for initialization
    void Start () {
        text = gameObject.GetComponent<Text>();
        userList = new List<UserInfo>();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public static void DisplayReceived(string received)
    {
        text.text = received;
    }

    public static void DisplayUserList()
    {
        text.text = "";

        for (int i = 0; i < userList.Count; ++i)
        {
            text.text += "\n" + userList[i].nickname + " (" + userList[i].totalScore + ")";
        }
    }

    public static void AddToUserList(UserInfo userInfo)
    {
        userList.Add(userInfo);
        DisplayUserList();
    }

    public static void RemoveFromUserList(UserInfo userInfo)
    { 

        userList.Remove(userInfo);
        DisplayUserList();
    }

    public static void SetUserList(UserInfo[] userInfos)
    {
        userList = new List<UserInfo>(userInfos);
        DisplayUserList();
    }

    public static void RenameUser(string newNickame, string oldNickname)
    {
        try
        {
            UserInfo user = userList.Find(x => x.nickname.Equals(oldNickname));
            UserInfo newUser = new UserInfo(newNickame, user.totalScore, false);
            userList.Remove(user);
            userList.Add(newUser);
            DisplayUserList();
        }
        catch { }
    }
}
