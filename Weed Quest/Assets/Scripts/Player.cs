using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RdPengine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public PetriNet playerNet; // a rede de Petri do objeto do Player
    public float speed = 0.05f; // velocidade, afeta movimentação

    private Rigidbody2D rb2d; // rigidbody do objeto do Player
    // NÃO ESQUECER DE CRIAR O BENDITO RIGIDBODY2d -b

    public GameObject shieldComponent;
    public cameraPlayer mainCamera;    

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
    public Place Shield;
    public Place OutOfStamina;
    public Place OutOfHP;
    private Place initialize;    
    
    public Place NextLevel;
    public GameObject portalPrefab;
    public bool portalCreated = false;
    public Vector3 portalPos;


    // Textos a serem atualizados, itens de UI, provavelmente não deviam estar aqui, mas a fins de praticidade:
    public Text hpText;
    public Text spText;
    public Text staminaText;
    public Text weedText;

    public Slider hpSlider;
    public Slider spSlider;
    public Slider staminaSlider;

    // A definir:

    // Se o jogo usar um Manager, a quantia de Weeds coletadas não pode ficar aqui, em cujo caso teria
    // uma "cruzada" entre o Player e a rede de Petri do Manager; Talvez seja melhor fazer a RdP no Inimigo
    // para centralizar essas coisas no Player? -b

    //* Start is called before the first frame update
    void Start()
    {
        mainCamera.player = gameObject;
        portalCreated = false;

        rb2d = GetComponent<Rigidbody2D>(); // carrega o Rigidbody através do objeto em si
        playerNet = new PetriNet("Assets/Networks/player.pflow"); // carrega a PetriNet

        // Apontando os Places para seus devidos lugares

        HP = playerNet.GetPlaceByLabel("HP");
        SP = playerNet.GetPlaceByLabel("Mana");
        Stamina = playerNet.GetPlaceByLabel("Stamina");
        Weed = playerNet.GetPlaceByLabel("Weed");
        Shield = playerNet.GetPlaceByLabel("Shield");
        NextLevel = playerNet.GetPlaceByLabel("Next Level");
        OutOfHP = playerNet.GetPlaceByLabel("Out of HP");
        OutOfStamina = playerNet.GetPlaceByLabel("Out of Stamina");
        initialize = playerNet.GetPlaceByLabel("#Initialize");

        // Seta esses valores para o máximo definido nas devidas variáveis

        playerNet.GetPlaceByLabel("#AddHP").AddTokens(maxHP);
        playerNet.GetPlaceByLabel("#AddMana").AddTokens(maxSP);
        playerNet.GetPlaceByLabel("#AddStamina").AddTokens(maxStamina);
        playerNet.GetPlaceByLabel("#ResetWeed").AddTokens(maxWeed);

        hpSlider.GetComponent<Slider>().maxValue = maxHP;
        spSlider.GetComponent<Slider>().maxValue = maxSP;
        staminaSlider.GetComponent<Slider>().maxValue = maxStamina;

        hpSlider.GetComponent<Slider>().value = HP.Tokens;
        spSlider.GetComponent<Slider>().value = SP.Tokens;
        staminaSlider.GetComponent<Slider>().value = Stamina.Tokens;

        StartCoroutine(StaminaDecay()); // inicia o decay natural de Stamina do player
        StartCoroutine(ManaRegen()); // inicia a regen natural de Mana do player
        StartCoroutine(testInitialized());
    }

    //* Update is called once per frame
    void Update()
    {
        updateBars();
        updateTexts();

        float horizontalImpulse = Input.GetAxis("Horizontal");
        float verticalImpulse = Input.GetAxis("Vertical");
        Vector3 impulse = new Vector3(horizontalImpulse, verticalImpulse, 0);

        GetComponent<Rigidbody2D>().MovePosition(transform.position + impulse * speed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (SP.Tokens >= 10 && Shield.Tokens != 1)
            {
                StartCoroutine(StartShields());
            }
        }




        GameOver();
    }

    private void OnCollisionEnter2D(Collision2D collision) // primariamente colisão com o Inimigo (ou unicamente?)
    {
        //if (collision.gameObject.CompareTag("enemy"))
        //{
        //    //Debug.Log(collision);
        //    PetriNet pn = other.gameObject.GetComponent<Npc1Controller>().npc;
        //    player.GetPlaceByLabel("#CollisionWithEnemy").AddTokens(1, pn);
        //}
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
            StartCoroutine(instantiatePortal());
        }
        else if (collision.gameObject.CompareTag("portal"))
        {
            Debug.Log("Voce venceu!");
            LevelSettings.level += 1;
            SceneManager.LoadScene("Loader");
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
            if (maxSP > SP.Tokens)
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

    IEnumerator StartShields()
    {
        playerNet.GetPlaceByLabel("#ActivateShield").Tokens = 1;
        shieldComponent.SetActive(true);
        playerNet.GetPlaceByLabel("#RemoveMana").Tokens = 10; 
        
        updateBars();

        yield return new WaitForSeconds(5);

        playerNet.GetPlaceByLabel("#ShieldTimeout").Tokens = 1;
        shieldComponent.SetActive(false);
    }

    void updateTexts()
    {
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

    IEnumerator instantiatePortal()
    {
        yield return new WaitForSeconds(2);
        if (maxWeed - Weed.Tokens == 0 && !portalCreated)
        {
            Instantiate(portalPrefab, portalPos, new Quaternion(0, 0, 0, 0));
            Debug.Log("portal instanciado!");
            //Instantiate(portalPrefab, transform.position, transform.rotation);
        }  
    }

    void GameOver()
    {
        if (OutOfHP.Tokens > 0 || OutOfStamina.Tokens > 0)
        {
            Debug.Log("Voce perdeu :(");
            //Debug.Log("Tokens HP: " + OutOfHP.Tokens);
            //Debug.Log("Tokens Stamina: " + OutOfStamina.Tokens);
            SceneManager.LoadScene("Loader");
        }
    }

    IEnumerator testInitialized()
    {
        yield return new WaitForSeconds(2);
        initialize.Tokens = 1;
    }

    public void InstantiateHelp(Slider _hpSlider, Slider _spSlider, Slider _staminaSlider, Text _weedText, cameraPlayer _mainCamera, int _maxWeed, Vector3 _portalPos)
    {
        hpSlider = _hpSlider;
        spSlider = _spSlider;
        staminaSlider = _staminaSlider;
        weedText = _weedText;
        mainCamera = _mainCamera;
        maxWeed = _maxWeed;
        portalPos = _portalPos;
    }

    void updateBars()
    {
        hpSlider.GetComponent<Slider>().value = HP.Tokens;
        spSlider.GetComponent<Slider>().value = SP.Tokens;
        staminaSlider.GetComponent<Slider>().value = Stamina.Tokens;
    }
}
