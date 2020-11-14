#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LvLUp : Effect
{
    private void Start()
    {
        target = Target.Player;
        serverGameManager = ServerGameManager.serverGameManager;
        Initialize();
    }

    internal override void UseCard()
    {
        PlayerInGame.localPlayerInGame.UseCardOnLocalPlayer(this.netId);
    }

    /// <summary>
    /// Increases level of player who uses this card.
    /// </summary>
    [Server]
    internal override IEnumerator EffectOnUse( )
    {
        yield return new WaitForSecondsRealtime(1f);

        if (CustomNetworkManager.customNetworkManager.isServerBusy)
            yield return new WaitUntil(() => !CustomNetworkManager.customNetworkManager.isServerBusy);
        CustomNetworkManager.customNetworkManager.isServerBusy = true;

        PlayerInGame player;
        player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>();

        if (player != null)
            player.Level += 1;
        else
            Debug.LogError("Player who played card is not found!");

        //StartCoroutine( PlayerInGame.localPlayerInGame.ServerDiscardCard(this.netId));

        yield return new WaitForEndOfFrame();

        player.RpcDiscardCard(this.netId);

        yield return new WaitForEndOfFrame();
        CustomNetworkManager.customNetworkManager.isServerBusy = false;
    }
}
