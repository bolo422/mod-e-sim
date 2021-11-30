using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeedSpriteSetup : MonoBehaviour
{
    public SpriteRenderer[] sprites;


    public void SetSprite(int i)
    {
        sprites[i].enabled = true;
    }


}
