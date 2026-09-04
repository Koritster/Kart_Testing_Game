using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetcodeLobby : NetworkBehaviour
{
    public static NetcodeLobby instance;

    //Datos de jugador
    public struct PlayerNetworkData : INetworkSerializable, IEquatable<PlayerNetworkData>
    {
        public ulong clientId;
        public FixedString32Bytes playerName;
        public FixedString32Bytes playerKart;
        public int spawnIndex;

        public PlayerNetworkData(FixedString32Bytes _playerName, FixedString32Bytes _playerKart, int _playerIndex, ulong _clientId)
        {
            this.clientId = _clientId;
            this.playerName = _playerName;
            this.playerKart = _playerKart;
            this.spawnIndex = _playerIndex;
        }

        public bool Equals(PlayerNetworkData other) => clientId == other.clientId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerKart);
            serializer.SerializeValue(ref spawnIndex);
        }
    }

    //Lista de jugadores
    public NetworkList<PlayerNetworkData> players = new NetworkList<PlayerNetworkData>(default, NetworkVariableBase.DefaultReadPerm, NetworkVariableWritePermission.Owner);

    public NetworkVariable<bool> GameStarted =
        new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    [Header("References")]
    public List<Transform> spawnPositions;

    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private GameObject _PlayersPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }

    private void SetSpawnpoints()
    {
        GameObject[] spawnpointsGO = GameObject.FindGameObjectsWithTag("Spawnpoint");

        foreach(GameObject spawn in spawnpointsGO)
        {
            spawnPositions.Add(spawn.transform);
        }
    }

    //Registrar jugador al servidor, se llama cada que un jugador entra a la sesión
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddPlayerServerRpc(
    FixedString32Bytes name,
    FixedString32Bytes kart,
    RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        int spawnIndex = players.Count;

        PlayerNetworkData playerData = new PlayerNetworkData(name, kart, spawnIndex, clientId);
        players.Add(playerData);

        if (SceneManager.GetActiveScene().name == "MapaCambio")
        {
            InstantiatePlayer(playerData);
        }
    }

    private void OnSceneLoaded(
    string sceneName,
    LoadSceneMode loadSceneMode,
    List<ulong> clientsCompleted,
    List<ulong> clientsTimedOut)
    {
        Debug.Log($"ESCENA CARGADA: {sceneName}");

        foreach (PlayerNetworkData playerData in players)
        {
            InstantiatePlayer(playerData);
        }
    }

    private void InstantiatePlayer(PlayerNetworkData clientData)
    {
        SetSpawnpoints();
        Transform spawn = spawnPositions[clientData.spawnIndex];

        NetworkObject player = Instantiate(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );
        
        //Spawnear objeto network manualmente
        player.SpawnAsPlayerObject(clientData.clientId);

        Debug.Log($"[SERVER] Spawn player for {clientData.clientId}");

        NewKartController carController = player.GetComponent<NewKartController>();

        carController.Teleport(spawn);

        carController.playerName.Value = clientData.playerName;
        carController.carModel.Value = clientData.playerKart;

        carController.transform.forward = spawn.forward;
    }

    public void StartGame()
    {
        StartGameRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void StartGameRpc()   
    {
        GameStarted.Value = true;

        /*foreach (var playerData in players)
        {
            if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                playerData.clientId,
                out var client))
                continue;

            CarController playerObj = client.PlayerObject.GetComponent<CarController>();
            Transform spawn = spawnPositions[playerData.spawnIndex];

            playerObj.Teleport(spawn);
        }*/

        StartGameClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Owner)]
    private void StartGameClientRpc()
    {
        _PlayersPanel.SetActive(false);
        Debug.Log("Starting game");

        NewKartController playerObj = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NewKartController>();
        //playerObj.SetHitteables();
        Transform spawn = spawnPositions[(int) NetworkManager.Singleton.LocalClientId];

        playerObj.Teleport(spawn);

        NewKart[] karts = FindObjectsByType<NewKart>(FindObjectsSortMode.None);

        foreach(NewKart kart in karts)
        {
            PositionsManager.instance.RegisterKart(kart);
        }


        /*foreach(var a in FindObjectsByType<MoveToWaypoints>(FindObjectsSortMode.None))
        {
            a.ActivateMovement();
        }*/

        if (!IsServer) return;

        PositionsManager.instance.started.Value = true;
        PositionsManager.instance.CalculatePositionsServerRpc();
    }
}
