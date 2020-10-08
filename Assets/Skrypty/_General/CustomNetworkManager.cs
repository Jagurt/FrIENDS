#pragma warning disable CS0618 // Type too old lul

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class CustomNetworkManager : NetworkManager
{
    private NetworkClient myClient;

    [SerializeField] internal bool isServerBusy = false;
    internal static CustomNetworkManager customNetworkManager;

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
        Debug.Log(networkAddress);
        Debug.Log(networkPort);
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
    }
}
