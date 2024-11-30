using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DetectableTargetManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (DetectableTargetManager.Instance != null)
            DetectableTargetManager.Instance.Deregister(this);
    }
}