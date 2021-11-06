using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CaveGenerator : MonoBehaviour
{
    public int width, height;
    
    [Range(0, 100)]
    public int randomFillPercent;
    public int seed;
    public bool useRandomSeed;
    [Range(0, 5)]
    public int smooth;
    public GameObject tileset, tilesetFlood, tilesetMain, tileGround1;
    float xi = -1.0f;
    float yi = 1.0f;
    public GameObject player, enemy1, enemy2, hp, sp, stamina, weed, portal;
    int WeedQnt;
    int enemiesQnt;
    int potionsQnt;
    int spotsQnt, spotsSmooth;

    public Text hpText;
    public Text spText;
    public Text staminaText;
    public Text weedText;

    public Slider hpSlider;
    public Slider spSlider;
    public Slider staminaSlider;

    public cameraPlayer mainCamera;

    public int[,] map, mapSpots;
    Queue<Zones> zones = new Queue<Zones>();
    Queue<Zones> backup = new Queue<Zones>();
    public Zones mainZone = new Zones();

    // Start is called before the first frame update
    void Start()
    {
        randomFillPercent += LevelSettings.level * 2;
        setupObjectsQnt();
        generateCave();
        generateSpotCave();
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
        setMainZone();
        setTravelCost(3);
        drawCave();
        spawnObjects();

        for (int i = 0; i < mainZone.spots.Length; i++)
        {
            if(mainZone.spots[i].travelCost != 0)
                Debug.Log(mainZone.spots[i].travelCost + "\n");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            generateCave();
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR

        Positions[] testarray = mainZone.Allpositions.ToArray();
        for (int i = 0; i < testarray.Length; i++)
        {
            Handles.Label(
            new Vector3(testarray[i].pos.x, testarray[i].pos.y, 100),
            testarray[i].travelCost.ToString());

            //string label;
            //if(testarray[i].travelCost >= 85 && testarray[i].travelCost <= 94){ label = "m"; }
            //else if(testarray[i].travelCost >= 95){ label = "D"; }
            //else{ label = " "; }

            //Handles.Label(
            //new Vector3(testarray[i].pos.x, testarray[i].pos.y, 100),
            //label);
        }
#endif
    }


    void generateCave()
    {
        map = new int[width, height];
        RandomFillMap(100, map, randomFillPercent);

        for (int i = 0; i < smooth; i++)
        {
            SmoothMap(map);
        }

        

    }

    void generateSpotCave()
    {
        mapSpots = new int[width, height];
        RandomFillMap(100, mapSpots, randomFillPercent);
        SmoothMap(mapSpots);
        convertMaps();
    }

    void RandomFillMap(int max, int[,] mapRef, int fillPercent)
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
                if (x <= 2 || x >= width - 3 || y <= 2 || y >= height - 3)
                    mapRef[x, y] = 1;
                else
                    mapRef[x, y] = (number.Next(0, max) < fillPercent) ? 1 : 0;
            }
        }
    }

    void SmoothMap(int[,] mapRef)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = getNeightbourWallsCount(x, y, mapRef);
                if (neighbourWallTiles > 4)
                    mapRef[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    mapRef[x, y] = 0;
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

    int getNeightbourWallsCount(int x, int y, int[,] mapRef)
    {
        int wallCount = 0;
        for (int neightbourX = x - 1; neightbourX <= x + 1; neightbourX++)
            for (int neightbourY = y - 1; neightbourY <= y + 1; neightbourY++)
            {
                if (neightbourX >= 0 && neightbourX < width && neightbourY >= 0 && neightbourY < height)
                {
                    if (neightbourX != x || neightbourY != y)
                    {
                        wallCount += mapRef[neightbourX, neightbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
                
            }

        return wallCount;
    }

    void convertMaps()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapSpots[x, y] == 1 && map[x, y] == 0)
                {
                    map[x, y] = 3;
                }

            }
        }

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
                    if (map[x, y] == 3)
                    {
                        GameObject tilePrefab = tileGround1;
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

        int random = Random.Range(0, mainZone.spots.Length);
        int rPortal = Random.Range(0, mainZone.spots.Length);

        //Portal
        Vector3 portalPos = new Vector3(mainZone.spots[rPortal].pos.x, mainZone.spots[rPortal].pos.y, 0.0f);
        //Player
        GameObject tilePrefab = player;
        Vector3 prefabPosition = tilePrefab.transform.position;
        prefabPosition.x = mainZone.spots[random].pos.x;
        prefabPosition.y = mainZone.spots[random].pos.y;
        tilePrefab.GetComponent<Player>().InstantiateHelp(hpSlider, spSlider, staminaSlider, weedText, mainCamera, WeedQnt, portalPos);
        GameObject newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;


        //Enemy
        for (int i = 0; i < enemiesQnt; i++)
        {
            random = Random.Range(0, mainZone.spots.Length);

            int r2 = Random.Range(0, 1);
            if (r2 == 0) { tilePrefab = enemy1; }
            else { tilePrefab = enemy2; }

            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[random].pos.x;
            prefabPosition.y = mainZone.spots[random].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //Weed
        for (int i = 0; i < WeedQnt; i++)
        {
            random = Random.Range(0, mainZone.spots.Length);
            tilePrefab = weed;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[random].pos.x;
            prefabPosition.y = mainZone.spots[random].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
            
        }

        //HpPot
        for (int i = 0; i < potionsQnt; i++)
        {
            random = Random.Range(0, mainZone.spots.Length);
            tilePrefab = hp;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[random].pos.x;
            prefabPosition.y = mainZone.spots[random].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //SpPot
        for (int i = 0; i < potionsQnt; i++)
        {
            random = Random.Range(0, mainZone.spots.Length);
            tilePrefab = sp;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[random].pos.x;
            prefabPosition.y = mainZone.spots[random].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

        //StaminaPot
        for (int i = 0; i < potionsQnt; i++)
        {
            random = Random.Range(0, mainZone.spots.Length);
            tilePrefab = stamina;
            prefabPosition = tilePrefab.transform.position;
            prefabPosition.x = mainZone.spots[random].pos.x;
            prefabPosition.y = mainZone.spots[random].pos.y;
            newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        }

    }

    void setupObjectsQnt()
    {
        if (LevelSettings.level <= 3)
        {
            potionsQnt = 5;
            enemiesQnt = 6;
            WeedQnt = 10;
            spotsQnt = 10;
        }
        else if (LevelSettings.level <= 6 && LevelSettings.level > 3)
        {
            potionsQnt = 2;
            enemiesQnt = 3;
            WeedQnt = 5;
            spotsQnt = -1;
        }
        else
        {
            potionsQnt = 1;
            enemiesQnt = 2;
            WeedQnt = 2;
            spotsQnt = -1;
        }

    }

    void setTravelCost(int cost)
    {
        for (int i = 0; i < mainZone.spots.Length; i++)
        {
            if (mapSpots[mainZone.spots[i].pos.x, mainZone.spots[i].pos.y] == 1 && map[mainZone.spots[i].pos.x, mainZone.spots[i].pos.y] == 0)
            {
                map[mainZone.spots[i].pos.x, mainZone.spots[i].pos.y] = cost;
                mainZone.spots[i].travelCost = cost;
            }
        }
    }

}

public class Positions
{

    public Vector2Int pos;
    public Queue<Positions> neighborns = new Queue<Positions>();
    public int counter;
    public bool hasNeighborns;
    public int travelCost;// = -1;//Random.Range(1, 101);


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


