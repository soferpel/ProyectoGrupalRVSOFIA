using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
public class ClientController : MonoBehaviour
{

    private Animator animator;

    private Transform buyPoint;
    private Transform finalPoint;
    private Transform doorExitPoint;
    private Transform finalDestinationPoint;
    public float waitTime = 2f;
    public float distanceThreshold = 1f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isFinalMove = false;
    private int pointsVisited = 0;
    private int pointsToVisit = 0;
    public bool isAlive = true;

    public Rigidbody[] deadRigidbodies;
    public Collider[] deadColliders;
    public int countBodyParts = 5;
    public GameObject body;
    public GameObject[] sliceableParts;
    private ShopNavigator shopNavigator;

    public float waitTimeBuyPoint = 10f;
    public bool inBuyPoint = false;
    public bool inQueue = false;
    public bool served = false;
    private BuyPointController buyPointController;
    private Coroutine waitTimeCoroutine;

    public ClientTimerSlider slider;
    void Start()
    {
        slider.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = 1;//Random.Range(2, 5);
        animator = GetComponent<Animator>();
        StartCoroutine(EnterFromDoor());
        agent.enabled = false;
        foreach (Rigidbody rg in deadRigidbodies)
        {
            rg.isKinematic = true;
        }
        foreach (Collider collider in deadColliders)
        {
            collider.enabled = false;
        }
    }
    IEnumerator EnterFromDoor()
    {
        Transform entryPoint = doorExitPoint;
        Transform finalEntryPoint = finalPoint;

        agent.enabled = false;
        animator.SetBool("walk", true);

        while (Vector3.Distance(transform.position, entryPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, entryPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(entryPoint.position - transform.position).normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }
        shopNavigator.OpenDoor();

        while (Vector3.Distance(transform.position, finalEntryPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalEntryPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(finalEntryPoint.position - transform.position).normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }


        agent.enabled = true;
        animator.SetBool("walk", false);
        StartCoroutine(MoveToPoints());

    }

    IEnumerator MoveToPoints()
    {
        while (isAlive && agent.enabled)
        {
            if (isFinalMove)
            {
                DecideFinalAction();
                yield break;
            }

            currentTarget = shopNavigator.GetRandomPointWithinStoreBounds();
            pointsVisited++;

            if (currentTarget != null)
            {
                yield return MoveToTarget(currentTarget, true);
            }

            yield return new WaitForSeconds(waitTime);

            if (pointsVisited >= pointsToVisit)
            {
                isFinalMove = true;
            }
        }
    }

    IEnumerator MoveToTarget(Transform target, bool lookAround)
    {
        if (agent.enabled)
        {

            agent.SetDestination(target.position);

            while (agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                if (agent.desiredVelocity.sqrMagnitude > 0.01f)
                {
                    Vector3 direction = -agent.desiredVelocity.normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }

                yield return null;
            }

            animator.SetBool("walk", false);
            if (lookAround) animator.SetBool("lookAround", true);
            agent.updateRotation = true;

            if ( target.gameObject != null && target.gameObject.name == "RandomTarget" || target.gameObject.name == "QueuePosition")
            {
                Destroy(target.gameObject);
            }
        }
    }

    void DecideFinalAction()
    {
        float decision = Random.value;

        if (decision < 1f && buyPoint != null)
        {
            StartCoroutine(MoveToBuyMode());
        }
        else
        {
            StartCoroutine(MoveToFinalPoint());
        }
    }

    private IEnumerator MoveToFinalPoint()
    {
        yield return StartCoroutine(MoveToTarget(finalPoint, false)); 
        yield return StartCoroutine(ExitThroughDoor());
    }

    private IEnumerator MoveToBuyMode()
    {
        if (!buyPointController.IsOccupied())
        {
            buyPointController.OccupyPoint(this);
            yield return StartCoroutine(MoveToTarget(buyPointController.buyPointTransform, false));
            StartWaitTimer();
            inBuyPoint = true;

        }
        else
        {
            bool inQueue= buyPointController.PlaceInQueue(this);

            if(inQueue){
                int queuePosition = buyPointController.customerQueue.IndexOf(this);
                if (queuePosition >= 0)
                {
                    Vector3 queuePositionOffset = buyPointController.buyPointTransform.position - new Vector3(2 * (queuePosition + 1), 0, 0);
                    Transform queuePositionTransform = new GameObject("QueuePosition").transform;
                    queuePositionTransform.position = queuePositionOffset;

                    yield return StartCoroutine(MoveToTarget(queuePositionTransform, false));
                }
            }
            else
            {
                StartCoroutine(MoveToFinalPoint());
            }
            
        }
        
    }
    private IEnumerator MoveToBuyPoint()
    {
        yield return StartCoroutine(MoveToTarget(buyPointController.buyPointTransform, false));
        StartWaitTimer();
        inBuyPoint = true;

    }
    public void MoveToQueuePosition(Vector3 newPosition, bool inbuyPoint)
    {
        Transform queuePositionTransform = new GameObject("QueuePosition").transform;
        queuePositionTransform.position = newPosition;

        if (inbuyPoint)
        {
            StartCoroutine(MoveToBuyPoint());
            Destroy(queuePositionTransform.gameObject);
        }
        else
        {
            StartCoroutine(MoveToTarget(queuePositionTransform, false));
        }

    }
    void StartWaitTimer()
    {
        if (waitTimeCoroutine != null)
        {
            StopCoroutine(waitTimeCoroutine);
        }
        waitTimeCoroutine = StartCoroutine(WaitAtBuyPoint());
    }

    IEnumerator WaitAtBuyPoint()
    {
        slider.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < waitTimeBuyPoint && !served)
        {
            elapsedTime += Time.deltaTime;
            slider.SetSliderValue(elapsedTime, waitTimeBuyPoint);
            yield return null;
        }

        if (!served)
        {
            buyPointController.FreePoint();
            StartCoroutine(MoveToFinalPoint());
        }
        slider.SetActive(false);
    }
    IEnumerator ExitThroughDoor()
    {
        Transform exitPoint = doorExitPoint;
        Transform finalExitPoint = finalDestinationPoint;

        agent.enabled = false;
        shopNavigator.OpenDoor();
        animator.SetBool("walk", true);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(exitPoint.position - transform.position).normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }


        while (Vector3.Distance(transform.position, finalExitPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalExitPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(finalExitPoint.position - transform.position).normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }

        Destroy(gameObject);
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
        }
    }

