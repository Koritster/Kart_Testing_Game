using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class SessionButton : MonoBehaviour
{
    [SerializeField] private Button m_SessionBtn;
    [SerializeField] private TextMeshProUGUI m_SessionName;
    [SerializeField] private TextMeshProUGUI m_SessionPlayers;
    [SerializeField] private GameObject m_SessionPasswordIcon;

    private void Awake()
    {
        m_SessionBtn = GetComponent<Button>();
    }

    public void SetSession(ISessionInfo m_sessionInfo)
    {
        m_SessionName.text = m_sessionInfo.Name;
        m_SessionPlayers.text = $"{m_sessionInfo.MaxPlayers - m_sessionInfo.AvailableSlots}/{m_sessionInfo.MaxPlayers}";
        m_SessionPasswordIcon.SetActive(m_sessionInfo.HasPassword);

        //Agregar funcionalidad para entrar a la sala
        m_SessionBtn.onClick.AddListener(() => Session.Instance.JoinSessionWithId(m_sessionInfo.Id));
    }
}
