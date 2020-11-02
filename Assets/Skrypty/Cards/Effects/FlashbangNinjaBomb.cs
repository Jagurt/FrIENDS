﻿#pragma warning disable CS0618 // Type too old lul

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

    /// <summary>
    /// Effect of this card forcefully stops a fight.
    /// All monsters and effects in use are discarded, and player can fight again.
    /// </summary>
    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        if (serverGameManager.fightInProggres)
            serverGameManager.EndFight(false);

        yield return null;
    }
}
