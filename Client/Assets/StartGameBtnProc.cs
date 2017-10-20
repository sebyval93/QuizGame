using UnityEngine;
using UnityEngine.UI;

public class StartGameBtnProc : MonoBehaviour {

    private GameObject roomLabel;
    private Text roomLabelText;
    private static Button startGameButton;

	// Use this for initialization
	void Start () {
        roomLabel = GameObject.Find("ChatMenuRoomLabel");
        roomLabelText = roomLabel.GetComponent<Text>();
        startGameButton = gameObject.GetComponent<Button>();

        startGameButton.interactable = false;
	}
	
    public void OnClick()
    {
        GameManager.StartGame();
    }

    public static void Enable()
    {
        if (startGameButton != null)
            startGameButton.interactable = true;
    }

    public static void Disable()
    {
        if (startGameButton != null)
            startGameButton.interactable = false;
    }

}
