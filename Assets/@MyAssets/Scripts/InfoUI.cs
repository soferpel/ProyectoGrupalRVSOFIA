using UnityEngine;
using TMPro;

public class InfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private TextMeshProUGUI text;
    private OrderController orderController;

    public ClientManager clientManager;
    private MafiaController currentMafia;

    private void Start()
    {
        orderController = FindObjectOfType<OrderController>();
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            UpdateMafiaInfo();
            UpdateCashInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
        if(clientManager.mafia != null)
        {
            currentMafia = clientManager.mafia.GetComponent<MafiaController>();

            if (currentMafia != null)
            {
                descriptionText.text = currentMafia.AppearanceDescription;
                orderText.text = currentMafia.orderDescription;
            }
        }
        else
        {
            descriptionText.text = "No hay mafioso presente";
            orderText.text = "Sin pedido";
        }
    }

    private void UpdateCashInfo()
    {
        text.text =  orderController.cash.ToString();
    }
}
