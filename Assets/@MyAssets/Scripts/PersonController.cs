using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class PersonController : MonoBehaviour
{
    public ClientTimerSlider slider;
    public bool isAlive = true;
    public bool isReported = false;
    public float waitTime = 2f;
    public bool inBuyPoint = false;
    public bool served = false;
    public bool inQueue = false;
    public float waitTimeBuyPoint = 10f;

    protected NavMeshAgent agent;
    protected Animator animator;
    protected ShopNavigator shopNavigator;
    protected int pointsToVisit = 0;
    protected int pointsVisited = 0;
    protected Transform doorExitPoint;
    protected Transform finalPoint;
    protected Transform finalDestinationPoint;
    protected bool isFinalMove = false;
    protected Transform currentTarget;
    protected Transform buyPoint;
    protected BuyPointController buyPointController;
    protected Coroutine waitTimeCoroutine;
    protected float buyProbability = 0;
    protected int personDirection = 1;
    protected virtual void Start()
    {
        slider.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = Random.Range(2, 4);
        animator = GetComponent<Animator>();
        StartCoroutine(EnterFromDoor());
        agent.enabled = false;
    }

    protected IEnumerator EnterFromDoor()
    {
        Transform entryPoint = doorExitPoint;
        Transform finalEntryPoint = finalPoint;

        agent.enabled = false;
        animator.SetBool("walk", true);

        while (Vector3.Distance(transform.position, entryPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, entryPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(entryPoint.position - transform.position).normalized * personDirection;

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
            Vector3 direction = -(finalEntryPoint.position - transform.position).normalized * personDirection;

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

    protected IEnumerator MoveToPoints()
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

            yield return new WaitForSeconds(Random.Range(1, 4));

            if (pointsVisited >= pointsToVisit)
            {
                isFinalMove = true;
            }
        }
    }

    protected void DecideFinalAction()
    {
        float decision = Random.value;

        if (decision <= buyProbability && buyPoint != null)
        {
            StartCoroutine(MoveToBuyMode());
        }
        else
        {
            StartCoroutine(MoveToFinalPoint());
        }
    }

    protected IEnumerator MoveToBuyMode()
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
            bool inQueue = buyPointController.PlaceInQueue(this);

            if (inQueue)
            {
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

    protected IEnumerator MoveToTarget(Transform target, bool lookAround)
    {
        if (inBuyPoint)
        {
            buyPointController.FreePoint();
        }
        else if (inQueue)
        {
            buyPointController.RemoveClientFromQueue(this);
        }
        if (agent.enabled)
        {

            agent.SetDestination(target.position);

            while (agent.enabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
            {
                animator.SetBool("lookAround", false);
                animator.SetBool("walk", true);

                if (agent.desiredVelocity.sqrMagnitude > 0.01f)
                {
                    Vector3 direction = -agent.desiredVelocity.normalized * personDirection;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
                }

                yield return null;
            }

            animator.SetBool("walk", false);
            if (lookAround) animator.SetBool("lookAround", true);
            agent.updateRotation = true;

            if (target.gameObject != null && target.gameObject.name == "RandomTarget" || target.gameObject.name == "QueuePosition")
            {
                Destroy(target.gameObject);
            }
        }
    }

    protected IEnumerator MoveToFinalPoint()
    {
        yield return StartCoroutine(MoveToTarget(finalPoint, false));
        yield return StartCoroutine(ExitThroughDoor());
    }

    protected IEnumerator ExitThroughDoor()
    {
        Transform exitPoint = doorExitPoint;
        Transform finalExitPoint = finalDestinationPoint;

        agent.enabled = false;
        shopNavigator.OpenDoor();
        animator.SetBool("walk", true);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, Time.deltaTime * 2f);
            Vector3 direction = -(exitPoint.position - transform.position).normalized * personDirection;

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
            Vector3 direction = -(finalExitPoint.position - transform.position).normalized * personDirection;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 15f);
            }

            yield return null;
        }

        if (this is ClientController client && client.isReported)
        {
            Debug.Log("Cadáver reportado. Fin del juego.");
            GameManager.isGameOver = true;
        }

        Destroy(gameObject);
    }

    protected void StartWaitTimer()
    {
        if (waitTimeCoroutine != null)
        {
            StopCoroutine(waitTimeCoroutine);
        }
        waitTimeCoroutine = StartCoroutine(WaitAtBuyPoint());
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

    protected IEnumerator WaitAtBuyPoint()
    {
        slider.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < waitTimeBuyPoint && !served)
        {
            elapsedTime += Time.deltaTime;
            slider.SetSliderValue(elapsedTime, waitTimeBuyPoint);
            yield return null;
        }
        Debug.Log("x Me voy");
        buyPointController.FreePoint();
        StartCoroutine(MoveToFinalPoint());
        slider.SetActive(false);
    }

    protected IEnumerator MoveToBuyPoint()
    {
        yield return StartCoroutine(MoveToTarget(buyPointController.buyPointTransform, false));
        StartWaitTimer();
        inBuyPoint = true;

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
