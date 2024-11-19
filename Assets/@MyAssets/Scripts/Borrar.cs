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
        if (boundsColliders == null || boundsColliders.Length == 0)
        {
            Debug.LogWarning("No hay colliders en la lista para convertir.");
            return new Bounds[0]; 
        }

        List<Bounds> boundsList = new List<Bounds>();

        foreach (var collider in boundsColliders)
        {
            if (collider != null)
            {
                boundsList.Add(collider.bounds);
            }
            else
            {
                Debug.LogWarning("Se encontró un collider nulo en la lista.");
            }
        }

        return boundsList.ToArray();
    }

}
