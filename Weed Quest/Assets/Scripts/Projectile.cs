using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
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
        //Enemy enemy = collider.GetComponent<Enemy>();
        //if (enemy != null)
        //{
        // Script de causar dano no Inimigo
        // Destroy(gameObject);
        //}            
    }
}
