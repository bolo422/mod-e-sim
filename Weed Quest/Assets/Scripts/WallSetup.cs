using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WallSetup : MonoBehaviour
{
    int cX, cY;
    CaveGenerator caveManager;
    SpriteHolder forestSprite;
    SpriteRenderer sprite;
    
    
    [SerializeField]
    int maxX, maxY;

    // Start is called before the first frame update
    void Start()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        forestSprite = GameObject.Find("ForestSpriteHolder").GetComponent<SpriteHolder>();
        StartCoroutine(setupCoroutine());

        maxX = caveManager.width;
        maxY = caveManager.height;
    }

    public void setCaveParent(GameObject cave)
    {
        caveManager = cave.GetComponent<CaveGenerator>();
    }

    public void setCoord(int x, int y)
    {
        cX = x;
        cY = y;
    }

    IEnumerator setupCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        if (cX >= 2 && cX <= maxX - 3 && cY >= 2 && cY <= maxY - 3)
        {
            // Arbustos 1x1
            if (caveManager.map[cX - 1, cY] != 1 && caveManager.map[cX + 1, cY] != 1 && caveManager.map[cX, cY + 1] != 1 ||
                caveManager.map[cX - 1, cY] != 1 && caveManager.map[cX + 1, cY] != 1 && caveManager.map[cX, cY - 1] != 1 ||
                caveManager.map[cX, cY + 1] != 1 && caveManager.map[cX, cY - 1] != 1 && caveManager.map[cX + 1, cY] != 1 ||
                caveManager.map[cX, cY + 1] != 1 && caveManager.map[cX, cY - 1] != 1 && caveManager.map[cX - 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[22];
            }
            // Cima/Baixo
            else if (caveManager.map[cX, cY - 1] != 1 && caveManager.map[cX - 1, cY] == 1 && caveManager.map[cX + 1, cY] == 1)
            {
                sprite.sprite = forestSprite.forestSprites[15];
            }
            else if (caveManager.map[cX, cY + 1] != 1 && caveManager.map[cX - 1, cY] == 1 && caveManager.map[cX + 1, cY] == 1)
            {
                sprite.sprite = forestSprite.forestSprites[1];
            }
            // Cantos Exteriores
            else if (caveManager.map[cX, cY + 1] != 1 && caveManager.map[cX - 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[0];
            }
            else if (caveManager.map[cX, cY + 1] != 1 && caveManager.map[cX + 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[2];
            }
            else if (caveManager.map[cX, cY - 1] != 1 && caveManager.map[cX - 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[14];
            }
            else if (caveManager.map[cX, cY - 1] != 1 && caveManager.map[cX + 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[16];
            }
            // Esquerda/Direita
            else if (caveManager.map[cX, cY - 1] == 1 && caveManager.map[cX, cY + 1] == 1 && caveManager.map[cX + 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[9];
            }
            else if (caveManager.map[cX, cY - 1] == 1 && caveManager.map[cX, cY + 1] == 1 && caveManager.map[cX - 1, cY] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[7];
            }
            // Cantos Interiores Diagonais
            else if (caveManager.map[cX + 1, cY + 1] != 1)
            {
                if (caveManager.map[cX + 1, cY + 1] != 1 && caveManager.map[cX + 1, cY - 1] != 1 && caveManager.map[cX + 2, cY] != 1) // Se do lado tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[9];
                }
                else if (caveManager.map[cX - 1, cY + 1] != 1 && caveManager.map[cX + 1, cY + 1] != 1 && caveManager.map[cX, cY + 2] != 1) // Se do lado tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[1];
                }
                else
                {
                    sprite.sprite = forestSprite.forestSprites[17];
                }
            }
            else if (caveManager.map[cX - 1, cY + 1] != 1)
            {
                if (caveManager.map[cX - 1, cY + 1] != 1 && caveManager.map[cX - 1, cY - 1] != 1 && caveManager.map[cX - 2, cY] != 1) // Se do lado tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[7];
                }
                else if (caveManager.map[cX - 1, cY + 1] != 1 && caveManager.map[cX + 1, cY + 1] != 1 && caveManager.map[cX, cY + 2] != 1) // Se em cima tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[1];
                }
                else
                {
                    sprite.sprite = forestSprite.forestSprites[18];
                }
            }
            else if (caveManager.map[cX + 1, cY - 1] != 1)
            {
                if (caveManager.map[cX + 1, cY + 1] != 1 && caveManager.map[cX + 1, cY - 1] != 1 && caveManager.map[cX + 2, cY] != 1) // Se do lado tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[9];
                }
                else if (caveManager.map[cX - 1, cY - 1] != 1 && caveManager.map[cX + 1, cY - 1] != 1 && caveManager.map[cX, cY - 2] != 1) // Se embaixo tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[15];
                }
                else
                {
                    sprite.sprite = forestSprite.forestSprites[10];
                }
            }
            else if (caveManager.map[cX - 1, cY - 1] != 1)
            {
                if (caveManager.map[cX - 1, cY + 1] != 1 && caveManager.map[cX - 1, cY - 1] != 1 && caveManager.map[cX - 2, cY] != 1) // Se do lado tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[7];
                }
                else if (caveManager.map[cX - 1, cY - 1] != 1 && caveManager.map[cX + 1, cY - 1] != 1 && caveManager.map[cX, cY - 2] != 1) // Se embaixo tem um arbusto, se torna um lado normal
                {
                    sprite.sprite = forestSprite.forestSprites[15];
                }
                else
                {
                    sprite.sprite = forestSprite.forestSprites[11];
                }
            }
            // Parte "alta" da árvore reta
            else if (caveManager.map[cX, cY - 2] != 1)
            {
                sprite.sprite = forestSprite.forestSprites[8];
            }


            // Internos Diagonais Superiores, o código pega as slots erradas
            //else if (caveManager.map[cX, cY - 1] == 1 && caveManager.map[cX - 1, cY] == 1)
            //{
            //    if (caveManager.map[cX - 2, cY] != 1 || caveManager.map[cX - 1, cY - 1] != 1 || caveManager.map[cX, cY - 2] != 1)
            //    {
            //        sprite.sprite = forestSprite.forestSprites[4];
            //    }
            //}
            //else if (caveManager.map[cX, cY - 1] == 1 && caveManager.map[cX + 1, cY] == 1)
            //{
            //    if (caveManager.map[cX + 2, cY] != 1 || caveManager.map[cX + 1, cY - 1] != 1 || caveManager.map[cX, cY - 2] != 1)
            //    {
            //        sprite.sprite = forestSprite.forestSprites[3];
            //    }
            //}
        }
    }

}
