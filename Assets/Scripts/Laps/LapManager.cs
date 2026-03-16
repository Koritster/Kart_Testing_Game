using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LapManager : MonoBehaviour
{
    /*Códiogo para crear estructuras en multiplayer*/
    /*public struct PlayerNetworkData : INetworkSerializable, System.IEquatable<PlayerNetworkData>
    {
        //Variables
        public FixedString32Bytes playerName;
        public int score;

        //Constructor
        public PlayerNetworkData(FixedString32Bytes playerName, int score)
        {
            this.playerName = playerName;
            this.score = score;
        }

        //Esta madre permite leer los valores
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref score);
        }

        public bool Equals(PlayerNetworkData other)
        {
            return playerName.Equals(other.playerName) && score == other.score;
        }
    }

    public NetworkList<PlayerNetworkData> playerList;*/

    public int totalLaps = 3;
    public int totalCheckpoints;

    public int currentLap = 0;

    int nextCheckpointIndex = 0;
    bool canFinishLap = false;

    void Start()
    {
        currentLap = 1;
    }

    public void PassCheckpoint(int checkpointIndex)
    {
        if (checkpointIndex == nextCheckpointIndex)
        {
            nextCheckpointIndex++;

            if (nextCheckpointIndex >= totalCheckpoints)
            {
                canFinishLap = true;
            }
        }
    }

    public void TryCompleteLap()
    {
        if (!canFinishLap) return;

        currentLap++;
        nextCheckpointIndex = 0;
        canFinishLap = false;

        if (currentLap > totalLaps)
        {
            FinishRace();
        }
    }

    void FinishRace()
    {
        Debug.Log("Carrera terminada");
    }
}