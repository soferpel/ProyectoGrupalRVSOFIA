using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientController : MonoBehaviour
{
    private int lives;
    private Animator animator;
    public GameObject[] sliceableParts;
    public LayerMask sliceLayer;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<WeaponController>())
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(1f);
        foreach(GameObject part in sliceableParts)
        {
            part.layer = LayerMask.NameToLayer("Sliceable");
        }
    }
}
