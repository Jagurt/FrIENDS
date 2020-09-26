using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Gaerie : MonsterCard
{
    void Start()
    {
        serverGameManager = ServerGameManager.serverGameManager;
        Initialize();
    }

    [Server]
    override internal IEnumerator EffectOnUse( NetworkInstanceId targetNetId )
    {
        return base.EffectOnUse(targetNetId);
    }
}
