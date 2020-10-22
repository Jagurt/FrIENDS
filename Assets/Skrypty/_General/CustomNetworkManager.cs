#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    private NetworkClient myClient;
    internal static CustomNetworkManager customNetworkManager;

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
        //Debug.Log(networkAddress);
        //Debug.Log(networkPort);
        GlobalVariables.IsHost = false;
        myClient = StartClient();
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

        NetworkServer.Shutdown();
    }

    public override void OnServerAddPlayer( NetworkConnection conn, short playerControllerId )
    {
        if (SceneManager.GetActiveScene().name.Equals("TitleScene"))
        {
            base.OnServerAddPlayer(conn, playerControllerId);
        }
        else
        {
            base.OnServerConnect(conn);
        }
    }

    public override void OnServerConnect( NetworkConnection conn )
    {
        if (!playingConnections.Exists(x => x.address.Equals(conn.address)))                // If player isn't in active connections, it is new player and has to be added there.
        {
            if (SceneManager.GetActiveScene().name.Equals("GameScene"))                     // if new player tries to enter existing game
            {
                Debug.Log("Unexpected player tries to connect!");
                return;
            }

            //Debug.Log("New Player connects! conn.address - " + conn.address);
            CustomNetworkConnection newConn = new CustomNetworkConnection(conn.address);    // Creating custom copy of connection, becouse original one gets messed up when player disconnects

            playingConnections.Add(newConn);                                                // Adding connection to list of connections, this is removed in PlayerManager in case of leaving game in lobby
            base.OnServerConnect(conn);
        }
        else                                                                                // If player has existing playingConnection it entered game from lobby, it is reconnecting and should have his objects reassigned
        {
            Debug.Log("Reconnected to server! conn.address - " + conn.address);
            // In GameScene: Search for existing player connection to apply reconnection

            if (SceneManager.GetActiveScene().name.Equals("GameScene"))                     // This should only happen in "GameScene", this if is probably unnecesary
            {
                CustomNetworkConnection comparedConn = playingConnections.Find(x => x.address.Equals(conn.address)); // Finding existing CustomConnection for reconnected player

                foreach (var netId in comparedConn.clientOwnedObjects)
                {
                    GameObject gameObject = ClientScene.FindLocalObject(netId);
                    bool authorityChangeSucces = gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(conn);
                }
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
}
