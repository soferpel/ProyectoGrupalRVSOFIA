using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MafiaController : PersonController
{

    public string appearanceDescription;
    public string orderDescription;

    private Transform player;

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

            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<VisionSensor>());
            agent.isStopped = true;
            agent.enabled = false;

            StartCoroutine(HandleAttackSequence());
        }
    }

    private IEnumerator HandleAttackSequence()
    {
        if (player != null)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            float rotationSpeed = 5f;

            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                yield return null;
            }
        }

        if (animator != null)
        {
            animator.SetBool("lookAround", false);
            animator.SetBool("walk", false);
            //animator.SetBool("attack", true);
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
