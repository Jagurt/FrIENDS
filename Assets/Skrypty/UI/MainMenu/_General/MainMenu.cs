using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    internal static MainMenu mainMenu;

    internal static Transform startGameMenu;
    internal static Transform joinGameMenu;
    internal static Transform hostGameMenu;
    internal static Transform optionsMenu;
    internal static Transform profilePanel;
    internal static Transform lobby;
    internal static Transform lobbyPanel;

    [SerializeField] internal TMP_InputField IPAdressInputField;
    [SerializeField] internal TMP_InputField PortInputField;

    [SerializeField] internal TMP_InputField NicknameInOptionsInput;
    [SerializeField] internal TextMeshProUGUI NicknameInProfilePanelText;

    internal static PILLoadHeader pILLoadHeader;

    private void Start()
    {
        mainMenu = this;
        startGameMenu = transform.parent.Find("StartGameMenu");
        joinGameMenu = transform.parent.Find("JoinGameMenu");
        optionsMenu = transform.parent.Find("OptionsMenu");
        profilePanel = transform.parent.Find("ProfilePanel");
        lobby = transform.parent.Find("Lobby");
        lobbyPanel = lobby.Find("LobbyPanel");
        pILLoadHeader = lobbyPanel.transform.Find("LoadHeader").GetComponent<PILLoadHeader>();
        PortInputField = joinGameMenu.Find("PortInput").GetComponent<TMP_InputField>();
        IPAdressInputField = joinGameMenu.Find("IPAdressInput").GetComponent<TMP_InputField>();
        NicknameInOptionsInput = optionsMenu.Find("NicknameInput").GetComponent<TMP_InputField>();
        NicknameInProfilePanelText = profilePanel.Find("PlayersNickName").GetComponent<TextMeshProUGUI>();
        SetIPAdressToJoin();
        SetPortToJoin();
    }

    public void SetIPAdressToJoin()
    {
        GlobalVariables.IpToConnect = IPAdressInputField.text;
    }

    public void SetPortToJoin()
    {
        GlobalVariables.PortToConnect = int.Parse(PortInputField.text);
    }

    public void SetPlayerNickname()
    {
        GlobalVariables.NickName = NicknameInProfilePanelText.text = NicknameInOptionsInput.text;
    }
}
