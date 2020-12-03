#pragma warning disable CS0618 // Type is too old

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

/// <summary>
/// CustomNetworkManager deriving from built-in NetworkManager.
/// For customizing network behaviour.
/// </summary>
public class CustomNetManager : NetworkManager
{
    /// <summary>
    /// Declaration of class deriving from built-in interface "MessageBase". Essential when sending network messages to connected players (servers clients)
    /// It is empty becouse I don't actaully need to send any variable IN the message but I use it to call functions on clients
    /// </summary>
    public class EmptyMessage : MessageBase { }
    // Static reference to this script.
    internal static CustomNetManager singleton;
    /// <summary>
    /// NetworkClient is built in class. It contains useful variables such as:
    /// When myClient is host (serwer) - server port, handlers for server messages
    /// When myClient is player (client) - server ip, server port, handlers for client messages
    /// </summary>
    private NetworkClient myClient;

    MessageBase msgEmpty = new EmptyMessage();
    // AddPlayerMsg, ReconnectMsg - registering ids for network messages, MsgType.Highest - the highest id value of built-in messages.
    const short AddPlayerMsg = MsgType.Highest + 1;
    const short ReconnectMsg = MsgType.Highest + 2;

    internal static bool gameLoaded = false;
    internal static bool playerCreated = false;

