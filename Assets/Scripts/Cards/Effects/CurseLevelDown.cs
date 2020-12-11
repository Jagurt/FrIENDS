#pragma warning disable CS0618 // Type too old lul

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CurseLevelDown : Effect
{
    private void Start()
    {
        gameManager = GameManager.singleton;
        target = Target.Player;
        choosable = true;
        Initialize();
    }

    /// <summary> Applying curse effect ( -1 level ) to tergeted or default player. </summary>
    [Server]
    internal override IEnumerator EffectOnUse( )
    {
        yield return base.EffectOnUse();

        if (CustomNetManager.singleton.isServerBusy)
            yield return new WaitUntil(() => !CustomNetManager.singleton.isServerBusy);
        CustomNetManager.singleton.isServerBusy = true;

        PlayerInGame player = null;

        if (targetNetId != NetworkInstanceId.Invalid) // If player to curse is not chosen
            player = ClientScene.FindLocalObject(targetNetId).GetComponent<PlayerInGame>(); 

        if (!player)
        {
            // Player which currently has turn is chosen, this should only ocurr when curse is drawn as turns first door.
            if (gameManager.activePlayerIndex >= 0)
                player = gameManager.playersObjects[gameManager.activePlayerIndex].GetComponent<PlayerInGame>(); 
            else
                player = gameManager.playersObjects[0].GetComponent<PlayerInGame>();
        }


        player.Level -= 1;
        yield return new WaitForEndOfFrame();

        PlayerInGame.localPlayerInGame.RpcDiscardCard(this.netId);
        yield return new WaitForEndOfFrame();

        StartCoroutine(gameManager.ServerTurnOwnerReadiness());

        yield return new WaitForEndOfFrame();
        CustomNetManager.singleton.isServerBusy = false;
    }
}
