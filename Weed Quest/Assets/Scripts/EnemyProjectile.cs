using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed;

    private Transform player;
    private Vector2 target;
    private int damage;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        target = new Vector2(player.position.x, player.position.y);

        damage = 10;
        //Destroy(gameObject, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //movimento até o alvo
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);


        //GetComponent<Rigidbody2D>().velocity = transform.up * speed;

        //destruição após atingir posição do alvo
        if (transform.position.x == target.x && transform.position.y == target.y)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        { 
            GameObject other = collision.gameObject;

            if (other.GetComponent<Player>().playerNet.GetPlaceByLabel("Shield").Tokens != 1)
                //Destroy(other);
                other.GetComponent<Player>().playerNet.GetPlaceByLabel("#RemoveHP").AddTokens(damage);

            Destroy(gameObject);

        }
    }


}
