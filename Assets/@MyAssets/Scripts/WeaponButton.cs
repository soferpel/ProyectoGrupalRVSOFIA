using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    private WeaponController weaponController;
    private InfoUI infoUI;

    private void Start()
    {
        weaponController = FindObjectOfType<WeaponController>();
        infoUI = FindObjectOfType<InfoUI>();
    }

    public void RepairKnife()
    {
        if (weaponController != null)
        {
            weaponController.RepairKnife();
        }

        if (infoUI != null)
        {
            infoUI.UpdateWeaponInfo();
            infoUI.UpdateCashInfo();
        }
    }
}


