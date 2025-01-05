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
    private bool isOpen = false;

    private void Start()
    {
        socket.hoverEntered.AddListener(OnHoverEntered);
        socket.hoverExited.AddListener(OnHoverExited);
        socket.selectEntered.AddListener(OnObjectPlaced);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
       /* Debug.Log("SE LLAMA A HOVER ENTERED????");
        if (!isOpen)
        {
            isOpen = true;
            StartCoroutine(OpenLids());
        }*/
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        /*if (isOpen)
        {
            isOpen = false;
            StartCoroutine(CloseLids());
        }*/
    }


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
            // Encontrar el objeto raíz del cuerpo
            GameObject rootObject = FindRootObject(placedObject);

            // Destruir todos los hijos y el objeto raíz
            DestroyAllChildrenAndSelf(rootObject);

        }
    }
    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
    }

    private GameObject FindRootObject(GameObject obj)
    {
        // Recorre los padres hasta encontrar el objeto raíz (sin un padre)
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
        }
        return obj;
    }

    private void DestroyAllChildrenAndSelf(GameObject obj)
    {
        // Destruir todos los hijos del objeto principal
        foreach (Transform child in obj.transform)
        {
            DestroyAllChildrenAndSelf(child.gameObject); // Recursivamente destruir cada hijo
        }

        // Destruir el objeto principal después de los hijos
        Debug.Log($"Destruyendo: {obj.name}");
        Destroy(obj);
    }
    /*
    private void OnObjectPlaced(SelectEnterEventArgs args)
    {
        GameObject placedObject = args.interactableObject.transform.gameObject;
        Debug.Log($"OBJETO TIRADO: {placedObject.name}");
        Destroy(placedObject);
        /*
        GameObject rootObject = FindRootObject(placedObject);
        DestroyAllChildrenAndSelf(rootObject);
        

    }
    /*
    private GameObject FindRootObject(GameObject obj)
    {
        // Recorre los padres hasta encontrar el objeto raíz (sin un padre)
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
        }
        return obj;
    }

    private void DestroyAllChildrenAndSelf(GameObject obj)
    {
        // Destruir todos los hijos del objeto principal
        foreach (Transform child in obj.transform)
        {
            DestroyAllChildrenAndSelf(child.gameObject);  // Recursivamente destruir cada hijo
        }

        // Destruir el objeto principal después de los hijos
        Destroy(obj);
    }
    
    */

    private IEnumerator OpenLids()
    {
        float elapsed = 0f;
        //se guarda rotación inicial
        float leftStartY = lidLeft.localEulerAngles.y;
        float rightStartY = lidRight.localEulerAngles.y;

        float leftTargetY = leftStartY - lidOpenAngle;
        float rightTargetY = rightStartY - lidOpenAngle;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openCloseSpeed;

            lidLeft.localEulerAngles = new Vector3(
                lidLeft.localEulerAngles.x,
                Mathf.Lerp(leftStartY, leftTargetY, elapsed),
                lidLeft.localEulerAngles.z
            );

            lidRight.localEulerAngles = new Vector3(
                lidRight.localEulerAngles.x,
                Mathf.Lerp(rightStartY, rightTargetY, elapsed),
                lidRight.localEulerAngles.z
            );

            yield return null;
        }
    }

    private IEnumerator CloseLids()
    {
        float elapsed = 0f;
        float leftStartY = lidLeft.localEulerAngles.y;
        float rightStartY = lidRight.localEulerAngles.y;

        float leftTargetY = lidLeft.localEulerAngles.y + lidOpenAngle;
        float rightTargetY = lidRight.localEulerAngles.y + lidOpenAngle;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openCloseSpeed;

            lidLeft.localEulerAngles = new Vector3(
                lidLeft.localEulerAngles.x,
                Mathf.Lerp(leftStartY, leftTargetY, elapsed),
                lidLeft.localEulerAngles.z
            );

            lidRight.localEulerAngles = new Vector3(
                lidRight.localEulerAngles.x,
                Mathf.Lerp(rightStartY, rightTargetY, elapsed),
                lidRight.localEulerAngles.z
            );

            yield return null;
        }
    }
}


