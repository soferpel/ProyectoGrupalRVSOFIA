using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashPart : MonoBehaviour
{
    public GameObject part;

    private void OnDestroy()
    {
        Destroy(part);
    }
}
