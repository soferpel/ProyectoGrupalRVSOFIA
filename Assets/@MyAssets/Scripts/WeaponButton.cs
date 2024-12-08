using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    private WeaponController weaponController;
    public InfoUI infoUI;

    private void Start()
    {
        weaponController = infoUI.weaponController;
    }

    public void RepairKnife()
    {
        if (weaponController != null)
        {
            Debug.Log("Cuchillo arreglado");
            weaponController.RepairKnife();
        }

        if (infoUI != null)
        {
            infoUI.UpdateWeaponInfo();
            infoUI.UpdateCashInfo();
        }
    }
}


