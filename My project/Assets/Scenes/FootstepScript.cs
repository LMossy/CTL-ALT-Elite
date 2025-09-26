using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepScript : MonoBehaviour
{
    public GameObject footstep;       // walking footsteps (looped AudioSource GameObject)
    public GameObject sprint;         // sprint footsteps (looped AudioSource GameObject)
    public AudioSource jump;          // jump sound (single AudioSource with clip)

    void Start()
    {
        footstep.SetActive(false);
        sprint.SetActive(false);
    }

    void Update()
    {
        // --- Walking ---
        if ((Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d")) 
            && !Input.GetKey(KeyCode.LeftShift))
        {
            footsteps();
        }
        else if (!Input.GetKey("w") && !Input.GetKey("a") && !Input.GetKey("s") && !Input.GetKey("d"))
        {
            StopFootsteps();
        }

        // --- Sprinting ---
        if ((Input.GetKey("w") || Input.GetKey("a") || Input.GetKey("s") || Input.GetKey("d")) 
            && Input.GetKey(KeyCode.LeftShift))
        {
            sprint.SetActive(true);
            footstep.SetActive(false);
        }
        else
        {
            sprint.SetActive(false);
        }

        // --- Jumping ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump.PlayOneShot(jump.clip);
        }
    }

    void footsteps()
    {
        footstep.SetActive(true);
        sprint.SetActive(false);
    }

    void StopFootsteps()
    {
        footstep.SetActive(false);
        sprint.SetActive(false);
    }
}
