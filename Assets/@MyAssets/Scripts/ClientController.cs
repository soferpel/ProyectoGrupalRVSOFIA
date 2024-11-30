using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
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

    public Rigidbody[] deadRigidbodies;
    public Collider[] deadColliders;
    public int countBodyParts = 5;
    public GameObject body;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, patrolPoints.Count + 1);
        animator = GetComponent<Animator>();
        //StartCoroutine(MoveToPoints());
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

        while (isAlive)
        {
            if (isFinalMove)
            {
                currentTarget = finalPoints[Random.Range(0, finalPoints.Count)];
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
                StopMovement();
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

            agent.SetDestination(currentTarget.position);

            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                    if (agent.velocity.sqrMagnitude > 0.01f) 
                {
                    Vector3 direction = agent.velocity.normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(direction*-1);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
                yield return null;
            }

            animator.SetBool("walk", false);

            agent.updateRotation = true;
            animator.SetBool("lookAround",true);
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
        if(other.gameObject.GetComponent<WeaponController>())
        {
            StartCoroutine(Die(other));
        }
    }

    private IEnumerator Die(Collider other)
    {
        if (isAlive)
        {
            isAlive = false;
            //agent.isStopped = true; 
            //StopCoroutine(MoveToPoints());
            //gameObject.GetComponent<NavMeshAgent>().enabled = false;


            // 2. Apagar el Animator
            animator.enabled = false;

            



            // 5. Desactivar isKinematic y resetear fuerzas
            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.isKinematic = false; // Activar física
            }

            // 6. Activar colliders
            foreach (Collider col in deadColliders)
            {
                col.enabled = true;
            }



            Vector3 forceDirection = -(other.transform.position - transform.position).normalized; // Dirección hacia el punto de impacto
            float forceStrength = 4f; // Puedes ajustar esta fuerza
            Vector3 force = forceDirection * forceStrength;

            // 5. Aplicar fuerza a cada Rigidbody
            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.AddForce(force, ForceMode.Impulse); // Aplica la fuerza en forma de impulso
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
