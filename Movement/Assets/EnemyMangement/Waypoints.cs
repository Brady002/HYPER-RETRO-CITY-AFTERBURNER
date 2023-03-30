using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [SerializeField]
    public GameObject[] waypoints;

    public int currentTarget = 1;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<PlayerChase>() != null)
        {

            if (GetComponent<PlayerChase>().afterPlayer)
            {
                GameObject player = GameObject.FindWithTag("Player");
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 3 * Time.deltaTime);

            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentTarget].transform.position, 3 * Time.deltaTime);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentTarget].transform.position, 3 * Time.deltaTime);
        }

    }

}
