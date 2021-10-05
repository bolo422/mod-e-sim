using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 35; 
    private Vector3 sDirection;

    void Setup(Vector3 sDirection)
    {        
        //this.sDirection = sDirection;

        //Vector3 normDirection = sDirection.normalized;
        //float n = Mathf.Atan2(normDirection.y, normDirection.x) * Mathf.Rad2Deg;
        //if (n < 0) n += 360;

        //transform.eulerAngles = new Vector3(0, 0, n);
    }

    private void Start()
    {
        name = "projectile";
        Destroy(gameObject, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //float mSpeed = 100f;
        //transform.position += sDirection * mSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("wall"))
        {
            Destroy(gameObject);
        }
    }
}
