using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimationController : MonoBehaviour
{
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;

    private GameObject pokeInteractor;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        pokeInteractor = transform.parent.GetChild(0).gameObject;
    }

    void Update()
    {
        float triggerValue = triggerAction.action.ReadValue<float>();
        float gripValue = gripAction.action.ReadValue<float>();

        anim.SetFloat("Trigger", triggerValue);
        anim.SetFloat("Grip", gripValue);

        if (triggerValue > 0.8f)
        {
            pokeInteractor.SetActive(true);
        }
        else
        {
            pokeInteractor.SetActive(false);
        }
    }
}
