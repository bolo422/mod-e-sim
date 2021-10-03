using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraPlayer : MonoBehaviour
{
    public GameObject player;
    public Camera minimap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z); ;
        minimap.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, minimap.transform.position.z); 
    }
}
