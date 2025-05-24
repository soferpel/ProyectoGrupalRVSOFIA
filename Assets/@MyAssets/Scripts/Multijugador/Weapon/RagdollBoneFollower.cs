using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class RagdollBoneFollower : NetworkBehaviour
{
    public Transform targetBone=null;
    private NetworkObject networkObject;
    private XRGrabInteractable grab;
    public NetworkVariable<bool> isGrabbed = new NetworkVariable<bool>(false);


    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
        grab.selectEntered.AddListener((args) => RequestOwnership());
        grab.selectExited.AddListener((args) => ReleaseOwnership());
    }
    public void SetUp(Vector3 scaleFactor, Vector3 originalSize, Vector3 originalCenter)
    {
        SetUpClientRpc(scaleFactor,originalSize,originalCenter);
    }

    [ClientRpc]
    public void SetUpClientRpc(Vector3 scaleFactor, Vector3 originalSize, Vector3 originalCenter)
    {
        BoxCollider collider = GetComponent<BoxCollider>();

        collider.size = Vector3.Scale(originalSize, scaleFactor);
        collider.center = Vector3.Scale(originalCenter, scaleFactor);
    }
    float positionThreshold = 0.001f;
    float rotationThreshold = 0.1f;
    void Update()
    {
        if (targetBone != null && (IsOwner || IsServer) && (Vector3.Distance(targetBone.position, transform.position) > positionThreshold || Quaternion.Angle(targetBone.rotation, transform.rotation) > rotationThreshold))
        {
            if (IsOwner)
            {

                if (!isGrabbed.Value && targetBone != null)
                {
                    transform.position = targetBone.position;
                    transform.rotation = targetBone.rotation;
                }
            }
            if (IsServer)
            {

                if (isGrabbed.Value && targetBone != null)
                {
                    targetBone.position = transform.position;
                    targetBone.rotation = transform.rotation;
                }
            }
        }

    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        SetIsGrabbedServerRpc(true);
        if (targetBone != null)
        {
            targetBone.GetComponent<Rigidbody>().isKinematic = true;
            TargetBoneKinematicServerRpc(true);
        }
        if (!IsOwner && NetworkManager.Singleton.IsClient)
        {
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        SetIsGrabbedServerRpc(false);
        if (targetBone != null)
        {
            targetBone.GetComponent<Rigidbody>().isKinematic = false;
            TargetBoneKinematicServerRpc(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestOwnershipServerRpc(ulong clientId)
    {
        NetworkObject.ChangeOwnership(clientId);
    }
    private void RequestOwnership()
    {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestOwnershipRpc(clientID);
    }

    private void ReleaseOwnership()
    {
        ReleaseOwnershipRpc();
    }


    [Rpc(SendTo.Server)]
    private void RequestOwnershipRpc(ulong clientID)
    {
        networkObject.ChangeOwnership(clientID);
    }

    [Rpc(SendTo.Server)]
    private void ReleaseOwnershipRpc()
    {
        networkObject.RemoveOwnership();
    }
    [ServerRpc(RequireOwnership = false)]
    void SetIsGrabbedServerRpc(bool grabbed)
    {
        isGrabbed.Value = grabbed;
    }
    [ServerRpc(RequireOwnership = false)]
    void TargetBoneKinematicServerRpc(bool isKinematic)
    {
        targetBone.GetComponent<Rigidbody>().isKinematic = isKinematic;
    }
}
