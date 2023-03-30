using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostCheck : MonoBehaviour
{
    public void canBoost(Collision coll, string tag, Rigidbody rb, float jumpForce)
    {
        if (coll.contacts[0].normal.y == 1f && coll.gameObject.CompareTag(tag))
        {
            Debug.Log("GOT GOOMBAD");


            rb.velocity = new Vector3(rb.velocity.x * 1.4f, 0f, rb.velocity.z);

            rb.AddForce(transform.up * (jumpForce / 1.6f), ForceMode.Impulse);
            Vector3 local = FindObjectOfType<Waypoints>().gameObject.transform.localScale;
            local.y = 1f;
            FindObjectOfType<Waypoints>().gameObject.transform.localScale = local;


            FindObjectOfType<Waypoints>().enabled = false;
            Invoke("reEnable", 2f);
        }
    }

    void reEnable()
    {
        Vector3 local = FindObjectOfType<Waypoints>().gameObject.transform.localScale;
        local.y = 1.343615f;
        FindObjectOfType<Waypoints>().gameObject.transform.localScale = local;


        FindObjectOfType<Waypoints>().enabled = true;

    }

}
