using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClientController : MonoBehaviour
{
    private int lives;
    private Animator animator;
    public GameObject[] sliceableParts;
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
    private bool isAlive = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        animator = GetComponent<Animator>();
        StartCoroutine(MoveToPoints());
    }
    IEnumerator MoveToPoints()
    {
        List<Transform> visitedPoints = new List<Transform>();

        while (isAlive)
        {
            if (isFinalMove)
            {
                // Elegir punto final
                currentTarget = finalPoints[Random.Range(0, finalPoints.Count)];
                agent.SetDestination(currentTarget.position);

                while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                {
                    animator.SetBool("lookAround", false);
                    animator.SetBool("walk", true);

                    if (agent.velocity.sqrMagnitude > 0.01f) // Verificar que hay movimiento
                    {
                        Vector3 direction = agent.velocity.normalized; // Dirección del movimiento
                        Quaternion lookRotation = Quaternion.LookRotation(direction * -1);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Ajustar suavemente la rotación
                    }
                    yield return null;
                }

                animator.SetBool("walk", false);
                StopMovement();
                yield break;
            }

            // Decidir siguiente destino
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

                    if (agent.velocity.sqrMagnitude > 0.01f) // Verificar que hay movimiento
                {
                    Vector3 direction = agent.velocity.normalized; // Dirección del movimiento
                    Quaternion lookRotation = Quaternion.LookRotation(direction*-1);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Ajustar suavemente la rotación
                }
                yield return null;
            }

            animator.SetBool("walk", false);

            // Animación de mirar alrededor
            agent.updateRotation = true;
            animator.SetBool("lookAround",true);
            yield return new WaitForSeconds(waitTime);
            // Decidir si es el movimiento final
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
        if(other.gameObject.GetComponent<WeaponController>())
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        if (isAlive)
        {

            isAlive = false; // Marcar como muerto
            agent.isStopped = true; // Detener el movimiento del NavMeshAgent
            StopCoroutine(MoveToPoints()); // Detener la rutina de movimiento
            gameObject.GetComponent<NavMeshAgent>().enabled = false;
            animator.SetTrigger("isDead");
            yield return new WaitForSeconds(1f);
            foreach(GameObject part in sliceableParts)
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
