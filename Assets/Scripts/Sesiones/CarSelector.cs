using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarSelector : MonoBehaviour
{
    public static CarSelector instance;

    [SerializeField] private List<CarModel> availableCars = new List<CarModel>();
    [SerializeField] private GameObject m_CarBtnPrefab;
    [SerializeField] private Transform m_BtnHolder;
    [SerializeField] private GameObject m_LobbiesPanel;
    [SerializeField] private GameObject m_KartSelectorPanel;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        foreach(var car in availableCars)
        {
            Debug.Log("Instanciando boton para el carro " + car.name);
            GameObject carObj = Instantiate(m_CarBtnPrefab, m_BtnHolder);

            carObj.GetComponentInChildren<Image>().sprite = car.carIcon;
            carObj.GetComponentInChildren<TextMeshProUGUI>().text = car.name;
            carObj.GetComponent<Button>().onClick.AddListener(() => Session.Instance.RegisterCarSelection(car.name));
            carObj.GetComponent<Button>().onClick.AddListener(() => m_LobbiesPanel.SetActive(true));
            carObj.GetComponent<Button>().onClick.AddListener(() => m_KartSelectorPanel.SetActive(false));
        }
    }

    public GameObject SearchKartModelByName(string _kart)
    {
        foreach(var car in availableCars)
        {
            if (car.name == _kart)
                return car.carVisual;
        }

        return null;
    }
}
