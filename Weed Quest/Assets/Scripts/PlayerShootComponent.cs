using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;

public class PlayerShootComponent : MonoBehaviour
{
    [SerializeField]
    private Transform spawnPosition;

    [SerializeField]
    private GameObject projectile;

    [SerializeField]
    private float bulletSpeed;

    private Vector2 lookDirection;
    private float lookAngle;

    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        lookDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.parent.position;
        lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, lookAngle - 90f);

        if(Input.GetMouseButtonDown(0) && player.GetComponent<Player>().playerNet.GetPlaceByLabel("Mana").Tokens >= 5)
        {
            player.GetComponent<Player>().playerNet.GetPlaceByLabel("#ProjectileFired").AddTokens(5);
            FireProjectile();
        }        
    }

    void FireProjectile()
    {
        GameObject projectileCast = Instantiate(projectile, spawnPosition.position, transform.rotation);
        projectileCast.GetComponent<Rigidbody2D>().velocity = transform.up * bulletSpeed;
    }
}
