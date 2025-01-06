using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InfoUI : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel; 
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI orderText;
    [SerializeField] private TextMeshProUGUI textCash;
    [SerializeField] private TextMeshProUGUI textWeapon;
    [SerializeField] private Button repairButton;
    [SerializeField] private TextMeshProUGUI repairMessage;
    [SerializeField] private Slider repairSlider;
    [SerializeField] private Image repairSliderFillImage;
    [SerializeField] private Button boxButton;
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private BoxButtom boxBuyController;

    public OrderController orderController;
    public ClientManager clientManager;
    private MafiaController currentMafia;
    public WeaponController weaponController;
    private Coroutine messageCoroutine;

    private void Start()
    {
        if (textBox != null)
        {
        textBox.text = boxBuyController.boxCost.ToString();

        }
    }

    void Update()
    {
        if (infoPanel != null && infoPanel.activeSelf)
        {
            UpdateMafiaInfo();
            UpdateCashInfo();
            UpdateWeaponInfo();
            UpdateBoxInfo();
        }
    }

    private void UpdateMafiaInfo()
    {
        if(clientManager.mafia != null)
        {
            currentMafia = clientManager.mafia.GetComponent<MafiaController>();

            if (currentMafia != null && !currentMafia.served)
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
        else
        {
            descriptionText.text = "No hay mafioso presente";
            orderText.text = "Sin pedido";
        }
    }

    public void UpdateCashInfo()
    {
        textCash.text =  orderController.cash.ToString();
    }
    public void UpdateBoxInfo()
    {
        boxButton.interactable = orderController.cash >= boxBuyController.boxCost;
    }

    public void UpdateWeaponInfo()
    {
        int value = weaponController.currentDurability;
        textWeapon.text = $"Durabilidad del cuchillo (max "+ weaponController.maxDurability + "): " + value;
        repairSlider.value = (float)value/(float)weaponController.maxDurability;
        Debug.Log("durab: "+ (float)value / (float)weaponController.maxDurability);
        repairSliderFillImage.color = Color.Lerp(Color.red, Color.green, (float)value/ (float)weaponController.maxDurability);
        UpdateRepairButton();
    }

    private void UpdateRepairButton()
    {
        if (repairButton != null)
        {
            bool canRepair = weaponController.currentDurability <= 0 && orderController.cash >= weaponController.repairCost;
            repairButton.interactable = canRepair;
            

            if (!canRepair)
            {
                ShowRepairMessage("No tienes suficiente dinero para reparar el cuchillo.");
            }
            else
            {
                HideRepairMessage();
            }

            if (weaponController.currentDurability > 0)
            {
                ShowRepairMessage("Tu arma todavía no necesita reparación.");
            }
            else
            {
                HideRepairMessage();
            }
        }
    }

    private void ShowRepairMessage(string message)
    {
        repairMessage.gameObject.SetActive(true); 
        repairMessage.text = message;
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(HideMessageIEnum());
    }
    private IEnumerator HideMessageIEnum()
    {
        yield return new WaitForSeconds(3.0f);
        HideRepairMessage();
    }

    private void HideRepairMessage()
    {
        repairMessage.gameObject.SetActive(false);
    }



}
