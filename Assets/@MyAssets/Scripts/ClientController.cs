using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
public class ClientController : PersonController
{
    public float distanceThreshold = 1f;

    public bool hasBeenAttacked = false;

    public Rigidbody[] deadRigidbodies;
    public Collider[] deadColliders;
    public int countBodyParts = 5;
    public GameObject body;
    public GameObject[] sliceableParts;
    public Vector3 reportDirection;
    public Joint[] joints;
    public bool isFemale;
    public GameObject[] clothesMaterials;
    public GameObject skinMaterial;

    protected override void Start()
    {
        Debug.Log("Female: " + isFemale);
        buyProbability = 0.5f;
        foreach (Rigidbody rg in deadRigidbodies)
        {
            rg.isKinematic = true;
        }
        foreach (Collider collider in deadColliders)
        {
            collider.enabled = false;
        }
        base.Start();
    }

    public void ReportDeath()
    {
        if (!isAlive || isReported) return;

        personDirection = -1;
        isFinalMove = true;
        isReported = true;
        StopAllCoroutines();
        reportDirection = agent.destination;
        agent.speed = 4;
        agent.enabled = false;
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {

        if (inBuyPoint)
        {
            buyPointController.FreePoint();
        }
        else if (inQueue)
        {
            buyPointController.RemoveClientFromQueue(this);
        }
        audioSource[4].Stop();
        if (isFemale)
        {
            audioSource[1].Play();
        }
        else
        {
            audioSource[2].Play();
        }
        animator.SetTrigger("scared");

            Vector3 direction = -(reportDirection - transform.position).normalized * personDirection;
            Debug.Log(reportDirection + "mira q: "+direction);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = targetRotation;
        
        yield return new WaitForSeconds(2f);

        animator.SetTrigger("run");
        agent.enabled = true;
        yield return StartCoroutine(MoveToFinalPoint());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<WeaponController>())
        {
            Debug.Log("Cliente atacado por un cuchillo.");
            audioSource[4].Stop();
            audioSource[3].Play();
            hasBeenAttacked = true;
            StopAllCoroutines();
            StartCoroutine(Die(other));
        }
    }

    protected virtual IEnumerator Die(Collider other)
    {
        if (isAlive)
        {

            slider.SetActive(false);
            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<VisionSensor>());
            isAlive = false;
            if (inBuyPoint)
            {
                Debug.Log("MUERTO SERVICIO");
                buyPointController.FreePoint();
            }else if(inQueue)
            {
                Debug.Log("MUERTO EN COLA");
                buyPointController.RemoveClientFromQueue(this);
            }
            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
            animator.enabled = false;
            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.isKinematic = false;
            }

            foreach (Collider col in deadColliders)
            {
                col.enabled = true;
                col.gameObject.AddComponent<DetectableTarget>();
                col.gameObject.layer = LayerMask.NameToLayer("BodyParts");
            }

            Vector3 forceDirection = -(other.transform.position - transform.position).normalized; 
            float forceStrength = 4f;
            Vector3 force = forceDirection * forceStrength;

            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }
            
            BodyPartController bodypartController = gameObject.AddComponent<BodyPartController>();
            bodypartController.decayTime = 240;
            if (skinMaterial.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer skinrender)) {
                if (bodypartController.materials == null)
                {
                    bodypartController.materials = new List<Material>();
                }
                if (skinrender.material != null)
                {
                    if (!bodypartController.materials.Contains(skinrender.material))
                    {
                        bodypartController.materials.Add(skinrender.material);
                    }
                }
            }
            foreach (GameObject clothesMaterial in clothesMaterials)
            {
                foreach (Transform child in clothesMaterial.transform)
                {
                    if (child.gameObject.activeSelf)
                    {
                        if(child.gameObject.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer render))
                        {
                            if (bodypartController.materials == null)
                            {
                                bodypartController.materials = new List<Material>();
                                Debug.Log("Inicializando la lista materials.");
                            }
                            if (render.material == null)
                            {
                                Debug.LogWarning($"El objeto {child.gameObject.name} tiene un SkinnedMeshRenderer pero no tiene un material asignado.");
                                continue; // Omite este objeto si no tiene un material
                            }
                            if (!bodypartController.materials.Contains(render.material))
                            {
                                Debug.Log($"Objeto: {child.gameObject.name}, Material: {render.material.name}");
                                bodypartController.materials.Add(render.material);
                            }/*
                            Debug.Log("EJEMPLO: " + child.gameObject.name +render.material);
                            bodypartController.materials.Add(render.material);*/
                        }
                    }
                }
            }

            yield return new WaitForSeconds(2f);

            foreach (GameObject part in sliceableParts)
            {
                part.layer = LayerMask.NameToLayer("Sliceable");
            }

        }
    }
}

