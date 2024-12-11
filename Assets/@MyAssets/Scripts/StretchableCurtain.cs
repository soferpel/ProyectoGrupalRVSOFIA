using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StretchableCurtain : MonoBehaviour
{
    public Transform fixedAnchor; 
    private XRGrabInteractable grabInteractable;
    private Transform grabbingHand; 

    private Vector3 initialScale; 
    private float initialDistance; 
    public GameObject curtain; 

    private Vector3 initialPosition; 
    private float pivotOffset; 


    private const float maxScaleX = 1.93f;
    private const float minScaleX = 0.42f;

    public float maxGrabDistance = 1.0f;
    void Start()
    {

        grabInteractable = GetComponent<XRGrabInteractable>();


        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);


        initialScale = curtain.transform.localScale;
        initialPosition = curtain.transform.position;
        pivotOffset = fixedAnchor.position.x - curtain.transform.position.x;
    }

    void Update()
    {

        if (grabbingHand != null)
        {
            float distanceToHand = Vector3.Distance(grabbingHand.position, transform.position);
            if (distanceToHand > maxGrabDistance)
            {

                ForceRelease();
                return;
            }

            Vector3 direction = grabbingHand.position - fixedAnchor.position;
            float currentDistance = direction.magnitude;
            float stretchFactor = currentDistance / initialDistance;

 
            stretchFactor *= Mathf.Sign(direction.x);


            Vector3 newScale = initialScale;
            newScale.x = Mathf.Clamp(initialScale.x * stretchFactor, minScaleX, maxScaleX);
            curtain.transform.localScale = newScale;


            Vector3 newPosition = curtain.transform.position;
            newPosition.x = fixedAnchor.position.x - (pivotOffset * (newScale.x / initialScale.x));
            curtain.transform.position = newPosition;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {

        grabbingHand = args.interactorObject.transform;


        initialDistance = Vector3.Distance(grabbingHand.position, fixedAnchor.position);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        initialScale = curtain.transform.localScale;
        initialPosition = curtain.transform.position;

        grabbingHand = null;
    }
    private void ForceRelease()
    {
        Debug.Log("soltado");

        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
        }


        grabbingHand = null;
    }
}
