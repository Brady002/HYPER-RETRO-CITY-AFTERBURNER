using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointCommunication : MonoBehaviour
{
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
        if (!coll.CompareTag("Enemy")) return;
        if (coll.GetComponent<PlayerChase>() != null)
        {
            if (coll.GetComponent<PlayerChase>().afterPlayer) return;


        }
        if (int.Parse(gameObject.name.Substring(gameObject.name.Length - 1)) == coll.GetComponent<Waypoints>().waypoints.Length)
        {
            coll.GetComponent<Waypoints>().currentTarget = 0;
        }
        else
        {
            coll.GetComponent<Waypoints>().currentTarget = int.Parse(gameObject.name.Substring(gameObject.name.Length - 1));
        }




    }
}
