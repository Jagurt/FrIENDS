using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

class JoinButtonOnGamePanel : MonoBehaviour
{
    private Text joinButtonText;
    private MatchInfoSnapshot match;

    private void Awake()
    {
        joinButtonText = GetComponentInChildren<Text>();
        GetComponent<Button>().onClick.AddListener(JoinMatch);
    }

    public void Initialize(MatchInfoSnapshot match, Transform panelTransform)
    {
        this.match = match;
        joinButtonText.text = match.name;
        transform.SetParent(panelTransform);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
    }

    private void JoinMatch()
    {
        FindObjectOfType<CustomNetManager>().JoinMatch(match);
    }
}
