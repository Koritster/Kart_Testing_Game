using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetcodeLobby : NetworkBehaviour
{
    public static NetcodeLobby instance;

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

    public NetworkList<PlayerNetworkData> players = new NetworkList<PlayerNetworkData>(default, NetworkVariableBase.DefaultReadPerm, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> GameStarted =
        new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

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
    }

    public override void OnNetworkSpawn()
    {
        
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddPlayerServerRpc(
    FixedString32Bytes name,
    FixedString32Bytes kart,
    RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        int spawnIndex = players.Count;

        players.Add(new PlayerNetworkData
        {
            clientId = clientId,
            playerName = name,
            spawnIndex = spawnIndex
        });

        Transform spawn = spawnPositions[spawnIndex];

        NetworkObject player = Instantiate(
            playerPrefab,
            spawn.position,
            spawn.rotation
        );

        player.SpawnAsPlayerObject(clientId);

        Debug.Log($"[SERVER] Spawn player for {clientId}");

        CarController car = player.GetComponent<CarController>();
        
        car.playerName.Value = name;
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

        CarController playerObj = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CarController>();
        playerObj.SetHitteables();
        Transform spawn = spawnPositions[(int) NetworkManager.Singleton.LocalClientId];

        playerObj.Teleport(spawn);

        foreach(var a in FindObjectsByType<MoveToWaypoints>(FindObjectsSortMode.None))
        {
            a.ActivateMovement();
        }
    }
}
