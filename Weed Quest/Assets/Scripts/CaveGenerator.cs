using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public int width, height;

    [Range(0, 100)]
    public int randomFillPercent;
    public string seed;
    public bool useRandomSeed;
    [Range(0, 5)]
    public int smooth;
    public GameObject tileset, tilesetFlood, tilesetMain;
    float xi = -1.0f;
    float yi = 1.0f;
    public GameObject player, enemy1, enemy2, hp, mp, ammo, weed, portal;

    int[,] map;
    Queue<Zones> zones = new Queue<Zones>();
    Queue<Zones> backup = new Queue<Zones>();
    Zones mainZone = new Zones();

    // Start is called before the first frame update
    void Start()
    {
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
            seed = Time.time.ToString();
        }

        System.Random number = new System.Random(seed.GetHashCode());

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
        int r = Random.Range(0, mainZone.spots.Length);

        GameObject tilePrefab = player;
        Vector3 p = tilePrefab.transform.position;
        p.x = mainZone.spots[r].pos.x;
        p.y = mainZone.spots[r].pos.y;
        GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;

        r = Random.Range(0, mainZone.spots.Length);

        tilePrefab = enemy1;
        p = tilePrefab.transform.position;
        p.x = mainZone.spots[r].pos.x;
        p.y = mainZone.spots[r].pos.y;
        newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
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
