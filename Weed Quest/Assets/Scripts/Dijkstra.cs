using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{
    public CaveGenerator caveGenerator;
    public int range = 50;

    public Positions[] Pathfinding(Vector2 start, Vector2 end)
    {
        Debug.Log("começando o pathfinding");
        //Return variable
        List<Positions> path = new List<Positions>();
        //List<Positions> visitedNodes = new List<Positions>();

        Positions currentPosition = new Positions();
        Positions endPosition = new Positions();
        Positions startPosition = new Positions();

        List<Positions> unvisitedPositions = new List<Positions>();


        int iterator = 0;

        Positions[] positionsArray = caveGenerator.mainZone.Allpositions.ToArray();
        float minDistanceStart = Mathf.Infinity;
        float minDistanceEnd = Mathf.Infinity;

        for (int i = 0; i < positionsArray.Length; i++)
        {
            unvisitedPositions.Add(positionsArray[i]);
            positionsArray[i].visited = false;
            positionsArray[i].totalTravelCost = Mathf.Infinity;
            //start coord to Positions
            if (Vector2.Distance(start, positionsArray[i].pos) < minDistanceStart)
            {
                minDistanceStart = Vector2.Distance(start, positionsArray[i].pos);
                currentPosition = positionsArray[i];
                startPosition = positionsArray[i];
            }

            //end coord to Positions
            if (Vector2.Distance(end, positionsArray[i].pos) < minDistanceEnd)
            {
                minDistanceEnd = Vector2.Distance(end, positionsArray[i].pos);
                endPosition = positionsArray[i];
            }
        }

        

        currentPosition.totalTravelCost = 0;

        bool visitedAll = false;
        while (!visitedAll)
        //for (int k = 0; k < 4; k++)
        {
            
            iterator++;
            currentPosition.nIterator = iterator;
            //Debug.Log("IT: " + iterator);
            //Set new neighborns
            currentPosition.newNeighborn(caveGenerator.map, caveGenerator.height, caveGenerator.width, 1, caveGenerator);
            currentPosition.visited = true;
            unvisitedPositions.Remove(currentPosition);
            Positions[] tempArray = currentPosition.newNeigh.ToArray();

            //Loop trough neighborns and set they TTC
            for (int i = 0; i < tempArray.Length; i++)
            {
                if (tempArray[i].totalTravelCost > currentPosition.totalTravelCost + tempArray[i].travelCost)
                {
                    tempArray[i].totalTravelCost = currentPosition.totalTravelCost + tempArray[i].travelCost;
                    tempArray[i].parentPosition = currentPosition;
                    if(tempArray[i].totalTravelCost > range - 1)
                    {
                        unvisitedPositions.Remove(tempArray[i]);
                    }
                    //Debug.Log("TTC setado: " + iterator);
                }
            }

            //Set Nearest Unvisited Position as the next currentPosition
            float lowestTTC = range;
            visitedAll = true;
            for (int i = 0; i < tempArray.Length; i++)
            {
                if (!tempArray[i].visited && tempArray[i].totalTravelCost < lowestTTC)
                {
                    lowestTTC = tempArray[i].totalTravelCost;
                    currentPosition = tempArray[i];
                    visitedAll = false;
                }
            }

            //lowestTTC = Mathf.Infinity;
            if (visitedAll && unvisitedPositions.Count > 0)
            {
                for (int i = 0; i < unvisitedPositions.Count; i++)
                {
                    if (unvisitedPositions[i].totalTravelCost < range)
                    {
                        currentPosition = unvisitedPositions[i];
                        visitedAll = false;
                        break;
                    }
                }
            }
        }

        if (endPosition.visited)
        {
            path.Add(endPosition);
            while (endPosition != startPosition)
            {
                //Debug.Log("adding path node: " + endPosition.parentPosition.nIterator);
                path.Add(endPosition.parentPosition);
                endPosition = endPosition.parentPosition;
            }
            path.Reverse();
        }

        Debug.Log("finalizando o pathfinding");
        return path.ToArray();
    }
}
