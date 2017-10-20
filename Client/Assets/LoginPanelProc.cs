using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoginPanelProc : MonoBehaviour {

    public GameObject networkGO;
    public InputField usernameInput;
    public InputField passwordInput;
    Dropdown serverDropdown;
    NetworkClient networkClient;

	// Use this for initialization
	void Start ()
    {
        serverDropdown = gameObject.transform.FindChild("ServerDropdown").gameObject.GetComponent<Dropdown>();
        if (!JSONReader.ReadFile())
        {
            serverDropdown.options.Add(new Dropdown.OptionData("Localhost"));
            serverDropdown.RefreshShownValue();
        }
        else
        {
            List<string> optionList = JSONReader.GetUrlList();
            for (int i = 0; i < optionList.Count; ++i)
            {
                serverDropdown.options.Add(new Dropdown.OptionData(optionList[i]));
            }

            serverDropdown.options.Add(new Dropdown.OptionData("Localhost"));
            serverDropdown.RefreshShownValue();
        }
	}

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void Login()
    {
        NetworkClient.address = serverDropdown.options[serverDropdown.value].text;
        string username = usernameInput.text;
        string hashedPass = NetworkClient.hashPassword(passwordInput.text, usernameInput.text);
        networkClient = GameObject.Find("Networking").GetComponent<NetworkClient>();
        networkClient.Connect(serverDropdown.options[serverDropdown.value].text);

        Message.SendMessage(MessageType.LOGIN_REQUEST, SerializationUtils.SerializeObject(new LoginInfo(username, hashedPass)));

    }

    public void Register()
    {
        NetworkClient.address = serverDropdown.options[serverDropdown.value].text;
        string username = usernameInput.text;
        string hashedPass = NetworkClient.hashPassword(passwordInput.text, usernameInput.text);
        networkClient = GameObject.Find("Networking").GetComponent<NetworkClient>();
        networkClient.Connect(serverDropdown.options[serverDropdown.value].text);

        Message.SendMessage(MessageType.CREATE_USER_REQUEST, SerializationUtils.SerializeObject(new LoginInfo(username, hashedPass)));
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
