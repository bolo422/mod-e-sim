using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minimapScript : MonoBehaviour
{
    private Camera camera;
    void Start()
    {
        camera = this.GetComponent<Camera>();
    }

    void Update()
    {
        camera.orthographicSize += Input.mouseScrollDelta.y * 2 * -1;
    }
}
