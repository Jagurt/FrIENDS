using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

class MatchListPanel : MonoBehaviour
{
    [SerializeField] private JoinButtonOnGamePanel buttonPrefab;

    private void Awake()
    {
        AvailableMatchesList.OnAvailableMatchesChanged += AvailableMatchesList_OnAvailableMatchesChanged;
    }

    private void AvailableMatchesList_OnAvailableMatchesChanged( List<MatchInfoSnapshot> matches )
    {
        ClearExistingButtons();
        CreateNewJoinGameButtons(matches);
    }

    private void ClearExistingButtons()
    {
        var buttons = GetComponentsInChildren<JoinButtonOnGamePanel>();
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }
    }

    private void CreateNewJoinGameButtons( List<MatchInfoSnapshot> matches )
    {
        foreach (var match in matches)
        {
            var button = Instantiate(buttonPrefab);
            button.Initialize(match, transform);
        }
    }
}

