using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : MonoBehaviour
{ 

    public Positions[] Pathfinding(Vector2 start, Vector2 end, CaveGenerator caveGenerator)
    {
        //Return variable
        List<Positions> path = new List<Positions>();
        //List<Positions> visitedNodes = new List<Positions>();

        Positions currentPosition = new Positions();
        Positions endPosition = new Positions();
        Positions startPosition = new Positions();


        Positions[] positionsArray = caveGenerator.mainZone.Allpositions.ToArray();
        float minDistanceStart = Mathf.Infinity;
        float minDistanceEnd = Mathf.Infinity;

        for (int i = 0; i < positionsArray.Length; i++)
        {
            //start coord to Positions
            if (Vector2.Distance(start, positionsArray[i].pos) < minDistanceStart)
            {
                minDistanceStart = Vector2.Distance(start, positionsArray[i].pos);
                currentPosition = positionsArray[i];
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
        {

            //Set new neighborns
            currentPosition.newNeighborn(caveGenerator.map, caveGenerator.height, caveGenerator.width, 1);
            currentPosition.visited = true;
            Positions[] tempArray = currentPosition.neighborns.ToArray();

            //Loop trough neighborns and set they TTC
            for (int i = 0; i < tempArray.Length; i++)
            {
                if (tempArray[i].totalTravelCost > currentPosition.totalTravelCost + tempArray[i].travelCost)
                {
                    tempArray[i].totalTravelCost = currentPosition.totalTravelCost + tempArray[i].travelCost;
                    tempArray[i].parentPosition = currentPosition;
                }
            }

            //Set Nearest Unvisited Position as the next currentPosition
            float lowestTTC = Mathf.Infinity;
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
        }

        path.Add(endPosition);

        while (endPosition != startPosition)
        {
            path.Add(endPosition.parentPosition);
            endPosition = endPosition.parentPosition;
        }

        return path.ToArray();
    }
}
