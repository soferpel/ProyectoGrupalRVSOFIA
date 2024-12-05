using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientTimerSlider : MonoBehaviour
{
    public Slider timerSlider;
    public Image sliderFill;
    private bool isActive;

    public void SetSliderValue(float value, float maxValue)
    {
        timerSlider.value = 1-(value/maxValue);
        UpdateSliderColor(1 - (value / maxValue));
    }
    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        timerSlider.transform.parent.gameObject.SetActive(isActive);
    }

    void LateUpdate()
    {
        if (isActive && Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            directionToCamera.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
            Vector3 eulerRotation = targetRotation.eulerAngles;
            eulerRotation.x = 0;
            eulerRotation.z = 0;
            timerSlider.transform.parent.rotation = Quaternion.Euler(eulerRotation);
        }
    }

    void UpdateSliderColor(float percentage)
    {
        if (sliderFill != null)
        {
            sliderFill.color = Color.Lerp(Color.red, Color.green, percentage);
        }
    }
}
