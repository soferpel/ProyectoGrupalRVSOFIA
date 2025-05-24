using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(NetworkObject), typeof(XRGrabInteractable))]
public class NetworkedGrab : NetworkBehaviour
{
    private NetworkObject networkObject;
    private XRGrabInteractable interactable;

    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        interactable = GetComponent<XRGrabInteractable>();
        interactable.selectEntered.AddListener((args) => RequestOwnership());
        interactable.selectExited.AddListener((args) => ReleaseOwnership());
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
        if (networkObject.OwnerClientId != clientID)
        {
            networkObject.ChangeOwnership(clientID);
        }
    }

    [Rpc(SendTo.Server)]
    private void ReleaseOwnershipRpc()
    {
        networkObject.RemoveOwnership();
    }


}
