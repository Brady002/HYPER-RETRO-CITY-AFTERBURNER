using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChase : MonoBehaviour
{
    public bool afterPlayer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider coll)
    {
        if (!coll.CompareTag("Player")) return;
        afterPlayer = true;
    }

    void OnTriggerExit(Collider coll)
    {
        if (!coll.CompareTag("Player")) return;
        afterPlayer = false;
    }
}
