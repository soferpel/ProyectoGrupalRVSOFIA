using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TemporalBorrar : MonoBehaviour
{

    private Animator animator;
    public GameObject[] sliceableParts;
    public List<Collider> bodyPartsCollider;
    public LayerMask sliceLayer;

    public Transform finalPoint;
    public float waitTime = 2f;
    public float distanceThreshold = 1f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isFinalMove = false;
    public bool isAlive = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        StartCoroutine(MoveToPoints());
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

        }
    }

    private void MoveToFinalPoint()
    {
        if (finalPoint == null) return;

        currentTarget = finalPoint;
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

        // Reiniciar la lógica de movimiento para dirigirse al punto final
        StopCoroutine(MoveToPoints());
        MoveToFinalPoint();
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
            isAlive = false; 
            StopCoroutine(MoveToPoints()); 
            agent.isStopped = true; 
            animator.SetTrigger("isDead");
            yield return new WaitForSeconds(1f);
            foreach (GameObject part in sliceableParts)
            {
                part.layer = LayerMask.NameToLayer("Sliceable");
            }
            foreach (Collider collider in bodyPartsCollider)
            {
                collider.enabled = true;
            }
        }
    }

    

}



