using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClothingMP : NetworkBehaviour
{
    public GameObject skin_head;
    public GameObject[] skin_body;

    public GameObject cigarette;
    public GameObject crowbar;
    public GameObject fireaxe;
    public GameObject glock;
    public GameObject phone;

    public GameObject beard_a;
    public GameObject beard_b;
    public GameObject beard_c;
    public GameObject beard_d;

    public GameObject hair_a;
    public GameObject hair_b;
    public GameObject hair_c;
    public GameObject hair_d;
    public GameObject hair_e;

    public GameObject cap;
    public GameObject cap2;
    public GameObject cap3;
    public GameObject chain1;
    public GameObject chain2;
    public GameObject chain3;

    public GameObject banker_suit;

    public GameObject cock_suit;
    public GameObject cock_suit_hat;

    public GameObject farmer_suit;
    public GameObject farmer_suit_hat;

    public GameObject fireman_suit;
    public GameObject fireman_suit_hat;

    public GameObject mechanic_suit;
    public GameObject mechanic_suit_hat;

    public GameObject nurse_suit;

    public GameObject security_guard_suit;
    public GameObject security_guard_suit_hat;

    public GameObject seller_suit;

    public GameObject worker_suit;
    public GameObject worker_suit_hat;

    public GameObject glasses;
    public GameObject jacket;
    public GameObject pullover;
    public GameObject scarf;
    public GameObject shirt;

    public GameObject shoes1;
    public GameObject shoes2;
    public GameObject shoes3;

    public GameObject shortpants;
    public GameObject t_shirt;
    public GameObject tank_top;
    public GameObject trousers;

    public Texture[] skin_textures;

    public Texture[] beard_textures;

    public Texture[] hair_a_textures;
    public Texture[] hair_b_textures;
    public Texture[] hair_c_textures;
    public Texture[] hair_d_textures;
    public Texture[] hair_e_textures;

    public Texture[] cap_textures;
    public Texture[] cap2_textures;
    public Texture[] cap3_textures;
    public Texture[] chain1_textures;
    public Texture[] chain2_textures;
    public Texture[] chain3_textures;

    public Texture[] banker_suit_texture;

    public Texture cock_suit_texture;


    public Texture farmer_suit_texture;


    public Texture fireman_suit_texture;


    public Texture mechanic_suit_texture;


    public Texture nurse_suit_texture;

    public Texture security_guard_suit_texture;

    public Texture seller_suit_texture;

    public Texture worker_suit_texture;


    public Texture[] glasses_texture;
    public Texture[] jacket_textures;
    public Texture[] pullover_textures;
    public Texture[] scarf_textures;
    public Texture[] shirt_textures;

    public Texture[] shoes1_textures;
    public Texture[] shoes2_textures;
    public Texture[] shoes3_textures;

    public Texture[] shortpants_textures;
    public Texture[] t_shirt_textures;
    public Texture[] tank_top_textures;
    public Texture[] trousers_textures;

    public bool show_run;

    bool hat;

    public static List<string> existingClientDescriptions = new List<string>();
    private int male_female;
    private MafiaControllerMP mafiaController;
    private ClientControllerMP clientController;
    public delegate void AppearanceGenerated(string description);
    public static event AppearanceGenerated OnMafiaAppearanceGenerated;
    public static event AppearanceGenerated OnClientAppearanceGenerated;

    Coroutine coroutine_random_clothing;

    public List<GameObject> characterParts = new List<GameObject>();


    [ClientRpc]
    void SetPartActiveClientRpc(int partIndex, bool isActive, MaterialData matData, string textureName)
    {
        if (partIndex >= 0 && partIndex < characterParts.Count)
        {
            characterParts[partIndex].SetActive(isActive);
            if (isActive)
            {
                Material mat = matData.ToMaterial();
                var renderer = characterParts[partIndex].GetComponent<Renderer>();
                renderer.material = mat;
                renderer.material.mainTexture.name = textureName;
            }
        }
    }

    void start_random_clothing()
    {
        hat = true;

        hair_a.SetActive(false);
        hair_b.SetActive(false);
        hair_c.SetActive(false);
        hair_d.SetActive(false);
        hair_e.SetActive(false);

        beard_a.SetActive(false);
        beard_b.SetActive(false);
        beard_c.SetActive(false);
        beard_d.SetActive(false);

        cap.SetActive(false);
        cap2.SetActive(false);
        cap3.SetActive(false);

        chain1.SetActive(false);
        chain2.SetActive(false);
        chain3.SetActive(false);

        banker_suit.SetActive(false);

        cock_suit.SetActive(false);
        cock_suit_hat.SetActive(false);

        farmer_suit.SetActive(false);
        farmer_suit_hat.SetActive(false);

        fireman_suit.SetActive(false);
        fireman_suit_hat.SetActive(false);

        mechanic_suit.SetActive(false);
        mechanic_suit_hat.SetActive(false);

        nurse_suit.SetActive(false);

        security_guard_suit.SetActive(false);
        security_guard_suit_hat.SetActive(false);

        seller_suit.SetActive(false);

        worker_suit.SetActive(false);
        worker_suit_hat.SetActive(false);

        glasses.SetActive(false);

        jacket.SetActive(false);

        pullover.SetActive(false);

        scarf.SetActive(false);

        shirt.SetActive(false);

        shoes1.SetActive(false);

        shoes2.SetActive(false);

        shoes3.SetActive(false);

        shortpants.SetActive(false);

        t_shirt.SetActive(false);

        tank_top.SetActive(false);

        trousers.SetActive(false);

        int skin_color = UnityEngine.Random.Range(0, 6);

        skin_head.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];
        foreach (GameObject skin_body_part in skin_body)
        {
            skin_body_part.GetComponent<Renderer>().materials[0].mainTexture = skin_textures[skin_color];
        }

        male_female = UnityEngine.Random.Range(0, 2);

        int hairColor = UnityEngine.Random.Range(0, 4);    // 0 = dark  1 = brown  2 = blonde

        if (male_female == 0)
        {

            hat = true;
            int hair = UnityEngine.Random.Range(0, 3);
            if (hair == 0)
            {
                hair_a.SetActive(true);
                int hair_cut = UnityEngine.Random.Range(0, 2);
                hat = true;
                if (hairColor == 0)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[0];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[1];
                    }
                }
                if (hairColor == 1)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[2];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[3];
                    }

                }
                if (hairColor == 2)
                {
                    if (hair_cut == 0)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[4];
                    }
                    if (hair_cut == 1)
                    {
                        hair_a.GetComponent<Renderer>().materials[0].mainTexture = hair_a_textures[5];
                    }
                }
            }

            if (hair == 1)
            {
                hair_b.SetActive(true);
                hat = false;
                int hair_cut = UnityEngine.Random.Range(0, 2);
                if (hairColor == 0)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[0];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[5];
                    }
                }
                if (hairColor == 1)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[1];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[3];
                    }

                }
                if (hairColor == 2)
                {
                    if (hair_cut == 0)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[2];
                    }
                    if (hair_cut == 1)
                    {
                        hair_b.GetComponent<Renderer>().materials[0].mainTexture = hair_b_textures[4];
                    }
                }
            }

            if (hair == 2)
            {
                hair_e.SetActive(true);
                hat = false;
                if (hairColor == 0)
                {
                    hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[0];
                }
                if (hairColor == 1)
                {
                    hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[1];
                }
                if (hairColor == 2)
                {
                    hair_e.GetComponent<Renderer>().materials[0].mainTexture = hair_e_textures[2];
                }
            }
        }
        if (male_female == 1)
        {
            hat = false;
            int hair = UnityEngine.Random.Range(0, 2);

            if (hair == 0)
            {
                hat = false;
                hair_c.SetActive(true);

                if (hairColor == 0)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[0];
                }
                if (hairColor == 1)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[1];
                }
                if (hairColor == 2)
                {
                    hair_c.GetComponent<Renderer>().materials[0].mainTexture = hair_c_textures[2];
                }
            }
            if (hair == 1)
            {
                hat = false;
                hair_d.SetActive(true);

                if (hairColor == 0)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[0];
                }
                if (hairColor == 1)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[1];
                }
                if (hairColor == 2)
                {
                    hair_d.GetComponent<Renderer>().materials[0].mainTexture = hair_d_textures[2];
                }
            }
        }

        if (male_female == 0)
        {
            int percent = UnityEngine.Random.Range(0, 100);
            if (percent > 0 && percent < 50)
            {
            }
            if (percent > 50 && percent < 70)
            {
                beard_a.SetActive(true);
                if (hairColor == 0)
                {
                    beard_a.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[0];
                }
                if (hairColor == 1)
                {
                    beard_a.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[1];
                }
                if (hairColor == 2)
                {
                    beard_a.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[2];
                }
            }
            if (percent > 70 && percent < 80)
            {
                beard_b.SetActive(true);
                if (hairColor == 0)
                {
                    beard_b.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[0];
                }
                if (hairColor == 1)
                {
                    beard_b.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[1];
                }
                if (hairColor == 2)
                {
                    beard_b.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[2];
                }
            }
            if (percent > 80 && percent < 90)
            {
                beard_c.SetActive(true);
                if (hairColor == 0)
                {
                    beard_c.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[0];
                }
                if (hairColor == 1)
                {
                    beard_c.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[1];
                }
                if (hairColor == 2)
                {
                    beard_c.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[2];
                }
            }
            if (percent > 90 && percent < 100)
            {
                beard_d.SetActive(true);
                if (hairColor == 0)
                {
                    beard_d.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[0];
                }
                if (hairColor == 1)
                {
                    beard_d.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[1];
                }
                if (hairColor == 2)
                {
                    beard_d.GetComponent<Renderer>().materials[0].mainTexture = beard_textures[2];
                }
            }
        }

        int suit_or_cloth = UnityEngine.Random.Range(0, 2);
        if (suit_or_cloth == 0)
        {
            int which_suit = UnityEngine.Random.Range(0, 9);

            if (which_suit == 0)
            {
                banker_suit.SetActive(true);
                int which_texture = UnityEngine.Random.Range(0, 7);
                banker_suit.GetComponent<Renderer>().materials[0].mainTexture = banker_suit_texture[which_texture];
            }
            if (which_suit == 1)
            {
                cock_suit.SetActive(true);
                cock_suit.GetComponent<Renderer>().materials[0].mainTexture = cock_suit_texture;
                if (hat)
                {
                    cock_suit_hat.SetActive(true);
                    cock_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = cock_suit_texture;
                }
            }
            if (which_suit == 2)
            {
                farmer_suit.SetActive(true);
                farmer_suit.GetComponent<Renderer>().materials[0].mainTexture = farmer_suit_texture;
                if (hat)
                {
                    farmer_suit_hat.SetActive(true);
                    farmer_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = farmer_suit_texture;
                }
            }
            if (which_suit == 3)
            {
                fireman_suit.SetActive(true);

                fireman_suit.GetComponent<Renderer>().materials[0].mainTexture = fireman_suit_texture;

                if (hat)
                {
                    fireman_suit_hat.SetActive(true);

                    fireman_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = fireman_suit_texture;
                }
            }
            if (which_suit == 4)
            {
                mechanic_suit.SetActive(true);

                mechanic_suit.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;

                if (hat)
                {
                    mechanic_suit_hat.SetActive(true);

                    mechanic_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = mechanic_suit_texture;
                }
            }
            if (which_suit == 5)
            {
                nurse_suit.SetActive(true);

                nurse_suit.GetComponent<Renderer>().materials[0].mainTexture = nurse_suit_texture;


            }
            if (which_suit == 6)
            {
                security_guard_suit.SetActive(true);

                security_guard_suit.GetComponent<Renderer>().materials[0].mainTexture = security_guard_suit_texture;

                if (hat)
                {
                    security_guard_suit_hat.SetActive(true);

                    security_guard_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = security_guard_suit_texture;
                }
            }
            if (which_suit == 7)
            {
                seller_suit.SetActive(true);

                seller_suit.GetComponent<Renderer>().materials[0].mainTexture = seller_suit_texture;
            }
            if (which_suit == 8)
            {
                worker_suit.SetActive(true);

                worker_suit.GetComponent<Renderer>().materials[0].mainTexture = worker_suit_texture;

                if (hat)
                {
                    worker_suit_hat.SetActive(true);

                    worker_suit_hat.GetComponent<Renderer>().materials[0].mainTexture = worker_suit_texture;
                }
            }
        }
        if (suit_or_cloth == 1)
        {
            int shoes = UnityEngine.Random.Range(0, 3);
            if (shoes == 0)
            {
                shoes1.SetActive(true);
                int shoes1_texture = UnityEngine.Random.Range(0, 8);
                shoes1.GetComponent<Renderer>().materials[0].mainTexture = shoes1_textures[shoes1_texture];
            }

            if (shoes == 1)
            {
                shoes2.SetActive(true);
                int shoes2_texture = UnityEngine.Random.Range(0, 7);
                shoes2.GetComponent<Renderer>().materials[0].mainTexture = shoes2_textures[shoes2_texture];
            }

            if (shoes == 2)
            {
                shoes3.SetActive(true);
                int shoes3_texture = UnityEngine.Random.Range(0, 6);
                shoes3.GetComponent<Renderer>().materials[0].mainTexture = shoes3_textures[shoes3_texture];
            }

            int glasses_percentage = UnityEngine.Random.Range(0, 100);
            if (glasses_percentage < 20)
            {
                glasses.SetActive(true);
                int texture_choose = UnityEngine.Random.Range(0, 6);
                glasses.GetComponent<Renderer>().materials[0].mainTexture = glasses_texture[texture_choose];
            }

            int chain = UnityEngine.Random.Range(0, 3);

            if (chain == 0)
            {
                chain1.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 4);
                chain1.GetComponent<Renderer>().materials[0].mainTexture = chain1_textures[textures];
            }
            if (chain == 1)
            {
                chain2.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 3);
                chain2.GetComponent<Renderer>().materials[0].mainTexture = chain2_textures[textures];

            }
            if (chain == 2)
            {
                chain3.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 3);
                chain3.GetComponent<Renderer>().materials[0].mainTexture = chain3_textures[textures];
            }

            int scarfPercentage = UnityEngine.Random.Range(0, 100);

            if (scarfPercentage < 20)
            {
                scarf.SetActive(true);
                int textures = UnityEngine.Random.Range(0, 11);
                scarf.GetComponent<Renderer>().materials[0].mainTexture = scarf_textures[textures];
            }

            int which_trouser = UnityEngine.Random.Range(0, 2);

            if (which_trouser == 0)
            {
                trousers.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 15);
                trousers.GetComponent<Renderer>().materials[0].mainTexture = trousers_textures[texture];

            }
            if (which_trouser == 1)
            {
                shortpants.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 11);
                shortpants.GetComponent<Renderer>().materials[0].mainTexture = shortpants_textures[texture];

            }

            int upper_cloth = UnityEngine.Random.Range(0, 4);

            if (upper_cloth == 0)
            {
                pullover.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 17);
                pullover.GetComponent<Renderer>().materials[0].mainTexture = pullover_textures[texture];
            }

            if (upper_cloth == 1)
            {
                shirt.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 14);
                shirt.GetComponent<Renderer>().materials[0].mainTexture = shirt_textures[texture];
            }
            if (upper_cloth == 2)
            {
                t_shirt.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 21);
                t_shirt.GetComponent<Renderer>().materials[0].mainTexture = t_shirt_textures[texture];
            }
            if (upper_cloth == 3)
            {
                tank_top.SetActive(true);
                int texture = UnityEngine.Random.Range(0, 11);
                tank_top.GetComponent<Renderer>().materials[0].mainTexture = tank_top_textures[texture];
            }

        }

        for (int i = 0; i < characterParts.Count; i++)
        {
            MaterialData matData = MaterialData.FromMaterial(characterParts[i].GetComponent<Renderer>().sharedMaterial);
            if (characterParts[i].activeSelf)
            {
                string textureName = characterParts[i].GetComponent<Renderer>().sharedMaterial.mainTexture.name;
                SetPartActiveClientRpc(i, true, matData, textureName);
            }
            else
            {
                string textureName = characterParts[i].GetComponent<Renderer>().sharedMaterial.mainTexture.name;
                SetPartActiveClientRpc(i, false, matData, textureName);
            }
        }

        string clientDescription = GenerateAppearanceDescription();
        clientController.appearanceDescription = clientDescription;
        OnClientAppearanceGenerated?.Invoke(clientDescription);
        clientController.isFemale = (male_female == 1) ? true : false;
        existingClientDescriptions.Add(clientDescription);
    }

    private void EnsureUniqueAppearanceForMafia()
    {
        string description;
        do
        {
            start_random_clothing();
            description = GenerateAppearanceDescription();
        }
        while (existingClientDescriptions.Contains(description));

        mafiaController = GetComponent<MafiaControllerMP>();
        if (mafiaController != null)
        {
            mafiaController.AppearanceDescription = description;
            existingClientDescriptions.Add(description);
            OnMafiaAppearanceGenerated?.Invoke(description);
        }

    }
    public override void OnNetworkSpawn()
    {
        characterParts = new List<GameObject>()
        {
            cigarette, crowbar, fireaxe, glock, phone,
            beard_a, beard_b, beard_c, beard_d,
            hair_a, hair_b, hair_c, hair_d, hair_e,
            cap, cap2, cap3, chain1, chain2, chain3,
            banker_suit,
            cock_suit, cock_suit_hat,
            farmer_suit, farmer_suit_hat,
            fireman_suit, fireman_suit_hat,
            mechanic_suit, mechanic_suit_hat,
            nurse_suit,
            security_guard_suit, security_guard_suit_hat,
            seller_suit,
            worker_suit, worker_suit_hat,
            glasses, jacket, pullover, scarf, shirt,
            shoes1, shoes2, shoes3,
            shortpants, t_shirt, tank_top, trousers,skin_head
        };
        foreach(GameObject body in skin_body)
        {
            characterParts.Add(body);
        }
        if (IsServer)
        {

            if (GetComponent<MafiaControllerMP>() != null)
            {
                EnsureUniqueAppearanceForMafia();
            }
            else
            {
                clientController = GetComponent<ClientControllerMP>();
                start_random_clothing();
            }
        }
    }

    public string GenerateAppearanceDescription()
    {
        string description = "";

        GameObject[] clothes = {
        hair_a, hair_b, hair_c, hair_d, hair_e,
        beard_a, beard_b, beard_c, beard_d,
        cap, cap2, cap3,
        chain1, chain2, chain3,
        banker_suit,
        cock_suit, cock_suit_hat,
        farmer_suit, farmer_suit_hat,
        fireman_suit, fireman_suit_hat,
        mechanic_suit, mechanic_suit_hat,
        nurse_suit,
        security_guard_suit, security_guard_suit_hat,
        seller_suit,
        worker_suit, worker_suit_hat,
        glasses,
        jacket,
        pullover,
        scarf,
        shirt,
        shoes1, shoes2, shoes3,
        shortpants,
        t_shirt,
        tank_top,
        trousers
    };
        description += (male_female == 0) ? "Genero: Hombre \n" : "Genero: Mujer \n ";
        if (skin_head.GetComponent<Renderer>() != null && skin_head.GetComponent<Renderer>().materials[0].mainTexture != null)
        {
            description += $"Color de piel y ojos: {skin_head.GetComponent<Renderer>().materials[0].mainTexture.name} \n ";
        }

        foreach (GameObject item in clothes)
        {
            if (item.activeSelf)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                string textureName = (renderer != null && renderer.material.mainTexture != null)
                    ? renderer.material.mainTexture.name
                    : "No Texture";
                Debug.Log("Textura: "+textureName);
                if (item.name == "traje de banquero")
                {
                    description += $"{item.name} {textureName} \n ";
                }
                else if (item.name.Contains("traje"))
                {
                    description += $"{item.name} \n ";
                }
                else
                {
                    description += $"{item.name} {textureName} \n ";
                }
            }
        }

        return description.TrimEnd(' ', '\n');
    }

}
