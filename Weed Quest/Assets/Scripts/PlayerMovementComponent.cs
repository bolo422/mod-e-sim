using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementComponent : MonoBehaviour
{
    GameObject Player;
    Vector2 moveDirection;
    public float moveSpeed;
    float originalMoveSpeed;
    Rigidbody2D rb2d;
    

    Vector2 lastPosition;

    Collision2D collision;
    bool collided;

    // Start is called before the first frame update
    void Start()
    {
        Player = gameObject.transform.parent.gameObject;
        rb2d = Player.GetComponent<Rigidbody2D>();
        moveSpeed = Player.GetComponent<Player>().moveSpeed;
        originalMoveSpeed = moveSpeed;
    }

    private void FixedUpdate()
    {
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        lastPosition = new Vector2(0, 0);

        if (moveX != 0 || moveY != 0)
        {
            gameObject.transform.position = new Vector2(gameObject.transform.position.x + (moveX / 2), gameObject.transform.position.y + (moveY / 2));

            if (collided)
            {
                if (collision.gameObject.CompareTag("wall") && collision != null)
                {
                    gameObject.transform.position = new Vector2(gameObject.transform.position.x - (moveX / 2), gameObject.transform.position.y - (moveY / 2));
                }
            }
            else
            {
                gameObject.transform.position = new Vector2(gameObject.transform.position.x - (moveX / 2), gameObject.transform.position.y - (moveY / 2));
                moveDirection = new Vector2(moveX / 2, moveY / 2);
            }

            if (moveX != 0 && moveY != 0)
            {
                moveX = Mathf.RoundToInt(moveX / 1.5f);
                moveY = Mathf.RoundToInt(moveY / 1.5f);
            }

            moveDirection = new Vector2(moveX, moveY);
        }

        //Se nenhum input de moviment oestiver ativo, zera toda a física de movimento do rigidbody
        if (moveX == 0 && moveY == 0)
        {
            rb2d.velocity = Vector2.zero;
            rb2d.angularVelocity = 0; rb2d.angularDrag = 0;
            moveDirection = new Vector2(0, 0);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        collided = true;
        this.collision = collision;

    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        collided = false;
        this.collision = null;
    }

    private void Move()
    {
        lastPosition = new Vector2(Player.transform.position.x, Player.transform.position.y);
        rb2d.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
        if (Player.transform.position.x != lastPosition.x + (moveDirection.x * moveSpeed) || Player.transform.position.y != lastPosition.y + (moveDirection.y * moveSpeed))
        {
            rb2d.velocity = new Vector2(0, 0);
            rb2d.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
           // Debug.Log($"test: {rb2d.velocity}");
        }
    }


}

