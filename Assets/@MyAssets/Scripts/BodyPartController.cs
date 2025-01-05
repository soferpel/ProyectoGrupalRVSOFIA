using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartController : MonoBehaviour
{

    public float decayTime; 
    public List<Material> materials; 
    public bool isDecayed = false;
    public float decayTimer = 0f;

    void Start()
    {
        decayTimer = decayTime;
        
    }

    void Update()
    {
        if (!isDecayed)
        {
            decayTimer -= Time.deltaTime;

            if (decayTimer <= 0)
            {
                DecayBodyPart();
            }
        }
    }

    void DecayBodyPart()
    {
        isDecayed = true;
        if (materials != null)
        {
            foreach(Material material in materials)
            {
                if (material != null)
                {
                    material.color = new Color(184f / 255f, 203f / 255f, 184f / 255f);
                }
            }
        }
        gameObject.tag = "Podrido";
        Debug.Log(name + " se ha podrido.");
    }
}
