using UnityEngine;
using TMPro;

public class InfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;

    private MafiaController currentMafia;

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            UpdateMafiaInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
        currentMafia = FindObjectOfType<MafiaController>();

        if (currentMafia != null)
        {
            descriptionText.text = currentMafia.AppearanceDescription;
            orderText.text = currentMafia.orderDescription;
        }
        else
        {
            descriptionText.text = "No hay mafioso presente";
            orderText.text = "Sin pedido";
        }
    }
}