    public void ReportDeath()
    {
        if (!isAlive) return;

        isFinalMove = true;

        StopCoroutine(MoveToPoints());
        MoveToFinalDestination();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            StopAllCoroutines();
            StartCoroutine(Die(other));
        }
    }

    private IEnumerator Die(Collider other)
    {
        if (isAlive)
        {
            slider.SetActive(false);
            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<VisionSensor>());
            isAlive = false;
            if (inBuyPoint)
            {
                Debug.Log("MUERTO SERVICIO");
                buyPointController.FreePoint();
            }else if(inQueue)
            {
                Debug.Log("MUERTO EN COLA");
                buyPointController.RemoveClientFromQueue(this);
            }
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
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
        return inBuyPoint;
    }

    public void SetBuyPoint(Transform buyPoint)
    {
        this.buyPoint = buyPoint;
    }
    public void SetFinalPoint(Transform finalPoint)
    {
        this.finalPoint = finalPoint;
    }
    public void SetDoorExitPoint(Transform doorExitPoint)
    {
        this.doorExitPoint = doorExitPoint;
    }
    public void SetFinalDestinationPoint(Transform finalDestinationPoint)
    {
        this.finalDestinationPoint = finalDestinationPoint;
    }
    public void SetBuyPointController(BuyPointController buyPointController)
    {
        this.buyPointController = buyPointController;
    }
    public void SetShopNavigator(ShopNavigator shopNavigator)
    {
        this.shopNavigator = shopNavigator;
    }
}

