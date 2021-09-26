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
    public GameObject tileset;
    float xi = -1.0f;
    float yi = 1.0f;

    int[,] map;

    // Start is called before the first frame update
    void Start()
    {
        generateCave();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        //if (map != null)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        for (int y = 0; y < height; y++)
        //        {
        //            if (map[x, y] == 1)
        //            {
        //                GameObject tilePrefab = tileset;
        //                Vector3 p = tilePrefab.transform.position;
        //                p.x = xi + x * width;
        //                p.z = zi - y * height;
        //                GameObject newTile = Instantiate(tilePrefab, p, Quaternion.identity) as GameObject;
        //            }
                    

        //            Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
        //            Vector3 pos = new Vector3(-width / 2 + x /*+ 0.5f*/, -height / 2 + y/* + 0.5f*/);
        //            Gizmos.DrawCube(pos, Vector3.one);
        //        }
        //    }
        //}
    }
}
