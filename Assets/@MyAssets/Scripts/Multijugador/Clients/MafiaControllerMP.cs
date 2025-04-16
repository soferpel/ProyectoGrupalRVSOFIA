using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MafiaControllerMP : PersonControllerMP
{

    public string orderDescription;
    public GameObject gun;
    public GameObject player;
    private int audioRandom;
    private bool attacked = false;

    protected override void Start()
    {
        audioRandom = UnityEngine.Random.Range(0, 3);
        buyProbability = 1;
        base.Start();
    }

    public void GenerateOrder()
    {
        if (IsServer)
        {
            string[] bodyParts = { "Torso", "Cabeza", "Pierna", "Brazo" };
            string selectedPart = bodyParts[Random.Range(0, bodyParts.Length)];
            orderDescription = selectedPart + "";
            Debug.Log("Pedido del mafioso: " + orderDescription);

            OrderClientRpc(orderDescription);
        }
    }

    [ClientRpc]
    private void OrderClientRpc(string order)
    {
        orderDescription = order;
        Debug.Log("Pedido sincronizado: " + orderDescription);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.TryGetComponent<WeaponControllerMP>(out WeaponControllerMP weapon) && !attacked && weapon.isGrabbed.Value)
        {
            attacked = true;
            Debug.Log("El mafioso no puede ser atacado.");
            StopAllCoroutines();
            StartCoroutine(HandleAttackSequence());
        }
    }

    public IEnumerator HandleAttackSequence()
    {
        gun.SetActive(true);
        agent.enabled = false;

        if (player != null && IsServer)
        {
            Vector3 directionToPlayer = (Camera.main.transform.position - transform.position).normalized;
            Vector3 flatDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
            Quaternion finalRotation = lookRotation * Quaternion.Euler(0, 80, 0);

            float rotationSpeed = 5f;
            audioSource[4].Stop();
            if (audioRandom == 0)
            {
                audioSource[1].Play();
            }
            else if (audioRandom == 1)
            {
                audioSource[2].Play();
            }
            else
            {
                audioSource[3].Play();
            }
            if (animator != null)
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", false);
                animator.SetBool("attack", true);

            }
            while (Quaternion.Angle(transform.rotation, finalRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }
        }

        if (animator != null)
        {
            animator.SetBool("lookAround", false);
            animator.SetBool("walk", false);
            animator.SetBool("attack", true);
        }

        yield return new WaitForSeconds(2f);
        FullGameManager.Instance.GoToGameOver();
    }

    public string AppearanceDescription
    {
        get => appearanceDescription;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("La apariencia asignada es vacía o nula.");
            }
            else
            {
                appearanceDescription = value;
                Debug.Log("Apariencia del mafioso asignada: " + appearanceDescription);
            }
        }
    }
    void OnEnable()
    {
        Clothing.OnMafiaAppearanceGenerated += HandleAppearanceGenerated;
    }

    void OnDisable()
    {
        Clothing.OnMafiaAppearanceGenerated -= HandleAppearanceGenerated;
    }

    private void HandleAppearanceGenerated(string description)
    {
        AppearanceDescription = description;
    }

    public string getGeneratedOrder()
    {
        return orderDescription;
    }

    protected override IEnumerator WaitAtBuyPoint()
    {
        slider.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < waitTimeBuyPoint && !served)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
            elapsedTime += Time.deltaTime;
            slider.SetSliderValue(elapsedTime, waitTimeBuyPoint);
            yield return null;
        }
        Debug.Log("x Me voy");
        buyPointController.FreePoint();
        if (!served) StartCoroutine(HandleMafiaApproachAndAttack());
        else StartCoroutine(MoveToFinalPoint());
        slider.SetActive(false);
    }

    private IEnumerator HandleMafiaApproachAndAttack()
    {
        Transform playerTransform = Camera.main.transform;
        if (playerTransform == null)
        {
            yield break;
        }

        agent.stoppingDistance = 2;
        yield return StartCoroutine(MoveToPlayer());
        Debug.Log("HE LLEGADO");
        StartCoroutine(HandleAttackSequence());
    }

    protected IEnumerator MoveToPlayer()
    {

        if (agent.enabled)
        {

            agent.SetDestination(Camera.main.transform.position);

            while (agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                agent.SetDestination(Camera.main.transform.position);

                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                if (agent.desiredVelocity.sqrMagnitude > 0.01f)
                {
                    Vector3 direction = -agent.desiredVelocity.normalized * personDirection;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }

                yield return null;
            }

            animator.SetBool("walk", false);
            agent.updateRotation = true;


        }
    }

}
