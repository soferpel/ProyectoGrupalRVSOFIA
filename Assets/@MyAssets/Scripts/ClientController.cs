using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClientController : MonoBehaviour
{
    
    private Animator animator;
    public GameObject[] sliceableParts;
    public List<Collider> bodyPartsCollider;
    public LayerMask sliceLayer;

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
    public bool isAlive = true;

    void Start()
    {
        //agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        animator = GetComponent<Animator>();
        //StartCoroutine(MoveToPoints());
        foreach (Collider collider in bodyPartsCollider)
        {
            collider.enabled = false;
        }
    }

    IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (isAlive)
        {
            if (isFinalMove)
            {
                MoveToFinalPoint();
                yield break;
            }

            if (!isGoingToBuy && Random.value < 0.5f && buyPoint != null && pointsVisited < pointsToVisit - 1)
            {
                currentTarget = buyPoint;
                waitTime = 5f;
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

            // Mover al destino
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

            agent.updateRotation = true;
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
        Debug.Log("El NPC ha terminado su recorrido.");
    }

    public void ReportDeath()
    {
        if (!isAlive) return;

        Debug.Log("ReportDeath llamado. Cambiando a movimiento final.");
        isFinalMove = true;

        //StopCoroutine(MoveToPoints());
        //MoveToFinalPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isAlive)
        {
            isAlive = false; // Marcar como muerto
            //StopCoroutine(MoveToPoints()); // Detener la rutina de movimiento
            //agent.isStopped = true; // Detener el movimiento del NavMeshAgent
            animator.SetTrigger("isDead");
            yield return new WaitForSeconds(1f);
            foreach (GameObject part in sliceableParts)
            {
                part.layer = LayerMask.NameToLayer("Sliceable");
            }
            Destroy(GetComponent<Rigidbody>());
            foreach (Collider collider in bodyPartsCollider)
            {
                collider.enabled = true;
            }

        }
    }

    public bool isOnBuyPoint()
    {
        float distance = Vector3.Distance(transform.position, buyPoint.position);
        bool isCloseToBuyPoint = distance <= distanceThreshold;
        if (isCloseToBuyPoint)
        {
            Debug.Log("Cliente cerca del Buy Point.");
        }
        return isCloseToBuyPoint;
    }
    
}



