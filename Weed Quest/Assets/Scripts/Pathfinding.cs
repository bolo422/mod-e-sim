using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

    public int[,] map;
    Node[,] nodes;
    

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < map.Length; x++)
        {
            for (int y = 0; y < map.Length; y++)
            {
                
                switch (map[x,y])
                {
                    case 1:
                        nodes[x, y] = new Node(new Vector2Int(x, y), Mathf.Infinity);
                        break;

                    case 2:
                        nodes[x, y] = new Node(new Vector2Int(x, y), 1);
                        break;

                    case 3:
                        nodes[x, y] = new Node(new Vector2Int(x, y), 3);
                        break;
                }

                //positions[x,y] = 
            //if map[] == 1 positions[i].travelCost = infinito e além




            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //Vector2Int b = new Vector2Int(1, 1);
        //Node a = new Node(b, 5);
    }

    void Djikstra2()// posição player, posição objetivo)
    {
        //djikstra igual o outro
    }



    public class Node
    {
        public Node(Vector2Int _pos, float _travelCost)
        {
            pos = _pos;
            travelCost = _travelCost;
        }

        public Vector2Int pos;
        public List<Positions> newNeigh = new List<Positions>();
        public float travelCost;
        public float totalTravelCost = Mathf.Infinity;
        public Positions parentPosition;
        public bool visited = false;
        public int nIterator = 0;


        void Neighborns()
        {
            //verificar vizinhos e quais infos vou precisar para isso
        }


    }

    // Quando gerar os objetos, gerar emptyObjects que vão servir como Spots para gerar os inimigos


}
