using System;

public enum MessageType : uint
{
    // server sends and receives
    CHAT,

    // server sends
    GAME_INITIAL_TURN,
    GAME_TURN,
    GAME_TURN_END,
    GAME_BOARD_UPDATE,
    GAME_IN_PROGRESS,
    GAME_WON,
    GAME_LOST,
    GAME_QUESTION,
    GAME_INVALID_MOVE,
    GAME_STARTED,
    GAME_CORRECT,
    GAME_INCORRECT,
    GAME_SCORE_UPDATE,
    USERINFO_DATA,
    USER_JOINED_ROOM,
    USER_LEFT_ROOM,
    USER_ALREADY_IN_ROOM,
    ROOM_FULL,
    ROOM_ALREADY_EXISTS,
    ROOM_DOES_NOT_EXIST,
    ROOM_LIST,
    GAME_PLAYER_LIST,
    MOVE_TO_ROOM,
    LOGIN_FAILURE,
    USER_ALREADY_EXISTS,
    NICKNAME_CHANGED,
    NICKNAME_CHANGED_SUCCESS,
    NICKNAME_NOT_AVAILABLE,
    INVALID_COMMAND,
    USER_DETAILS,

    //server receives
    CHAT_COMMAND,
    CREATE_USER_REQUEST,
    LOGIN_REQUEST,
    SET_NICKNAME,
    CREATE_ROOM_REQUEST,
    JOIN_ROOM_REQUEST,
    JOIN_LOBBY_REQUEST,
    START_GAME_REQUEST,
    GAME_QUESTION_ANSWER,
    GAME_TERRITORY_ATTACKED,
    CLIENT_DISCONNECTION

}

[Serializable]
public struct InetMessage
{
    public uint id;
    public byte[] objectData;
}

[Serializable]
public struct QuestionInfo
{
    public int id;
    public string questionText;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
}

[Serializable]
public struct QuestionAnswer
{
    public int id;
    public int option;
}

[Serializable]
public struct RoomInfo
{
    public RoomInfo(string roomName, int numUsers)
    {
        this.roomName = roomName;
        this.numUsers = numUsers;
    }
    public string roomName;
    public int numUsers;
}

[Serializable]
public struct UserInfo
{
    public UserInfo(string nickname, int totalScore, bool isAI)
    {
        this.nickname = nickname;
        this.totalScore = totalScore;
        this.isAI = isAI;
    }
    public string nickname;
    public int totalScore;
    public bool isAI;
}

[Serializable]
public struct LoginInfo
{
    public LoginInfo(string username, string hash)
    {
        this.username = username;
        this.hash = hash;
    }
    public string username;
    public string hash;
}

[Serializable]
public struct PlayerList
{
    public string[] players;
}