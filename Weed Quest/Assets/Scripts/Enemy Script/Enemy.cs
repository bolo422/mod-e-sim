using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;


public class Enemy : MonoBehaviour
{
    public float speed;
    public Vector3 direction = Vector3.up;//(0,0,0)
    bool running = false;
    Vector3 destination;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!running)
        {
            StartCoroutine(changeDirection());
        }
        //destination = transform.position + direction;
        //transform.position += direction * speed;
        //transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime*2);
        //rb.AddForce(direction * speed,ForceMode2D.Impulse);
        rb.velocity += new Vector2(direction.x, direction.y) * Time.deltaTime;
    }
    IEnumerator changeDirection()
    {
        running = true;
        yield return new WaitForSeconds(1);
        direction.x = Random.Range(-1, 2);
        direction.y = Random.Range(-1, 2);
        running = false;
    }
}
