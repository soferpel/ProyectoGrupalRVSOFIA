using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MafiaController : MonoBehaviour
{
    private Animator animator;
    public List<Transform> patrolPoints;
    public Transform buyPoint;
    public List<Transform> finalPoints;
    public float waitTime = 2f;
    public float distanceThreshold = 1f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isGoingToBuy = false;
    private bool isFinalMove = false;
    private int pointsVisited = 0;
    private int pointsToVisit = 0;

    public string appearanceDescription;
    public string orderDescription;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        GenerateOrder();
        StartCoroutine(MoveToPoints());
    }

    IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (true) 
        {
            if (isFinalMove)
            {
                MoveToFinalPoint();
                yield break;
            }

            if (!isGoingToBuy && Random.value < 0.5f && buyPoint != null && pointsVisited < pointsToVisit - 1)
            {
                currentTarget = buyPoint;
                waitTime = 500f;
                isGoingToBuy = true;
                pointsVisited++;
            }
            else
            {
                do
                {
                    currentTarget = patrolPoints[Random.Range(0, patrolPoints.Count)];
                } while (visitedPoints.Contains(currentTarget));

                visitedPoints.Add(currentTarget);
                pointsVisited++;
            }

            agent.SetDestination(currentTarget.position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                if (agent.velocity.sqrMagnitude > 0.01f)
                {
                    Vector3 direction = agent.velocity.normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(direction * -1);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                yield return null;
            }

            animator.SetBool("walk", false);
            animator.SetBool("lookAround", true);
            yield return new WaitForSeconds(waitTime);

            if (pointsVisited >= pointsToVisit || isGoingToBuy)
            {
                isFinalMove = true;
            }
        }
    }

    private void MoveToFinalPoint()
    {
        if (finalPoints.Count == 0) return;

        currentTarget = finalPoints[Random.Range(0, finalPoints.Count)];
        agent.SetDestination(currentTarget.position);
        Debug.Log(AppearanceDescription);
        StartCoroutine(MoveToFinalDestination());
    }

    private IEnumerator MoveToFinalDestination()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            animator.SetBool("walk", true);

            if (agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = agent.velocity.normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction * -1);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
            yield return null;
        }

        animator.SetBool("walk", false);
        StopMovement();
    }

    private void StopMovement()
    {
        agent.isStopped = true;
        Debug.Log("El mafioso ha terminado su recorrido.");
    }

    public bool isOnBuyPoint()
    {
        float distance = Vector3.Distance(transform.position, buyPoint.position);
        bool isCloseToBuyPoint = distance <= distanceThreshold;
        if (isCloseToBuyPoint)
        {
            Debug.Log("Mafioso cerca del punto de compra.");
        }
        return isCloseToBuyPoint;
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
        }
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
