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
    public GameObject tileset, tilesetFlood, tilesetMain, tileGround1, tileWater, destroyer;
    float xi = -1.0f;
    float yi = 1.0f;
    public GameObject player, enemy1, enemy2, hp, sp, stamina, weed, portal, sparks;
    public Pathfinding pathfinding;
    int WeedQnt;
    int enemiesQnt;
    int potionsQnt;
    int spotsQnt, spotsSmooth;
    Vector3 portalPos;

    Player playerRef;
    Positions[] path;
    public Dijkstra dijkstra;

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
    Zones floodedZone = new Zones();

    // Start is called before the first frame update
    void Start()
    {
        setupObjectsQnt();
        generateCave();             
        setMainZone();
        generateSpotCave();
        setTravelCost(3);
        drawCave();
        spawnObjects();
        pathfinding.map = map;

        spotsQnt = -1; // Remover quando isso finalmente for usado
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //eventSpawn("hp", 50);
            createFloodArea(true);
        }

        if (Input.GetKey(KeyCode.K))
        {
            path = dijkstra.Pathfinding(new Vector2(playerRef.transform.position.x, playerRef.transform.position.y),
            new Vector2(portalPos.x, portalPos.y));

            if (path.Length > 0)
            {
                Debug.Log("Caminho encontrado!");
                for (int i = 0; i < path.Length; i++)
                {
                    GameObject tilePrefab = sparks;
                    Vector3 prefabPosition = new Vector3(path[i].pos.x, path[i].pos.y, 0);
                    GameObject instant = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
                }
            }
            else
            {
                Debug.Log("Caminho não encontrado!");
            }

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
            testarray[i].totalTravelCost.ToString());
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
                if (mapSpots[x, y] == 1 && map[x, y] == 2)
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
                        //prefab -> mainZone.Allpositions[i]; i++
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
        portalPos = new Vector3(mainZone.spots[rPortal].pos.x, mainZone.spots[rPortal].pos.y, 0.0f);
        //Player
        GameObject tilePrefab = player;
        Vector3 prefabPosition = tilePrefab.transform.position;
        prefabPosition.x = mainZone.spots[random].pos.x;
        prefabPosition.y = mainZone.spots[random].pos.y;
        tilePrefab.GetComponent<Player>().InstantiateHelp(hpSlider, spSlider, staminaSlider, weedText, mainCamera, WeedQnt, portalPos);
        GameObject newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
        playerRef = newTile.GetComponent<Player>();

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
            if(newTile.GetComponent<Enemy>() != null)
            {
                newTile.GetComponent<Enemy>().damage += LevelSettings.level;
                //newTile.GetComponent<Enemy>().addHP(LevelSettings.level * 15);
            }
            else
            {
                newTile.GetComponent<EnemyShooter>().damage += LevelSettings.level;
                //newTile.GetComponent<EnemyShooter>().addHP(LevelSettings.level * 10);
            }
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
    // 1 + 2 + 3 + 4 + 5 + 6 == 21
    // 10 = 30%
    // 2% > 4% > 6% 
    void setupObjectsQnt() // Trocar o fixo para um algoritmo que utilize o randômico que iremos implementar para dar variação e continuidade às fases
    {
        //randomFillPercent += Mathf.FloorToInt(LevelSettings.level * 2f);

        if (LevelSettings.level <= 3)
        {
            randomFillPercent = new int();
            randomFillPercent = Mathf.FloorToInt(((LevelSettings.level * 2) + 30) + Random.Range(0, Mathf.RoundToInt(0.75f * LevelSettings.level + 1)));
            potionsQnt = 6;
            enemiesQnt = 25 - Random.Range(Mathf.RoundToInt(LevelSettings.level / 2), Mathf.RoundToInt(3f * LevelSettings.level + 1));
            WeedQnt = 4;
            //spotsQnt = 10;
        }
        else if (LevelSettings.level <= 6 && LevelSettings.level > 3)
        {
            randomFillPercent = new int();
            randomFillPercent = Mathf.FloorToInt(((LevelSettings.level * 2) + 30) + Random.Range(0, Mathf.RoundToInt(0.75f * LevelSettings.level)));
            potionsQnt = 4;
            enemiesQnt = 15 - Random.Range(Mathf.RoundToInt(LevelSettings.level / 2), Mathf.RoundToInt(1f * LevelSettings.level));
            WeedQnt = 3;
            //spotsQnt = -1;
        }
        else
        {
            randomFillPercent = new int();
            randomFillPercent = Mathf.FloorToInt(((LevelSettings.level * 2) + 30) + Random.Range(0, Mathf.RoundToInt(0.75f * LevelSettings.level)));
            potionsQnt = 1;
            enemiesQnt = 5;
            WeedQnt = 2;
            //spotsQnt = -1;
        }

        Debug.Log("Random Fill: " + randomFillPercent + "%");
        Debug.Log("Level: " + LevelSettings.level);
    }

    void setTravelCost(int cost)
    {
        for (int i = 0; i < mainZone.spots.Length; i++)
        {
            if (map[mainZone.spots[i].pos.x, mainZone.spots[i].pos.y] == cost)
            {
                mainZone.spots[i].travelCost = cost;
            }
        }
    }

    void eventSpawn(string type, int qnt)
    {
        GameObject tilePrefab = new GameObject();
        int uses = qnt;

        if (type == "enemy")
            tilePrefab = enemy1;
        else if (type == "enemy2")
            tilePrefab = enemy2;
        else if (type == "hp")
            tilePrefab = hp;
        else if (type == "sp")
            tilePrefab = sp;
        else if (type == "stamina")
            tilePrefab = stamina;

        while(uses != 0)
        {
            int randomPosition = Random.Range(0, mainZone.spots.Length);
            Debug.Log(map[mainZone.spots[randomPosition].pos.x, mainZone.spots[randomPosition].pos.y]);
            if (map[mainZone.spots[randomPosition].pos.x, mainZone.spots[randomPosition].pos.y] == 2) //Verifico se já tem algo naquela posição
            {
                Vector3 prefabPosition = tilePrefab.transform.position;
                prefabPosition.x = mainZone.spots[randomPosition].pos.x;
                prefabPosition.y = mainZone.spots[randomPosition].pos.y;
                GameObject newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
                uses--;
            }
        }
        
    }

    IEnumerator updateWater()
    {        
        while (enabled)
        {
            Debug.Log("After Main Zone: " + mainZone.spots.Length);
            yield return new WaitForSeconds(2.5f);

            int lenght = floodedZone.spots.Length;
            for (int i = 0; i < lenght; i++)
            {
                while(floodedZone.spots[i].neighborns.Count != 0)
                {
                    floodedZone.spots[i].neighborns.Peek().setNeighbornFlood(map, height, width);
                    floodedZone.addPosition(floodedZone.spots[i].neighborns.Peek());
                    floodedZone.setSpots();
                    removeSprite(floodedZone.spots[i].neighborns.Peek());
                    Debug.Log("To be Removed: " + floodedZone.spots[i].neighborns.Peek().pos);
                    mainZone.removePosition(floodedZone.spots[i].neighborns.Peek().pos);
                    floodedZone.spots[i].neighborns.Dequeue();
                }
            }
        }
        Debug.Log("End Main Zone: " + mainZone.spots.Length);
    }

    void createFloodArea(bool coroutine = false)
    {
        Positions random = mainZone.spots[Random.Range(0, mainZone.spots.Length)];
        random.setNeighbornFlood(map, height, width);
        floodedZone.addPosition(random);
        floodedZone.setSpots();
        removeSprite(random);
        mainZone.removePosition(random.pos);
        if(coroutine)
            StartCoroutine(updateWater());
    }

    void removeSprite(Positions pos)
    {
        GameObject tilePrefab = destroyer;
        Vector3 p = tilePrefab.transform.position;
        p.x = pos.pos.x;
        p.y = pos.pos.y;
        GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
    }

}

