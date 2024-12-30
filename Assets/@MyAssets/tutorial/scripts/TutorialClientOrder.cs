using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialClientOrder : ClientController
{
    public TutorialManager tutorialManager;
    protected override void Start()
    {
        slider.SetActive(false);
        agent = GetComponent<NavMeshAgent>();
        pointsToVisit = 0;
        animator = GetComponent<Animator>();
        agent.enabled = false;
        buyProbability = 1f;
        foreach (Rigidbody rg in deadRigidbodies)
        {
            rg.isKinematic = true;
        }
        foreach (Collider collider in deadColliders)
        {
            collider.enabled = false;
        }

        StartCoroutine(EnterFromDoor());
    }

    protected override IEnumerator EnterFromDoor()
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
        StartCoroutine(MoveToBuyPoint());
    }


    protected override IEnumerator WaitAtBuyPoint()
    {
        slider.SetActive(true);
        float elapsedTime = 0f;
        while (elapsedTime < waitTimeBuyPoint && !served)
        {
            elapsedTime += Time.deltaTime;
            slider.SetSliderValue(elapsedTime, waitTimeBuyPoint);
            yield return null;
        }
        if(served)tutorialManager.NextStep(2);
        Debug.Log("x Me voy");
        buyPointController.FreePoint();
        StartCoroutine(MoveToFinalPoint());
        slider.SetActive(false);
    }
    protected override IEnumerator MoveToBuyPoint()
    {
        yield return StartCoroutine(MoveToTarget(buyPointController.buyPointTransform, false));
        buyPointController.OccupyPoint(this);
        StartWaitTimer();
        inBuyPoint = true;
    }

    }
