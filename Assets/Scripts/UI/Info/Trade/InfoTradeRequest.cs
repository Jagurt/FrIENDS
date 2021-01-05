using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Class fo making specific, Trade Request Info objects. </summary>
public class InfoTradeRequest : Info
{
    PlayerInGame requestingTradePIG;
    public PlayerInGame RequestingTradePIG { get => requestingTradePIG;  }

    internal void Initialize( PlayerInGame requestingTradePIG )
    {
        base.Initialize(requestingTradePIG.nickName + " wants to trade with you.");
        this.requestingTradePIG = requestingTradePIG;
    }

    /// <summary>
    /// Called when accept button is pressed.
    /// </summary>
    internal void AcceptTradeRequest()
    {
        PlayerInGame.localPlayerInGame.AcceptTradeRequest(requestingTradePIG);
        this.Discard();
    }
    /// <summary>
    /// Called when decline button is pressed.
    /// </summary>
    internal void DeclineTradeRequest()
    {
        PlayerInGame.localPlayerInGame.DeclineTradeRequest(requestingTradePIG);
        this.Discard();
    }
}
