using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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

    //Registrar jugador al servidor
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddPlayerServerRpc(
    FixedString32Bytes name,
    FixedString32Bytes kart,
    RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        int spawnIndex = players.Count;

        /*players.Add(new PlayerNetworkData
        {
            clientId = clientId,
            playerName = name,
            spawnIndex = spawnIndex,
            playerKart = kart
        });*/

        players.Add(new PlayerNetworkData(name, kart, spawnIndex, clientId));

        Transform spawn = spawnPositions[spawnIndex];

        NetworkObject player = Instantiate(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );

        //Spawnear objeto network manualmente
        player.SpawnAsPlayerObject(clientId);

        Debug.Log($"[SERVER] Spawn player for {clientId}");

        Debug.Log(player.GetComponent<PlayerNetworkKart>());

        PlayerNetworkKart carController = player.GetComponent<PlayerNetworkKart>();
        
        carController.playerName.Value = name;

        carController.carModel.Value = kart;
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

        PlayerNetworkKart playerObj = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetworkKart>();
        playerObj.SetHitteables();
        Transform spawn = spawnPositions[(int) NetworkManager.Singleton.LocalClientId];

        playerObj.Teleport(spawn);

        foreach(var a in FindObjectsByType<MoveToWaypoints>(FindObjectsSortMode.None))
        {
            a.ActivateMovement();
        }

        PositionsManager.instance.started = true;
    }
}
