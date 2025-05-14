using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CustomizePlayerMP : NetworkBehaviour
{
    public Image hairImage;
    public Image personImage;
    
    public Sprite[] skinSprites;
    public Sprite[] hairStyles;

    private int skinIndex = 0;
    private int hairStyleIndex = 0;
    private int hairColorIndex = 0;

    public Button startButton;

    private void Start()
    {
        UpdateCharacter();
    }

    public void NextSkin() { 
        skinIndex = (skinIndex + 1) % skinSprites.Length; UpdateCharacter(); 
    }
    public void PrevSkin() { 
        skinIndex = (skinIndex - 1 + skinSprites.Length) % skinSprites.Length; UpdateCharacter(); 
    }
    public void NextHairStyle() { 
        hairStyleIndex = (hairStyleIndex + 1) % (hairStyles.Length/3); UpdateCharacter(); 
    }
    public void PrevHairStyle() { 
        hairStyleIndex = (hairStyleIndex - 1 + (hairStyles.Length/3)) % (hairStyles.Length/3); UpdateCharacter(); 
    }

    public void NextHairColor() { 
        hairColorIndex = (hairColorIndex + 1) % 3; UpdateCharacter();
    }
    public void PrevHairColor() { 
        hairColorIndex = (hairColorIndex - 1 + 3) % 3; UpdateCharacter();
    }

    private void UpdateCharacter()
    {
        personImage.sprite = skinSprites[skinIndex];
        hairImage.sprite = hairStyles[hairStyleIndex*3+hairColorIndex];
        SelectPlayer();
    }

    public void SelectPlayer()
    {
        FullGameManager.Instance.SelectPlayer(skinIndex, hairStyleIndex, hairColorIndex);
    }

    public void GoToGame()
    {
        FullGameManager.Instance.GoToGame();
    }

    public void EnableStartButton()
    {
        startButton.interactable = true;
    }
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer)
        {
            SelectPlayer();
        }
    }
}
