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


    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => -transform.forward;

    public float VisionConeAngle => _VisionConeAngle;
    public float VisionConeRange => _VisionConeRange;
    public Color VisionConeColour => _VisionConeColour;

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
        Debug.Log("VEO 2: report can see client ai");
        Awareness.ReportCanSee(seen);
    }

    
    public void OnDetected(GameObject target)
    {
        ClientController clientController = GetComponent<ClientController>();
        if (clientController == null)
        {
            ClientControllerMP clientControllerMP = GetComponent<ClientControllerMP>();
            if (clientControllerMP != null)
            {
                Debug.Log("VEO 6: clientcontrollermp");
                if (!clientControllerMP.isAlive)
                {
                    Debug.Log("VEO 7: is dead " + target.name);
                    clientControllerMP.ReportDeath();
                }
                if (target.gameObject.layer == LayerMask.NameToLayer("BodyParts"))
                {
                    Debug.Log("VEO 7: is  bodypart" + target.name);
                    clientControllerMP.ReportDeath();
                }
                if (target.CompareTag("Player"))
                {
                    Debug.Log("VEO 8: PLAYER");
                    if (target.TryGetComponent<PlayerHandController>(out PlayerHandController player))
                    {
                        Debug.Log("VEO 8: PLAYER IF 1");

                        if (player.HasBodyPart())
                        {
                            Debug.Log("VEO 8: PLAYER IF 2");

                            clientControllerMP.ReportDeath();
                        }
                    }
                }
            }
        }
        if (clientController != null)
        {
            if (!clientController.isAlive)
            {
                clientController.ReportDeath();
            } 
            if(target.gameObject.layer == LayerMask.NameToLayer("BodyParts"))
            {
                clientController.ReportDeath();
            }if(target.CompareTag("Player"))
            {
                if(target.TryGetComponent<PlayerHandController>(out PlayerHandController player))
                {

                    if (player.HasBodyPart()) {
                        clientController.ReportDeath();
                    }
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ClientAI))]
public class ClientAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as ClientAI;

        Vector3 startPoint = Mathf.Cos(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.right +
                             Mathf.Sin(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.forward;

        Handles.color = ai.VisionConeColour;
        Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.VisionConeAngle * 2f, ai.VisionConeRange);        

    }
}
#endif // UNITY_EDITOR