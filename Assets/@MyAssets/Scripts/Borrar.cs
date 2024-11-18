using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borrar : MonoBehaviour
{
    public GameObject target;
    public GameObject attachPoint;
    public GameObject[] children;
    public GameObject[] clothes;
    public GameObject[] clothesChild;
    public GameObject cuttedClothes;
    public Collider[] boundsColliders;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Bounds[] ConvertCollidersToBounds()
    {
        // Validar si la lista de colliders es nula o vacía
        if (boundsColliders == null || boundsColliders.Length == 0)
        {
            Debug.LogWarning("No hay colliders en la lista para convertir.");
            return new Bounds[0]; // Retornar un array vacío si no hay colliders
        }

        // Crear una lista para almacenar los Bounds
        List<Bounds> boundsList = new List<Bounds>();

        // Iterar sobre los colliders y extraer los Bounds
        foreach (var collider in boundsColliders)
        {
            if (collider != null)
            {
                boundsList.Add(collider.bounds); // Añadir el Bounds del collider
            }
            else
            {
                Debug.LogWarning("Se encontró un collider nulo en la lista.");
            }
        }

        // Convertir la lista de Bounds a un array y retornarla
        return boundsList.ToArray();
    }

}
