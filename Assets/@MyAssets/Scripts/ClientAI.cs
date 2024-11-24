using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(AwarenessSystem))]
public class ClientAI : MonoBehaviour
{
    ClientController clientController;
    [SerializeField] float _VisionConeAngle = 60f;
    [SerializeField] float _VisionConeRange = 30f;
    [SerializeField] Color _VisionConeColour = new Color(1f, 0f, 0f, 0.25f);

    [SerializeField] float _ProximityDetectionRange = 3f;
    [SerializeField] Color _ProximityRangeColour = new Color(1f, 1f, 1f, 0.25f);

    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => -transform.right;

    public float VisionConeAngle => _VisionConeAngle;
    public float VisionConeRange => _VisionConeRange;
    public Color VisionConeColour => _VisionConeColour;

    public float ProximityDetectionRange => _ProximityDetectionRange;
    public Color ProximityDetectionColour => _ProximityRangeColour;
    public float CosVisionConeAngle { get; private set; } = 0f;

    AwarenessSystem Awareness;

    void Awake()
    {
        CosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);
        Awareness = GetComponent<AwarenessSystem>();
        clientController = GetComponent<ClientController>();
    }

    

    public void ReportCanSee(DetectableTarget seen)
    {
        Awareness.ReportCanSee(seen);
    }


    public void ReportInProximity(DetectableTarget target)
    {
        Awareness.ReportInProximity(target);
    }
    /*
    public void OnSuspicious()
    {
        Debug.Log("I hear you");
    }
    */
    public void OnDetected(GameObject target)
    {
        Debug.Log("I see you " + target.gameObject.name);
        ClientController clientController = GetComponent<ClientController>();

        if (clientController != null)
        {
            clientController.ReportDeath(); // Llamar a la función
        }
        else
        {
            Debug.LogError("No se encontró un ClientController en este objeto.");
        }
    }
    /*
    public void OnFullyDetected(GameObject target)
    {
        Debug.Log("Charge! " + target.gameObject.name);
    }
    */
    public void OnLostDetect(GameObject target)
    {
        Debug.Log("Where are you " + target.gameObject.name);
    }

    public void OnLostSuspicion()
    {
        Debug.Log("Where did you go");
    }

    public void OnFullyLost()
    {
        Debug.Log("Must be nothing");
    }


}

#if UNITY_EDITOR
[CustomEditor(typeof(ClientAI))]
public class ClientAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as ClientAI;

        // draw the detectopm range
        Handles.color = ai.ProximityDetectionColour;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.ProximityDetectionRange);

        // work out the start point of the vision cone
        Vector3 startPoint = Mathf.Cos(-ai.VisionConeAngle * Mathf.Deg2Rad) * -ai.transform.right +
                             Mathf.Sin(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.forward;

        // draw the vision cone
        Handles.color = ai.VisionConeColour;
        Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.VisionConeAngle * 2f, ai.VisionConeRange);        

    }
}
#endif // UNITY_EDITOR