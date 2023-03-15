using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField]
    public GameObject[] waypoints;
    public bool afterPlayer = false;
    public int currentTarget = 1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!afterPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentTarget].transform.position, 3 * Time.deltaTime);
        }
        if (afterPlayer)
        {
            GameObject player = GameObject.FindWithTag("Player");
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 3 * Time.deltaTime);

        }
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
