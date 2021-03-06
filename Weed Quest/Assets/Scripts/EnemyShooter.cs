using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;
using UnityEngine.UI;

public class EnemyShooter : MonoBehaviour
{
    public PetriNet enemyNet;

    public float speed;
    public float stoppingDistance;
    public float retreatDistance;

    private Transform player;

    public int maxHP; // define HP m?xima do jogador
    public Place HP; // a HP do jogador
    public int damage;

    private float timeBtwShots;
    public float startTimeBtwShots;
    public GameObject projectile;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Initialize());
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyNet = new PetriNet("Assets/Networks/enemy.pflow");
        HP = enemyNet.GetPlaceByLabel("HP");
        enemyNet.GetPlaceByLabel("#AddHP").AddTokens(maxHP + (LevelSettings.level * 10));

        timeBtwShots = startTimeBtwShots;
    }

    // Update is called once per frame
    void Update()
    {
        //float lookAngle = Mathf.Atan2(player.position.y, player.position.x) * Mathf.Rad2Deg;
        //transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);


        // Recuar ou Avan?ar no player
        if (distanceToPlayer > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
        else if (distanceToPlayer < stoppingDistance && distanceToPlayer > retreatDistance)
        {
            transform.position = this.transform.position;
        }
        else if (distanceToPlayer < retreatDistance)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, -speed * Time.deltaTime);
        }

        //atirar e timer
        if (timeBtwShots <= 0 && distanceToPlayer < (stoppingDistance + retreatDistance/2))
        {
            GameObject shot = Instantiate(projectile, transform.position, Quaternion.identity);
            shot.GetComponent<EnemyProjectile>().damage = damage;
            timeBtwShots = startTimeBtwShots;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }

    //public void addHP(int hpToAdd)
    //{
    //    enemyNet.GetPlaceByLabel("#AddHP").Tokens = hpToAdd;
    //}

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

}