    // Variable that is set in Lobby after pressing Start Game Button, and is used later to create players list in GameScene after all players have created their objects.
    internal static int playersToConnect;
    /// <summary>
    /// List of connections of CustomNetworkConnections, contains their ip adresses and references to objects belonging to them, essential for reconnecting.
    /// Unity does have built-in NetworkConnection class but when a client disconnects its ip adress disappears. I need to keep ip adress to check if player is reconnecting.
    /// </summary>
    internal static List<CustomNetworkConnection> playingConnections = new List<CustomNetworkConnection>();
    /// <summary>
    /// [ClientRpc] - decorator for Rpc methods. These methods are called exclusively BY server and are executed ONLY ON clients (in our case host is server AND client).
    /// Only 1 Rpc method can be called per frame. Engine doesn't warn you if more of them are called in a single frame and becouse of this I had problems for over a month.
    /// 
    /// [SyncVar]   - decorator for variables that have to be synchronized on all clients by server.
    /// Those variables are automatically updated for clients when any change is made to them on server.
    /// Sometimes making changes to [SyncVar] variables also can occur once per frame, so they too use following mechanic.
    /// 
    /// To bypass "1 Rpc method per frame limitation" I made "isServerBusy" bool variable and each Rpc method is called from inside a Coroutine method.
    /// Those Coroutine methods function as follows:
    ///     1. check if isServerBusy is true 
    ///     2. wait for it to be false 
    ///     3. set isServerBusy to true and call Rpc method
    ///     4. wait an additional frame to prevent accidental second Rpc call
    ///     5. set isServerBusy to false
    /// [SerializeField] is a decorator that forces unity to show variable in inspector
    /// </summary>
    [SerializeField] internal bool isServerBusy = false;
    // Start is built-in called once object is enabled for the first time.
    private void Start()
    {
        singleton = this;
    }
    /// <summary>
    /// Start hosting is called from a Start Host Button in Main Menu.
    /// It creates match, server and starts listening for connections using built-in Unity features.
    /// </summary>
    public void StartHosting()
    {
        StartMatchMaker();
        matchMaker.CreateMatch("Seba's Match", 10, true, "", "", "", 0, 0, OnMatchCreated);
    }
    private void OnMatchCreated( bool success, string extendedInfo, MatchInfo responseData )
    {
        GlobalVariables.IsHost = true;
        myClient = StartHost();
    }
    /// <summary>
    /// Not used. These methods were supposed to be attached to match object, that would be created when getting match info from matchmaking server. 
    /// As far I know Unity doesn't provide this service anymore. I would have to make such server myself.
    /// </summary>
    /// <param name="match"></param>
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
        // StartClient() automatically sets myClients variables for client and connects with set ip on set port.
        myClient = StartClient();
    }
    /// <summary>
    /// Event called when StartClient() is called. Used to register handler for network message on client side. 
    /// </summary>
    /// <param name="client"></param>
    public override void OnStartClient( NetworkClient client )
    {
        client.RegisterHandler(AddPlayerMsg, OnClientAddPlayerMsg);
        base.OnStartClient(client);
    }
    /// <summary>
    /// Called when leaving game as client, stopping server as host.
    /// playingConnections.Clear() - resets list of connected players.
    /// ...Shutdown() - methods disabling networking components. It resets their variables. They are automatically turned on again by engine when needed.
    /// </summary>
    public void Disconnect()
    {
        if (GlobalVariables.IsHost) StopHost();
        else StopClient();

        playerCreated = false;
        playingConnections.Clear();
        myClient.Shutdown();
        NetworkServer.Shutdown();
        NetworkTransport.Shutdown();
    }
    /// <summary>
    /// Method to be called on reconnecting player.
    /// When player wants to reconnects it is in TitleScene, however game is in GameScene.
    /// It creates issue with scene and objects synchronization. 
    /// Unity automatically synchronizes those, but it has problems when creating new PlayerObject at the same time and it causes errors.
    /// Before it synchronizes existing objects they are automatically "Disabled" and after synchronization they become enabled automatically.
    /// When they are enabled for first time Start() is called, so I used 1 of them, ServerGameManager, to call method below.
    /// </summary>
    internal void AddPlayerOnReconnect()
    {
        if (playerCreated)
            return;
        playerCreated = true;
        ClientScene.AddPlayer(0);
    }
    /// <summary>
    /// Event triggered on server when player connects.
    /// </summary>
    /// <param name="conn">NetworkConnection of connecting client</param>
    public override void OnServerConnect( NetworkConnection conn )
    {
        if (!playingConnections.Exists(x => x.address.Equals(conn.address)))                // If player isn't in playing connections, it is new player and has to be added there.
        {
            if (SceneManager.GetActiveScene().name.Equals("GameScene"))                     // If new player tries to enter existing game
            {
                Debug.Log("Unexpected player tries to connect!");                           // Disconnect player, 
                conn.Disconnect();                                                          // TODO: Notify about connecting to existing game
                return;
            }

            CustomNetworkConnection newConn = new CustomNetworkConnection(conn.address);    // Creating custom copy of connection, becouse original one losses ip adress when player disconnects

            playingConnections.Add(newConn);                                                // Adding connection to list of connections, this is removed in PlayerManager in case of leaving game while in lobby
            base.OnServerConnect(conn);

            NetworkServer.SendToClient(conn.connectionId, AddPlayerMsg, msgEmpty);          // Send message to connected player that he may has to call OnClientAddPlayerMsg method;
        }
        // If player has existing playingConnection it entered game that is already playing (in GameScene),
        // it is reconnecting and should have his objects reassigned
        else
        {
            Debug.Log("Reconnected to server! conn.address - " + conn.address);
            // Code for printing network errors in network events
            if (conn.lastError != NetworkError.Ok)
            {
                if (LogFilter.logError)
                {
                    Debug.Log("What error is dis: " + conn.lastError);
                }
            }
        }
    }
    /// <summary>
    /// Called on server when player disconnects.
    /// </summary>
    /// <param name="conn">NetworkConnection object for disconnecting player.</param>
    public override void OnServerDisconnect( NetworkConnection conn )
    {
        // If in TitleScene, behave as usual.
        if (SceneManager.GetActiveScene().name.Equals("TitleScene"))
        {
            NetworkServer.DestroyPlayersForConnection(conn);
        }
        else
        {
            // playerControllers[0].gameObject - PlayerManager object which is this connections main PlayerObject
            // GetComponent<PlayerManager>() - script located on PlayerManagers object
            // Since in this event, conn.address doesn't exist for some reason, I need to store it somewhere
            string address = conn.playerControllers[0].gameObject.GetComponent<PlayerManager>().address;
            Debug.Log("OnServerDisconnect: conn.address - " + address);

            // Finding CustomNetworkConnection with disconnecting players address
            var playingConnection = playingConnections.Find(x => x.address.Equals(address));

            // Finding players main object(PlayerManager).
            GameObject GO = ClientScene.FindLocalObject(playingConnection.clientOwnedObjects[0]);

            // Players main object has to be destroyed becouse Unity prevents it from being reassigned.
            NetworkServer.Destroy(GO);

            // Setting players destroyed main objects netId (network reference) to invalid, becouse said object is destroyed.
            // I need to set it to NetworkInstanceId.Invalid, becouse it can't be set to null.
            playingConnection.clientOwnedObjects[0] = NetworkInstanceId.Invalid;

            // This is PlayerInGame object, the second object for player that handles player functionality when in GameScene
            GO = ClientScene.FindLocalObject(playingConnection.clientOwnedObjects[1]);

            // We need to remove authority to assign it later, when player reconnects. Essential for correct objects behaviour.
            GO.GetComponent<NetworkIdentity>().RemoveClientAuthority(conn);
            // Manually clearing clientOwnedObjects for connections to PlayerInGame object from being destroyed
            conn.clientOwnedObjects.Clear();
            // Clearing disconnected connection
            NetworkServer.DestroyPlayersForConnection(conn);

            // TODO : check which player got disconnected?  
            // Wait for players to reconnect, allow others to disconnect in meantime
        }
        // Code for printing network errors in network events
        if (conn.lastError != NetworkError.Ok)
        {
            if (LogFilter.logError)
            {
                Debug.Log("ServerDisconnected due to error: " + conn.lastError);
            }
        }
    }
    /// <summary>
    /// Message that server sends to client when client connects to lobby before the game starts.
    /// </summary>
    /// <param name="networkMessage"></param>
    void OnClientAddPlayerMsg( NetworkMessage networkMessage )
    {
        playerCreated = true;
        ClientScene.AddPlayer(myClient.connection, 0);
    }
    /// <summary>
    /// Event called on client when connecting to server.
    /// It usually sets client to ready and adds clients main object.
    /// I needed to remove all code from it becouse it caused errors when player was reconneting.
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientConnect( NetworkConnection conn ) { }
    /// <summary>
    /// Event called when client ends loading servers scene.
    /// Readiness is then set to false so I need to set it to true.
    /// If client is not ready it doesn't enable synchronized objects.
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientSceneChanged( NetworkConnection conn )
    {
        ClientScene.Ready(conn);
    }
    /// <summary>
    /// Event triggered on client when it disconnects.
    /// </summary>
    /// <param name="conn"></param>
    public override void OnClientDisconnect( NetworkConnection conn )
    {
        Disconnect();
    }
}
