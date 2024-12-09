using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MafiaController : PersonController
{

    public string appearanceDescription;
    public string orderDescription;
    public GameObject gun;
    public GameObject player;

    protected override void Start()
    {
        buyProbability = 1;
        GenerateOrder();
        base.Start();
    }

    private void GenerateOrder()
    {
        string[] bodyParts = { "Torso", "Cabeza", "Pierna", "Brazo"};
        string selectedPart = bodyParts[Random.Range(0, bodyParts.Length)];
        orderDescription = selectedPart + "";
        Debug.Log("Pedido del mafioso: " + orderDescription);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("El mafioso no puede ser atacado.");
            StopAllCoroutines();
            StartCoroutine(HandleAttackSequence());
        }
    }

    private IEnumerator HandleAttackSequence()
    {
        gun.SetActive(true);
        agent.enabled = false;
        
        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Vector3 flatDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            Quaternion lookRotation = Quaternion.LookRotation(flatDirection);
            Quaternion finalRotation = lookRotation * Quaternion.Euler(0, 80, 0);

            float rotationSpeed = 5f;
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


        GameManager.isGameOver = true;

        Debug.Log("Juego terminado. ¡Game Over!");

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
}
