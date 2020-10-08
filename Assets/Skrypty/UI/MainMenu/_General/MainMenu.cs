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

    internal static CustomNetworkManager NetworkManager;

    [SerializeField] internal TMP_InputField IPAdressInputField;
    [SerializeField] internal TMP_InputField PortInputField;

    [SerializeField] internal TMP_InputField NicknameInOptionsInput;
    [SerializeField] internal TextMeshProUGUI NicknameInProfilePanelText;

    private void Start()
    {
        mainMenu = this;
        NetworkManager = FindObjectOfType<CustomNetworkManager>();
        startGameMenu = transform.parent.Find("StartGameMenu");
        hostGameMenu = transform.parent.Find("HostGameMenu");
        joinGameMenu = transform.parent.Find("JoinGameMenu");
        optionsMenu = transform.parent.Find("OptionsMenu");
        profilePanel = transform.parent.Find("ProfilePanel");
        lobby = transform.parent.Find("Lobby");
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
