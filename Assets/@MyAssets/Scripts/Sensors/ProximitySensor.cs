using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClientAI))]
public class ProximitySensor : MonoBehaviour
{
    ClientAI LinkedAI;

    // Start is called before the first frame update
    void Start()
    {
        LinkedAI = GetComponent<ClientAI>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int index = 0; index < DetectableTargetManager.Instance.AllTargets.Count; ++index)
        {
            var candidateTarget = DetectableTargetManager.Instance.AllTargets[index];

            // skip if ourselves
            if (candidateTarget.gameObject == gameObject)
                continue;

            //if (Vector3.Distance(LinkedAI.EyeLocation, candidateTarget.transform.position) <= LinkedAI.ProximityDetectionRange)
            //    LinkedAI.ReportInProximity(candidateTarget);
        }
    }
}