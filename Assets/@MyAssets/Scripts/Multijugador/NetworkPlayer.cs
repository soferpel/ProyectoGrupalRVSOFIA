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
    
    [SerializeField] private GameObject headGO;
    [SerializeField] private GameObject[] hairs;
    [SerializeField] private Texture[] hairTextures;
    [SerializeField] private Texture[] skinTextures;
    [SerializeField] private Color[] handSkinTextures;

    public int skinIndex;
    public int hairStyleIndex;
    public int hairColorIndex;

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

    [ClientRpc]
    public void UpdateCharacterClientRpc(int skinIndex, int hairStyleIndex, int hairColorIndex)
    {
        this.skinIndex = skinIndex;
        this.hairStyleIndex = hairStyleIndex;
        this.hairColorIndex = hairColorIndex;
        headGO.GetComponent<Renderer>().materials[0].mainTexture = skinTextures[skinIndex];
        leftHand.GetChild(0).GetComponent<Renderer>().materials[0].color = handSkinTextures[skinIndex];
        rightHand.GetChild(0).GetComponent<Renderer>().materials[0].color = handSkinTextures[skinIndex];

        for (int i = 0; i< hairs.Length; i++)
        {
            if(i== hairStyleIndex)
            {
                hairs[i].SetActive(true);
                hairs[i].GetComponent<Renderer>().materials[0].mainTexture = hairTextures[hairStyleIndex*3+hairColorIndex];
            }
            else
            {
                hairs[i].SetActive(false);
            }
        }
    }

}
