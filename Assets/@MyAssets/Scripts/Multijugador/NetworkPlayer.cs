using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] Transform root;
    [SerializeField] Transform head;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [SerializeField] Renderer[] meshesToDisable;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
            return;

        foreach (Renderer r in meshesToDisable)
        {
            r.enabled = false;
        }
    }
    void Update()
    {
        if (!IsOwner)
            return;

        root.SetPositionAndRotation(XRRigReferences.Instance.root.position, XRRigReferences.Instance.root.rotation);
        head.SetPositionAndRotation(XRRigReferences.Instance.head.position, XRRigReferences.Instance.head.rotation);
        leftHand.SetPositionAndRotation(XRRigReferences.Instance.leftHand.position, XRRigReferences.Instance.leftHand.rotation);
        rightHand.SetPositionAndRotation(XRRigReferences.Instance.rightHand.position, XRRigReferences.Instance.rightHand.rotation);
    }
}
