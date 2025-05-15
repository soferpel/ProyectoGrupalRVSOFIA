using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RagdollNetworkManager : NetworkBehaviour
{
    public GameObject ragdollBoneFollowerPrefab;
    public Transform[] bonesToFollow;
    public void SpawnFollowerBones()
    {
        for (int i=0;i<bonesToFollow.Length;i++)
        {
            Transform bone = bonesToFollow[i];
            if (bone.TryGetComponent<BoxCollider>(out BoxCollider originalCollider))
            {

                GameObject go = Instantiate(ragdollBoneFollowerPrefab, bone.position, bone.rotation);
                var follower = go.GetComponent<RagdollBoneFollower>();
                BoxCollider collider = go.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = go.AddComponent<BoxCollider>();
                }

                Vector3 scaleFactor = new Vector3(
                    bone.lossyScale.x / go.transform.lossyScale.x,
                    bone.lossyScale.y / go.transform.lossyScale.y,
                    bone.lossyScale.z / go.transform.lossyScale.z
                );

                collider.size = Vector3.Scale(originalCollider.size, scaleFactor);
                collider.center = Vector3.Scale(originalCollider.center, scaleFactor);
                follower.targetBone = bone;
                go.GetComponent<NetworkObject>().Spawn();

                if (!bone.TryGetComponent<BoneTracker>(out BoneTracker tracker))
                {
                    tracker = bone.gameObject.AddComponent<BoneTracker>();
                }
                tracker.follower = follower;

                StartCoroutine(DelayedSetUp(go.GetComponent<RagdollBoneFollower>(), scaleFactor, originalCollider.size, originalCollider.center,i));
            }
        }
    }
    private IEnumerator DelayedSetUp(RagdollBoneFollower follower, Vector3 scaleFactor, Vector3 size, Vector3 center,int i)
    {
        yield return new WaitUntil(() => follower.GetComponent<NetworkObject>().IsSpawned);

        follower.SetUp(scaleFactor, size, center);
        NetworkObjectReference goRef = follower.gameObject.GetComponent<NetworkObject>();
        SettargetClientRpc(goRef, i);
    }
    [ClientRpc]
    private void SettargetClientRpc(NetworkObjectReference goRef,int i)
    {
        if (goRef.TryGet(out NetworkObject goNetObj))
        {
            GameObject go = goNetObj.gameObject;
            go.GetComponent<RagdollBoneFollower>().targetBone = bonesToFollow[i];
        }
    }
}
public class BoneTracker : MonoBehaviour
{
    public RagdollBoneFollower follower;
}
