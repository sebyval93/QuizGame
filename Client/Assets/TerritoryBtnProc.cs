using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TerritoryBtnProc : MonoBehaviour {

    public void OnClick()
    {
        Debug.Log(gameObject.name);
        Button button = gameObject.GetComponent<Button>();

        if (button.interactable)
        {
            int attackedTerritory = GetTerritory(gameObject);

            if (!(attackedTerritory < 0) || !(attackedTerritory > 7))
                Message.SendMessage(MessageType.GAME_TERRITORY_ATTACKED, new byte[] { (byte)attackedTerritory });

            UIManager.DisableButtons();
        }

    }

    public int GetTerritory(GameObject territory)
    {
        string territoryName = territory.name;
        switch (territoryName)
        {
            case "TerritoryBtn0":
                return 0;
            case "TerritoryBtn1":
                return 1;
            case "TerritoryBtn2":
                return 2;
            case "TerritoryBtn3":
                return 3;
            case "TerritoryBtn4":
                return 4;
            case "TerritoryBtn5":
                return 5;
            case "TerritoryBtn6":
                return 6;
            case "TerritoryBtn7":
                return 7;
        }

        return -1;
    }

}
