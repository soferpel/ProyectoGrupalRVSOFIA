using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DumpsterController : MonoBehaviour
{
    public XRSocketInteractor socket;
    public Transform lidLeft;
    public Transform lidRight;
    public float lidOpenAngle = 90f;
    public float openCloseSpeed = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("BodyParts"))
        {
            GameObject placedObject = other.gameObject;
            Debug.Log($"OBJETO TIRADO: {placedObject.name}");
            if(placedObject.TryGetComponent<TrashPart>(out TrashPart trashPart))
            {
                Destroy(trashPart.part);
            }
            GameObject rootObject = FindRootObject(placedObject);
            DestroyAllChildrenAndSelf(rootObject);
        }
    }

    private GameObject FindRootObject(GameObject obj)
    {
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
        }
        return obj;
    }

    private void DestroyAllChildrenAndSelf(GameObject obj)
    {
        foreach (Transform child in obj.transform)
        {
            DestroyAllChildrenAndSelf(child.gameObject);
        }
        Debug.Log($"Destruyendo: {obj.name}");
        Destroy(obj);
    }
    
}


