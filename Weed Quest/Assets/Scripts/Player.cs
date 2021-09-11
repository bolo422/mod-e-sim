using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public PetriNet playerNet; // a rede de Petri do objeto do Player
    public float speed; // velocidade, afeta movimentação

    private Rigidbody2D rb2d; // rigidbody do objeto do Player
    // NÃO ESQUECER DE CRIAR O BENDITO RIGIDBODY2d -b

    public int maxHP; // define HP máxima do jogador
    public int maxSP; // define Mana máxima do jogador
    public int maxStamina; // define Stamina máxima do jogador
    public int maxWeed; // define a quantia de Weed

    // Places são "lugares" dentro da PetriNet, usar .Tokens retorna o valor de Tokens, também podendo ser usado
    // para setar a quantia de Tokens um certo lugar possui
    public Place HP; // a HP do jogador
    public Place SP; // a Mana do jogador
    public Place Stamina; // a Stamina do jogador
    public Place Weed; // a quantia de Weed que o jogador coletou

    // Textos a serem atualizados, itens de UI, provavelmente não deviam estar aqui, mas a fins de praticidade:
    public Text hpText;
    public Text spText;
    public Text staminaText;
    public Text weedText;

    // A definir:

    // Se o jogo usar um Manager, a quantia de Weeds coletadas não pode ficar aqui, em cujo caso teria
    // uma "cruzada" entre o Player e a rede de Petri do Manager; Talvez seja melhor fazer a RdP no Inimigo
    // para centralizar essas coisas no Player? -b

    //* Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>(); // carrega o Rigidbody através do objeto em si
        playerNet = new PetriNet("Assets/Networks/player.pflow"); // carrega a PetriNet

        // Apontando os Places para seus devidos lugares

        HP = playerNet.GetPlaceByLabel("HP");
        SP = playerNet.GetPlaceByLabel("Mana");
        Stamina = playerNet.GetPlaceByLabel("Stamina");
        Weed = playerNet.GetPlaceByLabel("Weed");

        // Seta esses valores para o máximo definido nas devidas variáveis

        playerNet.GetPlaceByLabel("#AddHP").AddTokens(maxHP);
        playerNet.GetPlaceByLabel("#AddMana").AddTokens(maxSP);
        playerNet.GetPlaceByLabel("#AddStamina").AddTokens(maxStamina);
        playerNet.GetPlaceByLabel("#ResetWeed").AddTokens(maxWeed);

        StartCoroutine(StaminaDecay()); // inicia o decay natural de Stamina do player
        StartCoroutine(ManaRegen()); // inicia a regen natural de Mana do player
    }

    //* Update is called once per frame
    void Update()
    {
        checkForSurplus();
        updateTexts();

        float horizontalImpulse = Input.GetAxis("Horizontal");
        float verticalImpulse = Input.GetAxis("Vertical");
        Vector2 impulse = new Vector2(horizontalImpulse, verticalImpulse);

        GetComponent<Rigidbody2D>().AddForce(impulse * speed);
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
        if (collision.gameObject.CompareTag("potHP"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
            playerNet.GetPlaceByLabel("#@HPPot").AddTokens(10);
        }
        else if (collision.gameObject.CompareTag("potSP"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
            playerNet.GetPlaceByLabel("#@ManaPot").AddTokens(10);
        }
        else if (collision.gameObject.CompareTag("potStamina"))
        {
            collision.gameObject.SetActive(false); // desabilita a poção, para não ser coletada 2 vezes
            playerNet.GetPlaceByLabel("#@StamPot").AddTokens(10);
        }
        else if (collision.gameObject.CompareTag("weed"))
        {
            collision.gameObject.SetActive(false); // desabilita a weed, para não ser coletada 2 vezes
            playerNet.GetPlaceByLabel("#@WeedPickup").AddTokens(1);
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
            yield return new WaitForSeconds(3);
            
            playerNet.GetPlaceByLabel("#StaminaTick").Tokens = 1;
        }
    }

    void updateTexts()
    {
        hpText.text = "HP: " + HP.Tokens.ToString() + "/" + maxHP.ToString();
        spText.text = "SP: " + SP.Tokens.ToString() + "/" + maxSP.ToString();
        staminaText.text = "Stamina: " + Stamina.Tokens.ToString() + "/" + maxStamina.ToString();
        weedText.text = "Weed: " + Weed.Tokens.ToString() + "/" + maxWeed.ToString();
    }

    void checkForSurplus()
    {
        if (HP.Tokens > maxHP)
        {
            playerNet.GetPlaceByLabel("#RemoveHP").AddTokens(HP.Tokens - maxHP);
        }
        else if (SP.Tokens > maxSP)
        {
            playerNet.GetPlaceByLabel("#RemoveMana").AddTokens(SP.Tokens - maxSP);
        }
        else if (Stamina.Tokens > maxStamina)
        {
            playerNet.GetPlaceByLabel("#RemoveStamina").AddTokens(Stamina.Tokens - maxStamina);
        }
    }
}
