using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public PetriNet enemyNet;
    public float speed;
    public Vector3 direction = Vector3.up;//(0,0,0)
    bool running = false;
    Vector3 destination;
    Rigidbody2D rb;

    public int maxHP; // define HP máxima do jogador
    public Place HP; // a HP do jogador
    public int damage;
    
    


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Initialize());
        enemyNet = new PetriNet("Assets/Networks/enemy.pflow");
        rb = GetComponent<Rigidbody2D>();

        HP = enemyNet.GetPlaceByLabel("HP");
        enemyNet.GetPlaceByLabel("#AddHP").Tokens = maxHP;

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

    private void OnCollisionEnter2D(Collision2D collision)
    {        
        if (collision.gameObject.CompareTag("Player"))
        {  
            enemyNet.GetPlaceByLabel("#collision.player").AddTokens(1);
            GameObject other = collision.gameObject;

            if (other.GetComponent<Player>().playerNet.GetPlaceByLabel("Shield").Tokens != 1)
                other.GetComponent<Player>().playerNet.GetPlaceByLabel("#RemoveHP").AddTokens(damage);
           
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // Tiros
    {
       
        if (collision.gameObject.name == "projectile")
        {
            Debug.Log(HP.Tokens);
            enemyNet.GetPlaceByLabel("#collision.arrow").Tokens = collision.gameObject.GetComponent<Projectile>().damage;
            Destroy(collision.gameObject);
            if (enemyNet.GetPlaceByLabel("IsDead").Tokens > 0)
            {
                Destroy(gameObject);
            }
           
        }

    }
     IEnumerator Initialize()
    {
        yield return new WaitForSeconds(1);
        enemyNet.GetPlaceByLabel("#Initialize").Tokens = 1;
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
