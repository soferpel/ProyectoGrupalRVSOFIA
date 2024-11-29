using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DragPerson : MonoBehaviour
{
    private Transform interactorTransform;  // Transform del interactor (mano del jugador)
    private Rigidbody rb;
    public float dragForce = 10f;           // Fuerza que mueve al personaje
    public float damping = 5f;              // Amortiguación para evitar oscilaciones
    public float followDistance = 0.5f;    // Distancia entre la mano y la persona
    private Vector3 offset;                 // Distancia entre la mano y la persona
    public int countBodyParts = 5;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.isKinematic = false;  // Asegúrate de que no sea Kinematic
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        interactorTransform = args.interactorObject.transform;
        offset = transform.position - interactorTransform.position;  // Calcula la distancia inicial entre la mano y la persona
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        interactorTransform = null;  // Se acaba la interacción
    }

    void FixedUpdate()
    {
        if (interactorTransform != null)
        {
            // Calcula la posición de la persona en base a la mano
            Vector3 targetPosition = interactorTransform.position + offset;

            // Aplica el movimiento hacia la posición objetivo (suavizado para evitar movimientos bruscos)
            Vector3 force = (targetPosition - transform.position) * dragForce;

            // Aplica amortiguación para evitar oscilaciones
            force -= rb.velocity * damping;

            // Aplica la fuerza al Rigidbody
            rb.AddForce(force, ForceMode.Force);

            // Si el movimiento en el eje Y no es necesario, ajusta para mantener la misma altura
            targetPosition.y = transform.position.y;
            transform.position = targetPosition;
        }
    }
}
