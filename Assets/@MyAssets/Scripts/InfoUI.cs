using UnityEngine;
using TMPro;

public class InfoUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject infoPanel; // Pantalla con texto
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;

    [Header("Dependencies")]
    private MafiaController currentMafia;

    void Update()
    {
        // Solo actualizamos el contenido si el panel está activo
        if (infoPanel != null && infoPanel.activeSelf)
        {
            UpdateMafiaInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
        // Encuentra al mafioso activo en pantalla
        currentMafia = FindObjectOfType<MafiaController>();

        if (currentMafia != null)
        {
            // Actualiza los textos con la información del mafioso
            descriptionText.text = currentMafia.AppearanceDescription;
            orderText.text = currentMafia.orderDescription;
        }
        else
        {
            // Si no hay mafioso, muestra textos predeterminados
            descriptionText.text = "No hay mafioso presente";
            orderText.text = "Sin pedido";
        }
    }
}
