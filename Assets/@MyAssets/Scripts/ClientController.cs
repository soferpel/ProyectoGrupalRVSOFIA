using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClientController : MonoBehaviour
{
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

    //private Animator animator;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        //animator = GetComponent<Animator>();
        StartCoroutine(MoveToPoints());
    }
        IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (true)
        {
            if (isFinalMove)
            {
                currentTarget = finalPoints[Random.Range(0, finalPoints.Count)];
                agent.SetDestination(currentTarget.position);

                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    yield return null;
                }

                StopMovement();
                yield break;
            }

            if (!isGoingToBuy && Random.value < 0.5f && buyPoint != null && pointsVisited < pointsToVisit - 1)
            {
                currentTarget = buyPoint;
                waitTime = 50f; //editar
                isGoingToBuy = true;
                pointsVisited++;
            }
            else
            {
                currentTarget = patrolPoints[Random.Range(0, patrolPoints.Count)];

                while (visitedPoints.Contains(currentTarget))
                {
                    currentTarget = patrolPoints[Random.Range(0, patrolPoints.Count)];
                }

                visitedPoints.Add(currentTarget); 
                pointsVisited++;
            }

            agent.SetDestination(currentTarget.position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                //animator.SetBool("walk", true);
                yield return null;
            }

            //animator.SetBool("walk", false);
            yield return new WaitForSeconds(waitTime);

            if (pointsVisited >= pointsToVisit || isGoingToBuy)
            {
                isFinalMove = true;
            }
        }
    }

    private void StopMovement()
    {
        agent.isStopped = true;
        Debug.Log("El NPC ha terminado su recorrido.");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Client: OnTriggerEnter");
        if (other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("Client: OnTriggerEnter cuchillo");
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
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

        /*
        Debug.Log("Cliente esta en BuyPoint");
        Debug.Log(transform.position == buyPoint.position);
        return transform.position == buyPoint.position;
        */
    }
}
