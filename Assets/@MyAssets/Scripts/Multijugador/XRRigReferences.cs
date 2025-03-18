using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRigReferences : MonoBehaviour
{
    public static XRRigReferences Instance;

    public Transform root; 
    public Transform head; 
    public Transform leftHand; 
    public Transform rightHand; 

    private void Awake()
    {
        Instance = this;
    }
}
