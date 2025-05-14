using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class SliceablePartController : MonoBehaviour
{
    public GameObject target;
    public GameObject attachPoint;
    public GameObject[] clothes;
    public GameObject[] clothesChild;
    public Collider[] boundsColliders;
    public string gameObjectTag;
    public ClientController client;
    public ClientControllerMP clientMP;
    public List<string> boneNames;
    public Bounds[] ConvertCollidersToBounds()
    {
        if (boundsColliders == null || boundsColliders.Length == 0)
        {
            return new Bounds[0]; 
        }

        List<Bounds> boundsList = new List<Bounds>();

        foreach (var collider in boundsColliders)
        {
            if (collider != null)
            {
                boundsList.Add(collider.bounds);
            }
        }

        return boundsList.ToArray();
    }

}
