using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
public class ClientController : MonoBehaviour
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
    public bool isAlive = true;
    public bool hasBeenAttacked = false;

    public Rigidbody[] deadRigidbodies;
    public Collider[] deadColliders;
    public int countBodyParts = 5;
    public GameObject body;
    public GameObject[] sliceableParts;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        animator = GetComponent<Animator>();
        StartCoroutine(MoveToPoints());
        foreach(Rigidbody rg in deadRigidbodies)
        {
            rg.isKinematic = true;
        }
        foreach (Collider collider in deadColliders)
        {
            collider.enabled = false;
        }
    }

    IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (isAlive && agent.enabled)
        {
            if (isFinalMove)
            {
                MoveToFinalPoint();
                yield break;
            }

            if (!isGoingToBuy && Random.value < 0.5f && buyPoint != null && pointsVisited < pointsToVisit - 1)
            {
                currentTarget = buyPoint;
                waitTime = 150f;
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

            while (agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
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
        while (agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
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
        if (agent.enabled)
        {
            agent.isStopped = true;
            Debug.Log("El NPC ha terminado su recorrido.");
        }
    }

    public void ReportDeath()
    {
        if (!isAlive) return;

        Debug.Log("ReportDeath llamado. Cambiando a movimiento final.");
        isFinalMove = true;

        StopCoroutine(MoveToPoints());
        MoveToFinalPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("Cliente atacado por un cuchillo.");
            hasBeenAttacked = true;
            StartCoroutine(Die(other));
        }
    }

    private IEnumerator Die(Collider other)
    {
        if (isAlive)
        {
            StopCoroutine(MoveToPoints());
            StopCoroutine(MoveToFinalDestination());
            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<VisionSensor>());
            isAlive = false;
            agent.isStopped = true;
            agent.enabled = false;
            animator.enabled = false;
            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.isKinematic = false;
            }

            foreach (Collider col in deadColliders)
            {
                col.enabled = true;
                col.gameObject.AddComponent<DetectableTarget>();
                col.gameObject.layer = LayerMask.NameToLayer("BodyParts");
            }

            Vector3 forceDirection = -(other.transform.position - transform.position).normalized; 
            float forceStrength = 4f;
            Vector3 force = forceDirection * forceStrength;

            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(2f);

            foreach (GameObject part in sliceableParts)
            {
                part.layer = LayerMask.NameToLayer("Sliceable");
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

