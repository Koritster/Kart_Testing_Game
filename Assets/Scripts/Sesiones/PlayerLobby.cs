using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;

public class PlayerLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_PlayerName;

    private void Awake()
    {
    }

    public void SetSession(IReadOnlyPlayer m_player)
    {
        m_PlayerName.text = m_player.Id;

        Debug.LogWarning("Setteando nombre");

        if(!m_player.Properties.TryGetValue("PlayerName", out PlayerProperty m_playerNameProperty))
        {
            Debug.Log("No tiene propiedades");

            return;
        }

        m_PlayerName.text = m_playerNameProperty.Value;
    }
}
