using UnityEngine;

public class BillboardHealthbar : MonoBehaviour
{
    Camera cam;

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (cam)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}