public class Positions
{
    public Vector2Int pos;
    public Queue<Positions> neighborns = new Queue<Positions>();
    public List<Positions> newNeigh = new List<Positions>();
    public GameObject prefab = new GameObject();
    public int counter;
    public bool hasNeighborns;
    public int travelCost = 1;
    public float totalTravelCost = Mathf.Infinity;
    public Positions parentPosition;
    public bool visited = false;
    public int nIterator = 0;

    public void setPos(Vector2Int position)
    {
        pos = position;
    }

    public void newNeighborn(int[,] maze, int width, int depht, int type, CaveGenerator caveGenerator)
    {
        Vector2Int neigh = new Vector2Int();
        Positions[] posArray = caveGenerator.mainZone.Allpositions.ToArray();
        float lowestDistance = Mathf.Infinity;
        int closestDistanceIndex = 0;

        neigh.x = pos.x - 1;
        neigh.y = pos.y;
        if (neigh.x >= 0)
        {
            if (maze[neigh.x, neigh.y] != type)
            {
                for (int i = 0; i < posArray.Length; i++)
                {
                    if (Vector2.Distance(neigh, posArray[i].pos) < lowestDistance)
                    {
                        lowestDistance = Vector2.Distance(neigh, posArray[i].pos);
                        closestDistanceIndex = i;
                    }
                }
                newNeigh.Add(posArray[closestDistanceIndex]);
                //Debug.Log("Neigh added x-");
            }
        }

        lowestDistance = Mathf.Infinity;
        neigh.x = pos.x;
        neigh.y = pos.y - 1;
        if (neigh.y >= 0)
        {
            if (maze[neigh.x, neigh.y] != type)
            {
                for (int i = 0; i < posArray.Length; i++)
                {
                    if (Vector2.Distance(neigh, posArray[i].pos) < lowestDistance)
                    {
                        lowestDistance = Vector2.Distance(neigh, posArray[i].pos);
                        closestDistanceIndex = i;
                    }
                }
                newNeigh.Add(posArray[closestDistanceIndex]);
               // Debug.Log("Neigh added y-");
            }
        }

        lowestDistance = Mathf.Infinity;
        neigh.x = pos.x + 1;
        neigh.y = pos.y;
        if (neigh.x < depht)
        {
            if (maze[neigh.x, neigh.y] != type)
            {
                for (int i = 0; i < posArray.Length; i++)
                {
                    if (Vector2.Distance(neigh, posArray[i].pos) < lowestDistance)
                    {
                        lowestDistance = Vector2.Distance(neigh, posArray[i].pos);
                        closestDistanceIndex = i;
                    }
                }
                newNeigh.Add(posArray[closestDistanceIndex]);
                //Debug.Log("Neigh added x+");
            }
        }

        lowestDistance = Mathf.Infinity;
        neigh.x = pos.x;
        neigh.y = pos.y + 1;
        if (neigh.y < width)
        {
            if (maze[neigh.x, neigh.y] != type)
            {
                for (int i = 0; i < posArray.Length; i++)
                {
                    if (Vector2.Distance(neigh, posArray[i].pos) < lowestDistance)
                    {
                        lowestDistance = Vector2.Distance(neigh, posArray[i].pos);
                        closestDistanceIndex = i;
                    }
                }
                newNeigh.Add(posArray[closestDistanceIndex]);
                //Debug.Log("Neigh added y+");
            }
        }

    }

