using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;

public class Player : MonoBehaviour
{
    public PetriNet playerNet; // a rede de Petri do objeto do Player
    public float speed; // velocidade, afeta movimentação

    private Rigidbody2D rb2d; // rigidbody do objeto do Player
    // NÃO ESQUECER DE CRIAR O BENDITO RIGIDBODY2d -b

    public int maxHP; // define HP máxima do jogador
    public int maxSP; // define Mana máxima do jogador
    public int maxStamina; // define Stamina máxima do jogador

    // Places são "lugares" dentro da PetriNet, usar .Tokens retorna o valor de Tokens, também podendo ser usado
    // para setar a quantia de Tokens um certo lugar possui
    private Place HP; // a HP do jogador
    private Place SP; // a Mana do jogador
    private Place Stamina; // a Stamina do jogador

    // A definir:

    // Se o jogo usar um Manager, a quantia de Weeds coletadas não pode ficar aqui, em cujo caso teria
    // uma "cruzada" entre o Player e a rede de Petri do Manager; Talvez seja melhor fazer a RdP no Inimigo
    // para centralizar essas coisas no Player? -b

    //* Start is called before the first frame update
    void Start()
    {
        playerNet = new PetriNet("Assets/Networks/player.pflow"); // carrega a PetriNet
        rb2d = GetComponent<Rigidbody2D>(); // carrega o Rigidbody através do objeto em si

        // Apontando os Places para seus devidos lugares

        HP = playerNet.GetPlaceByLabel("HP");
        SP = playerNet.GetPlaceByLabel("Mana");
        Stamina = playerNet.GetPlaceByLabel("Stamina");

        // Seta esses valores para o máximo definido nas devidas variáveis

        HP.Tokens = maxHP;
        SP.Tokens = maxSP;
        Stamina.Tokens = maxStamina;

        StartCoroutine(StaminaDecay()); // inicia o decay natural de Stamina do player
    }

    //* Update is called once per frame
    void Update()
    {
        // Isso teoricamente aplica movimentação, eu não tenho muita experiência com Rigidbodies, então tipo
        // é uma cópia do que existe dentro do Update do exemplo passado, como isso funciona é meio que além de mim -b

        float horizontalImpulse = Input.GetAxis("Horizontal");
        float verticalImpulse = Input.GetAxis("Vertical");
        Vector2 impulse = new Vector2(horizontalImpulse, verticalImpulse);
        rb2d.AddForce(impulse * speed);
    }

    private void OnCollisionEnter2D(Collision2D collision) // primariamente colisão com o Inimigo (ou unicamente?)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            // Assume que o Inimigo terá uma RdP

            /*if (other.gameObject.CompareTag(("Enemy")))
            {
            PetriNet pn = other.gameObject.GetComponent<Npc1Controller>().npc;
            player.GetPlaceByLabel("#CollisionWithEnemy").AddTokens(1, pn);
            }*/
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // poções, potencialmente Weed, portais
    {
        if(collision.gameObject.CompareTag("potHP"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
        }
        else if (collision.gameObject.CompareTag("potSP"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
        }
        else if (collision.gameObject.CompareTag("potStamina"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
        }
        // Se a Weed ficar dentro do Player, vai ter mais uma verificação por ela, dentro do Else abaixo
        else
        {
            // Verificação de se é um portal para a próxima fase ocorre aqui dentro
        }
    }

    IEnumerator ManaRegen() // regeneração natural de Mana do player, intencionalmente lenta
    {
        while (HP.Tokens > 0)
        {
            if(maxSP > SP.Tokens)
            {
                // Garantir que o nodo dentro da RdP tem esse nome, é suposto a ser o "aviso" que um tick ocorreu
                playerNet.GetPlaceByLabel("#ManaRegen").Tokens = 1; 
            }
            // mudar "5" para a quantia de segundos entre cada tick de Mana
            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator StaminaDecay() // queda natural de Stamina do player
    {
        while (Stamina.Tokens > 0)
        {
            // Mesmo princípio da função acima, mudar "1" para a quantia de segundos entre cada tick de Stamina
            yield return new WaitForSeconds(1);
            
            playerNet.GetPlaceByLabel("#StaminaTick").Tokens = 1;
        }
    }
}
