#pragma warning disable CS0618 // Typ lub składowa jest przestarzała

using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary> 
/// Class for custom NetworkConnection objects becouse original Unity class 
/// loses address value when player disconnects. 
/// </summary>
public class CustomNetworkConnection 
{
    public string address;
    public List<NetworkInstanceId> clientOwnedObjects;

    public CustomNetworkConnection( string address )
    {
        this.address = address;
        clientOwnedObjects = new List<NetworkInstanceId>();
    }
}