    public void setNeighbornFlood(int[,] maze, int width, int depht)
    {
        Vector2Int neigh = new Vector2Int();
        Positions newNeigh = new Positions();

        neigh.x = pos.x - 1;
        neigh.y = pos.y;
        if (neigh.x >= 0)
            if (maze[neigh.x, neigh.y] > 1)
            {
                maze[neigh.x, neigh.y] = -1;
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x;
        neigh.y = pos.y - 1;
        if (neigh.y >= 0)
            if (maze[neigh.x, neigh.y] > 1)
            {
                maze[neigh.x, neigh.y] = -1;
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x + 1;
        neigh.y = pos.y;
        if (neigh.x < depht)
            if (maze[neigh.x, neigh.y] > 1)
            {
                maze[neigh.x, neigh.y] = -1;
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                neighborns.Enqueue(newNeigh);
            }

        neigh.x = pos.x;
        neigh.y = pos.y + 1;
        if (neigh.y < width)
            if (maze[neigh.x, neigh.y] > 1)
            {
                maze[neigh.x, neigh.y] = -1;
                newNeigh = new Positions();
                newNeigh.setPos(neigh);
                neighborns.Enqueue(newNeigh);
            }


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


public class Zones : MonoBehaviour
{
    public Queue<Positions> Allpositions = new Queue<Positions>();
    Queue<Positions> backupAllpositions = new Queue<Positions>();
    public int size = 0;
    public Positions[] spots; 

    public void addPosition(Positions pos)
    {
        Allpositions.Enqueue(pos);
        size++;
    }
    public void removePosition(Vector2Int pos)
    {
        bool end = false;
        while (!end)
        {
            if(Allpositions.Count != 0)
            {
                if (Allpositions.Peek().pos == pos)
                {
                    Allpositions.Dequeue();
                    while (backupAllpositions.Count != 0)
                    {
                        Allpositions.Enqueue(backupAllpositions.Peek());
                        backupAllpositions.Dequeue();
                    }
                    setSpots();
                    end = true;
                }
                else
                {
                    backupAllpositions.Enqueue(Allpositions.Peek());
                    Allpositions.Dequeue();
                    size--;
                }
            }
            else
            {
                while (backupAllpositions.Count != 0)
                {
                    Allpositions.Enqueue(backupAllpositions.Peek());
                    backupAllpositions.Dequeue();
                }
                setSpots();
                end = true;
            }
            
        }
    }
    public void spawnPrefab(Positions pos, GameObject tilePrefab)
    {
        Vector3 prefabPosition = tilePrefab.transform.position;
        prefabPosition.x = pos.pos.x;
        prefabPosition.y = pos.pos.y;
        GameObject newTile = Instantiate(tilePrefab, prefabPosition, Quaternion.identity) as GameObject;
    }
    public void setSpots()
    {
        spots = Allpositions.ToArray();
    }


}




