using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FlashbangNinjaBomb : Effect
{
    internal override void UseCard()
    {
        PlayerInGame.localPlayerInGame.UseCardOnLocalPlayer(this.netId);
    }

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        //throw new NotImplementedException();

        if (serverGameManager.fightInProggres)
            serverGameManager.EndFight(false);

        yield return null;
    }
}
