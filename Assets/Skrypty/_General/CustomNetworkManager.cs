#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    public class EmptyMessage : MessageBase
    {
        // empty?
    }

    internal static CustomNetworkManager customNetworkManager;

    private NetworkClient myClient;

    MessageBase msgEmpty = new EmptyMessage();
    const short AddPlayerMsg = MsgType.Highest + 1;
    const short ReconnectMsg = MsgType.Highest + 2;

    internal static bool gameLoaded = false;
    internal static int playersToConnect;

    [SerializeField] internal static List<CustomNetworkConnection> playingConnections = new List<CustomNetworkConnection>();

    [SerializeField] internal bool isServerBusy = false;

    private void Start()
    {
        customNetworkManager = this;
    }

    public void StartHosting()
    {
        StartMatchMaker();
        matchMaker.CreateMatch("Seba's Match", 4, true, "", "", "", 0, 0, OnMatchCreated);
    }

    private void OnMatchCreated( bool success, string extendedInfo, MatchInfo responseData )
    {
        GlobalVariables.IsHost = true;
        myClient = StartHost();
    }

    private void HandleMatchesListComplete( bool succes,
        string extendedInfo,
        List<MatchInfoSnapshot> responseData )
    {
        AvailableMatchesList.HandleNewMatchList(responseData);
    }

    public void JoinMatch( MatchInfoSnapshot match )
    {
        if (matchMaker = null)
            StartMatchMaker();

        matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, HandleJoinedMatch);
    }

    private void HandleJoinedMatch( bool success, string extendedInfo, MatchInfo responseData )
    {
        StartClient(responseData);
    }

    public void JoinMatchViaIP()
    {
        networkAddress = GlobalVariables.IpToConnect;
        networkPort = GlobalVariables.PortToConnect;
        GlobalVariables.IsHost = false;
        myClient = StartClient();
    }

    public override void OnStartClient( NetworkClient client )
    {
        client.RegisterHandler(AddPlayerMsg, OnClientAddPlayerMsg);
        client.RegisterHandler(ReconnectMsg, OnClientAddPlayerMsg);
        base.OnStartClient(client);
    }

    public void RefreshMatches()
    {
        if (matchMaker == null)
            StartMatchMaker();

        matchMaker.ListMatches(0, 10, "", true, 0, 0, HandleMatchesListComplete);
    }

    public void Disconnect()
    {
        if (GlobalVariables.IsHost) StopHost();
        else StopClient();

        playingConnections.Clear();
        myClient.Shutdown();
        NetworkServer.Shutdown();
        NetworkTransport.Shutdown();
    }

    public override void OnServerConnect( NetworkConnection conn )
    {
        if (!playingConnections.Exists(x => x.address.Equals(conn.address)))                // If player isn't in playing connections, it is new player and has to be added there.
        {
            if (SceneManager.GetActiveScene().name.Equals("GameScene"))                     // if new player tries to enter existing game
            {
                Debug.Log("Unexpected player tries to connect!");
                return;
            }

            CustomNetworkConnection newConn = new CustomNetworkConnection(conn.address);    // Creating custom copy of connection, becouse original one gets messed up when player disconnects

            playingConnections.Add(newConn);                                                // Adding connection to list of connections, this is removed in PlayerManager in case of leaving game in lobby
            base.OnServerConnect(conn);

            NetworkServer.SendToClient(conn.connectionId, AddPlayerMsg, msgEmpty);          // Send message to connected player that he may call ClientScene.AddPlayer(0);
        }
        else                                                                                // If player has existing playingConnection it entered game from lobby, it is reconnecting and should have his objects reassigned
        {
            Debug.Log("Reconnected to server! conn.address - " + conn.address);
            // In GameScene: Search for existing player connection to apply reconnection

            if (SceneManager.GetActiveScene().name.Equals("GameScene"))                     // This should only happen in "GameScene", this if is probably unnecesary
            {
                NetworkServer.SetClientNotReady(conn);
                NetworkServer.SendToClient(conn.connectionId, AddPlayerMsg, msgEmpty);      // Send message to connected player that he may call ClientScene.AddPlayer(0);
            }

            if (conn.lastError != NetworkError.Ok)
            {
                if (LogFilter.logError)
                {
                    Debug.Log("What error is dis: " + conn.lastError);
                }
            }
        }
    }
    public override void OnServerDisconnect( NetworkConnection conn )
    {
        if (SceneManager.GetActiveScene().name.Equals("TitleScene"))
        {
            NetworkServer.DestroyPlayersForConnection(conn);
        }
        else
        {
            string adress = conn.playerControllers[0].gameObject.GetComponent<PlayerManager>().address;
            Debug.Log("OnServerDisconnect: conn.address - " + adress);
            //conn.playerControllers[0].gameObject.GetComponent<PlayerManager>().connectionToClient;

            var playingConnection = playingConnections.Find(x => x.address.Equals(adress));   // Finding custom connection with right adress
            GameObject GO = ClientScene.FindLocalObject(playingConnection.clientOwnedObjects[0]);   // Finding Player Object
            NetworkServer.Destroy(GO);                                                              // Manually destroying player Object
            playingConnection.clientOwnedObjects[0] = NetworkInstanceId.Invalid;                    // Setting destroyed player Objects netId to invalid
            GO = ClientScene.FindLocalObject(playingConnection.clientOwnedObjects[1]);              // This is PlayerInGame object
            GO.GetComponent<NetworkIdentity>().RemoveClientAuthority(conn);                         // We need to remove authority to assign it later

            conn.clientOwnedObjects.Clear();                                // Manually clearing clientOwnedObjects for connections to prevent them from being destroyed
            NetworkServer.DestroyPlayersForConnection(conn);                // Clearing disconnected connection

            // TODO : check which player got disconnected?  conn.clientOwnedObjects
            // Block all functionality until players reconnects
            // Wait for players to reconnect, allow to disconnect in meantime
        }

        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError)
            {
                Debug.Log("ServerDisconnected due to error: " + conn.lastError);
            }
        }
    }

    void OnClientAddPlayerMsg( NetworkMessage networkMessage )
    {
        ClientScene.AddPlayer(myClient.connection, 0);
    }
    void OnClientReconnect( NetworkMessage networkMessage )
    {
        ClientScene.Ready(myClient.connection);
    }

    public override void OnClientConnect( NetworkConnection conn )
    {
        //if (this.clientLoadedScene)
        //    return;
        //ClientScene.Ready(conn);
    }
    public override void OnClientNotReady( NetworkConnection conn )
    {
        base.OnClientNotReady(conn);
        //ClientScene.Ready(conn);
    }
    public override void OnClientSceneChanged( NetworkConnection conn ) { ClientScene.Ready(conn); }
}
