using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionRefusal : MonoBehaviour
{
    void Start()
    {
        FindObjectOfType<MyNetworkManager>().OnFailedJoin.AddListener(() => gameObject.SetActive(true));
        gameObject.SetActive(false);
    }

}
