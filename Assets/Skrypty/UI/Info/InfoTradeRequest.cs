using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoTradeRequest : Info
{
    PlayerInGame requestingTradePIG;

    public PlayerInGame RequestingTradePIG { get => requestingTradePIG;  }

    internal void Initialize( PlayerInGame requestingTradePIG )
    {
        base.Initialize(requestingTradePIG.NickName + " wants to trade with you.");
        this.requestingTradePIG = requestingTradePIG;
    }

    internal void AcceptTradeRequest()
    {
        PlayerInGame.localPlayerInGame.AcceptTradeRequest(requestingTradePIG);
        this.Discard();
    }

    internal void DiscardTradeRequest()
    {
        PlayerInGame.localPlayerInGame.DeclineTradeRequest(requestingTradePIG);
        this.Discard();
    }
}
