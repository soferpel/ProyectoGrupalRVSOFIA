using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClientAI))]
public class VisionSensor : MonoBehaviour
{
    [SerializeField] LayerMask DetectionMask = ~0;
    public Transform eyeOffset;

    ClientAI LinkedAI;

    void Start()
    {
        LinkedAI = GetComponent<ClientAI>();
        Collider selfCollider = GetComponent<Collider>();
        if (selfCollider != null)
        {
            Physics.IgnoreCollision(selfCollider, selfCollider);
        }
    }


    void Update()
    {
        for (int index = 0; index < DetectableTargetManager.Instance.AllTargets.Count; ++index)
        {
            var candidateTarget = DetectableTargetManager.Instance.AllTargets[index];

            if (candidateTarget.gameObject == gameObject)
                continue;

            var vectorToTarget = candidateTarget.transform.position - LinkedAI.EyeLocation;

            if (vectorToTarget.sqrMagnitude > (LinkedAI.VisionConeRange * LinkedAI.VisionConeRange))
                continue;

            vectorToTarget.Normalize();

            if (Vector3.Dot(vectorToTarget, LinkedAI.EyeDirection) < LinkedAI.CosVisionConeAngle)
                continue;

            RaycastHit hitResult;
            if (Physics.Raycast(LinkedAI.EyeLocation, vectorToTarget, out hitResult,
                                LinkedAI.VisionConeRange, DetectionMask, QueryTriggerInteraction.Collide))
            {
                Debug.DrawRay(LinkedAI.EyeLocation, vectorToTarget, Color.red, 1f);
                if (hitResult.collider.GetComponentInParent<DetectableTarget>() == candidateTarget)
                {

                    Debug.Log("VEO 1:Dentro del if");
                    LinkedAI.ReportCanSee(candidateTarget);
                }
            }
        }
    }
}
