using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaveGenerator : MonoBehaviour
{
    public int width, height;
    
    [Range(0, 100)]
    public int randomFillPercent;
    public int seed;
    public bool useRandomSeed;
    [Range(0, 5)]
    public int smooth;
    public GameObject tileset, tilesetFlood, tilesetMain;
    float xi = -1.0f;
    float yi = 1.0f;
    public GameObject player, enemy1, enemy2, hp, sp, stamina, weed, portal;
    public int[] hpQtd;
    public int[] spQtd;
    public int[] staminaQtd;
    public int[] weedQtd;

    public Text hpText;
    public Text spText;
    public Text staminaText;
    public Text weedText;

    public Slider hpSlider;
    public Slider spSlider;
    public Slider staminaSlider;

    public cameraPlayer mainCamera;

    public int[,] map;
    Queue<Zones> zones = new Queue<Zones>();
    Queue<Zones> backup = new Queue<Zones>();
    Zones mainZone = new Zones();

    // Start is called before the first frame update
    void Start()
    {
        randomFillPercent += LevelSettings.level * 2;
        generateCave();
        setMainZone();
        spawnObjects();

        //for (int i = 0; i < mainZone.spots.Length; i++)
        //{
        //    GameObject tilePrefab = tilesetMain;
        //    Vector3 p = tilePrefab.transform.position;
        //    p.x = mainZone.spots[i].pos.x;
        //    p.y = mainZone.spots[i].pos.y;
        //    GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            generateCave();
        }
    }

    void generateCave()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < smooth; i++)
        {
            SmoothMap();
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    FloodFillZone(x, y);
                }
            }
        }

        drawCave();

    }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 1000);
            //Debug.Log(seed);
        }

        System.Random number = new System.Random(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    map[x, y] = 1;
                else
                    map[x, y] = (number.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = getNeightbourWallsCount(x, y);
                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;
            }
        }
    }

    void FloodFillZone(int x, int y)
    {        
        Queue<Positions> flood = new Queue<Positions>();
        Positions current = new Positions();
        Zones zone = new Zones();

        Vector2Int begin = new Vector2Int(x, y);
        current.setPos(begin);
        flood.Enqueue(current);

        while (flood.Count != 0)
        {
            current = flood.Peek();
            zone.addPosition(current);
            flood.Peek().setNeighborn(map, height, width, 2);
            flood.Peek().alterarMap(flood, map, height, width);
            flood.Dequeue();
        }

        zones.Enqueue(zone);
    }

    int getNeightbourWallsCount(int x, int y)
    {
        int wallCount = 0;
        for (int neightbourX = x - 1; neightbourX <= x + 1; neightbourX++)
            for (int neightbourY = y - 1; neightbourY <= y + 1; neightbourY++)
            {
                if (neightbourX >= 0 && neightbourX < width && neightbourY >= 0 && neightbourY < height)
                {
                    if (neightbourX != x || neightbourY != y)
                    {
                        wallCount += map[neightbourX, neightbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
                
            }

        return wallCount;
    }

    void drawCave()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (map[x, y] == 1)
                    {
                        GameObject tilePrefab = tileset;
                        Vector3 p = tilePrefab.transform.position;
                        p.x = x;
                        p.y = y;
                        GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
                        newTile.GetComponent<WallSetup>().setCaveParent(gameObject);
                        newTile.GetComponent<WallSetup>().setCoord(x, y);
                    }
                    if (map[x, y] == 2)
                    {
                        GameObject tilePrefab = tilesetFlood;
                        Vector3 p = tilePrefab.transform.position;
                        p.x = x;
                        p.y = y;
                        GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
                    }
                }
            }
        }
    }

    void setMainZone()
    {          
        int maxSize = 0;

        while (zones.Count != 0)
        {
            if (zones.Peek().size > maxSize)
            {                
               maxSize = zones.Peek().size;
            }
            backup.Enqueue(zones.Peek());
            zones.Dequeue();
        }

        for (int i = 0; i < backup.Count; i++)
        {
            if (backup.Peek().size == maxSize)
            {
                mainZone = backup.Peek();
                mainZone.setSpots();
                break;
            }
            
            backup.Dequeue();
        }

    }

    void spawnObjects()
    {
        Debug.Log("Main: " + mainZone.size);
        //Rands
        int r = Random.Range(0, mainZone.spots.Length);                                             
        int rWeed = Random.Range(weedQtd[0], weedQtd[1]);
        int rPortal = Random.Range(0, mainZone.spots.Length);

        //Portal
        Vector3 portalPos = new Vector3(mainZone.spots[rPortal].pos.x, mainZone.spots[rPortal].pos.y, 0.0f);
        //Player
        GameObject tilePrefab = player;
        Vector3 prefabPosition = tilePrefab.transform.position;
        prefabPosition.x = mainZone.spots[r].pos.x;
        prefabPosition.y = mainZone.spots[r].pos.y;
        tilePrefab.GetComponent<Player>().InstantiateHelp(hpSlider, spSlider, staminaSlider, weedText, mainCamera, rWeed, portalPos);
        GameObject newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;


        //Enemy
        for (int i = 0; i < rWeed * 2.5; i++)
        {
            r = Random.Range(0, mainZone.spots.Length);

            int r2 = Random.Range(0, 1);
            if (r2 == 0) { tilePrefab = enemy1; }
            else { tilePrefab = enemy2; }

            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[r].pos.x;
            prefabPosition.y = mainZone.spots[r].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //Weed
        for (int i = 0; i < rWeed; i++)
        {
            r = Random.Range(0, mainZone.spots.Length);
            tilePrefab = weed;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[r].pos.x;
            prefabPosition.y = mainZone.spots[r].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
            
        }

        //HpPot
        for (int i = 0; i < Random.Range(hpQtd[0], hpQtd[1]); i++)
        {
            r = Random.Range(0, mainZone.spots.Length);
            tilePrefab = hp;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[r].pos.x;
            prefabPosition.y = mainZone.spots[r].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //SpPot
        for (int i = 0; i < Random.Range(spQtd[0], spQtd[1]); i++)
        {
            r = Random.Range(0, mainZone.spots.Length);
            tilePrefab = sp;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[r].pos.x;
            prefabPosition.y = mainZone.spots[r].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //StaminaPot
        for (int i = 0; i < Random.Range(staminaQtd[0], staminaQtd[1]); i++)
        {
            r = Random.Range(0, mainZone.spots.Length);
            tilePrefab = stamina;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[r].pos.x;
            prefabPosition.y = mainZone.spots[r].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

    }


}

public class Positions
{

    public Vector2Int pos;
    public Queue<Positions> neighborns = new Queue<Positions>();
    public int counter;
    public bool hasNeighborns;

    public void setPos(Vector2Int position)
    {
        pos = position;
    }
    public void setNeighborn(int[,] maze, int width, int depht, int newCounter)
    {
        Vector2Int neigh = new Vector2Int();
        Positions newNeigh = new Positions();

        neigh.x = pos.x - 1;
        neigh.y = pos.y;
        if (neigh.x >= 0)
            if (maze[neigh.x, neigh.y] == 0)
            {
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                newNeigh.counter = newCounter;
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x;
        neigh.y = pos.y - 1;
        if (neigh.y >= 0)
            if (maze[neigh.x, neigh.y] == 0)
            {
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                newNeigh.counter = newCounter;
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x + 1;
        neigh.y = pos.y;
        if (neigh.x < depht)
            if (maze[neigh.x, neigh.y] == 0)
            {
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                newNeigh.counter = newCounter;
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x;
        neigh.y = pos.y + 1;
        if (neigh.y < width)
            if (maze[neigh.x, neigh.y] == 0)
            {
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                newNeigh.counter = newCounter;
                neighborns.Enqueue(newNeigh);
            }

    }

    public int getNeightbornQnt()
    {
        return neighborns.Count;
    }

    public void teste()
    {
        Queue<Positions> teste = new Queue<Positions>();

        while (neighborns.Count != 0)
        {
            Debug.Log("Position: " + counter + " | " + neighborns.Peek().pos);
            teste.Enqueue(neighborns.Peek());
            neighborns.Dequeue();
        }
        neighborns = teste;

    }

    public void alterarMap(Queue<Positions> main, int[,] maze, int width, int depht)
    {
        while (neighborns.Count != 0)
        {
            maze[neighborns.Peek().pos.x, neighborns.Peek().pos.y] = counter;
            main.Enqueue(neighborns.Peek());
            neighborns.Dequeue();
        }
    }

}

public class Zones
{
    public Queue<Positions> Allpositions = new Queue<Positions>();
    public int size = 0;
    public Positions[] spots; 

    public void addPosition(Positions pos)
    {
        Allpositions.Enqueue(pos);
        size++;
    }
    public void setSpots()
    {
        spots = Allpositions.ToArray();
    }

}
