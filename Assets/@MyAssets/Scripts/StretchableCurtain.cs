using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StretchableCurtain : MonoBehaviour
{
    public Transform fixedAnchor; // Punto fijo en el borde derecho
    private XRGrabInteractable grabInteractable; // Componente de agarre
    private Transform grabbingHand; // Referencia a la mano que está agarrando

    private Vector3 initialScale; // Escala inicial de la cortina
    private float initialDistance; // Distancia inicial entre la mano y el ancla fija
    public GameObject curtain; // Objeto de la cortina

    private Vector3 initialPosition; // Posición inicial de la cortina
    private float pivotOffset; // Distancia del pivote al borde derecho

    // Límite de escala
    private const float maxScaleX = 1.93f;
    private const float minScaleX = 0.42f;

    public float maxGrabDistance = 1.0f;
    void Start()
    {
        // Obtén el componente XR Grab Interactable
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Suscribirse a los eventos de agarre
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        // Guarda la escala inicial, posición inicial y calcula el offset del pivote
        initialScale = curtain.transform.localScale;
        initialPosition = curtain.transform.position;
        pivotOffset = fixedAnchor.position.x - curtain.transform.position.x;
    }

    void Update()
    {
        // Si está siendo agarrada, ajusta la escala y posición
        if (grabbingHand != null)
        {
            float distanceToHand = Vector3.Distance(grabbingHand.position, transform.position);
            if (distanceToHand > maxGrabDistance)
            {
                // Fuerza el soltar si la distancia excede el límite
                ForceRelease();
                return;
            }
            // Calcula la distancia actual y la dirección del estiramiento
            Vector3 direction = grabbingHand.position - fixedAnchor.position;
            float currentDistance = direction.magnitude;
            float stretchFactor = currentDistance / initialDistance;

            // Ajusta el signo del stretchFactor según la dirección
            stretchFactor *= Mathf.Sign(direction.x);

            // Ajusta la escala en el eje X con límites
            Vector3 newScale = initialScale;
            newScale.x = Mathf.Clamp(initialScale.x * stretchFactor, minScaleX, maxScaleX);
            curtain.transform.localScale = newScale;

            // Compensa la posición para mantener el borde derecho fijo
            Vector3 newPosition = curtain.transform.position;
            newPosition.x = fixedAnchor.position.x - (pivotOffset * (newScale.x / initialScale.x));
            curtain.transform.position = newPosition;
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Guarda la mano que está agarrando
        grabbingHand = args.interactorObject.transform;

        // Calcula la distancia inicial
        initialDistance = Vector3.Distance(grabbingHand.position, fixedAnchor.position);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        initialScale = curtain.transform.localScale;
        initialPosition = curtain.transform.position;
        // Restablece la referencia a la mano
        grabbingHand = null;
    }
    private void ForceRelease()
    {
        Debug.Log("soltado");
        // Forzar la finalización de la interacción
        if (grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            grabInteractable.interactionManager.SelectExit(interactor, grabInteractable);
        }

        // Limpia la referencia de la mano
        grabbingHand = null;
    }
}
