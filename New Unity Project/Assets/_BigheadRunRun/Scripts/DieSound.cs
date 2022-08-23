using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieSound : MonoBehaviour
{

    public AudioClip PlayerDieSound; //The sound that will play when the player dies and game is lost.
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter(Collider target)
    {
        if (target.tag == "Player")
        {
            GetComponent<AudioSource>().PlayOneShot(PlayerDieSound, 2.7F);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
