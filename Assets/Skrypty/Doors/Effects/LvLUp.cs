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
        PlayerInGame.localPlayerInGame.UseCard(this.netId);
    }

    [Server]
    internal override IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
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
        player.RpcDiscardCard(this.netId);
    }
}
