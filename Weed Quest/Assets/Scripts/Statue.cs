using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Statue : MonoBehaviour
{
    public Canvas canvas;
    public GameObject sparks;
    Positions[] path;

    private void Start()
    {
        canvas.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.enabled = true;
            collision.GetComponent<Player>().statueRange = true;
            
            if(collision.GetComponent<Player>().statue != this)
            {
                Debug.Log("statue ref set into player");
                collision.GetComponent<Player>().statue = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.enabled = false;
            collision.GetComponent<Player>().statueRange = false;
        }
    }

    public void drawPath()
    {
        for (int i = 0; i < path.Length; i++)
        {
            GameObject tilePrefab = sparks;
            Vector3 prefabPosition = new Vector3(path[i].pos.x, path[i].pos.y, 0);
            GameObject instance = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }
        Debug.Log($"{this.name}: Path drawn");
    }

    public void discoverPath(Vector2 portalCoord, Dijkstra dijkstra)
    {
        path = dijkstra.Pathfinding(new Vector2(transform.position.x, transform.position.y), portalCoord);

        if (path.Length > 0)
        {
            Debug.Log($"{this.name} set path successefuly");
        }
    }
}
