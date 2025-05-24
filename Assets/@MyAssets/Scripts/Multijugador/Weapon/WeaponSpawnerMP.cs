using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class WeaponSpawnerMP : NetworkBehaviour
{
    [Header("Weapon Spawning")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<ulong, GameObject> playerWeapons = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            StartCoroutine(SpawnInitialWeapons());
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private IEnumerator SpawnInitialWeapons()
    {
        yield return new WaitForSeconds(1f);

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnWeaponForClient(client.ClientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            StartCoroutine(SpawnWeaponDelayed(clientId));
        }
    }

    private IEnumerator SpawnWeaponDelayed(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);
        SpawnWeaponForClient(clientId);
    }

    private void SpawnWeaponForClient(ulong clientId)
    {
        if (playerWeapons.ContainsKey(clientId))
        {
            return;
        }

        //(0 para server host, 1 para client)
        int spawnIndex = NetworkManager.Singleton.IsServer && clientId == NetworkManager.Singleton.LocalClientId ? 0 : 1;

        if (spawnIndex >= spawnPoints.Length)
        {
            Debug.LogError("No hay suficientes puntos de spawn configurados");
            return;
        }

        GameObject weapon = Instantiate(weaponPrefab, spawnPoints[spawnIndex].position, spawnPoints[spawnIndex].rotation);
        NetworkObject weaponNetObject = weapon.GetComponent<NetworkObject>();
        weaponNetObject.SpawnWithOwnership(clientId);

        playerWeapons[clientId] = weapon;
        StartCoroutine(AssignWeaponToPlayer(weapon, clientId));
    }

    private IEnumerator AssignWeaponToPlayer(GameObject weapon, ulong clientId)
    {
        yield return new WaitForEndOfFrame();

        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        NetworkPlayer targetPlayer = null;

        foreach (var player in players)
        {
            if (player.OwnerClientId == clientId)
            {
                targetPlayer = player;
                break;
            }
        }

        if (targetPlayer != null)
        {
            NetworkObject weaponNetObj = weapon.GetComponent<NetworkObject>();
            AssignWeaponToPlayerServerRpc(weaponNetObj.NetworkObjectId, clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignWeaponToPlayerServerRpc(ulong weaponNetworkId, ulong playerId)
    {
        AssignWeaponToPlayerClientRpc(weaponNetworkId, playerId);
    }

    [ClientRpc]
    private void AssignWeaponToPlayerClientRpc(ulong weaponNetworkId, ulong playerId)
    {
        StartCoroutine(AssignWeaponCoroutine(weaponNetworkId, playerId));
    }

    private IEnumerator AssignWeaponCoroutine(ulong weaponNetworkId, ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponNetworkId, out NetworkObject weaponObject))
        {
            yield break;
        }

        NetworkPlayer[] players = FindObjectsOfType<NetworkPlayer>();
        PlayerHandControllerMP handController = null;

        foreach (var player in players)
        {
            if (player.OwnerClientId == playerId)
            {
                handController = player.GetComponent<PlayerHandControllerMP>();
                break;
            }
        }

        if (handController != null && handController.IsOwner)
        {
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ForceGrabWeapon(weaponObject.gameObject, handController.rightHandInteractor));
        }
    }

    private IEnumerator ForceGrabWeapon(GameObject weapon, XRDirectInteractor hand)
    {
        XRGrabInteractable weaponInteractable = weapon.GetComponent<XRGrabInteractable>();

        if (weaponInteractable == null)
        {
            Debug.LogError("El arma no tiene XRGrabInteractable");
            yield break;
        }

        weapon.transform.position = hand.transform.position;
        weapon.transform.rotation = hand.transform.rotation;

        yield return new WaitForEndOfFrame();

        if (hand.interactionManager != null)
        {
            hand.interactionManager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)weaponInteractable);
        }
    }
}