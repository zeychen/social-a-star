using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Priority_Queue;

public class PathFinding : MonoBehaviour
{
    public int socialWeight;
    PathRequestManager requestManager;
    AStarGrid grid;
    private void Awake()
    {

        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<AStarGrid>();    // equivalent to import Grid.cs
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {
            SimplePriorityQueue<Node> openSet = new SimplePriorityQueue<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Enqueue(startNode, 0);

            while (openSet.Count > 0)
            {
                //Node currentNode = openSet[0];
                Node currentNode = openSet.Dequeue();

                // optimized search for lowest fCost using priority queue
                closedSet.Add(currentNode);

                if(currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbor in grid.GetNeighbors(currentNode))
                {
                    if(!neighbor.walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if(newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.iCost = GetInfluence(neighbor) * socialWeight;
                        neighbor.parent = currentNode;

                        

                        if (!openSet.Contains(neighbor))
                            openSet.Enqueue(neighbor, neighbor.fCost);
                        else
                            openSet.UpdatePriority(neighbor, neighbor.fCost);
                    }
                }
            }
            yield return null;
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, targetNode);
            }
            requestManager.FinishedProcessingPath(waypoints, pathSuccess);
        }
    }



    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    // heuristic portion of A star
    int GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distY = Mathf.Abs(a.gridY - b.gridY);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
    }

    // influence cost portion of A star
    int GetInfluence(Node neighbor)
    {
        int influenceValues = 0;
        // raycast to figure out whether the path hits an influence area
        Ray ray = new Ray(neighbor.worldPosition + Vector3.up * 50, Vector3.down);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 100))
        {
            // get movementPenalty
            GameObject seeker = GameObject.FindGameObjectWithTag("Player");
            Dictionary<string, List<LayerWeight>> objDict = seeker.GetComponent<Influences>().smartObjectDictionary;    // key: smart object - value: layers
            string key = hit.collider.gameObject.name;
            if (objDict.ContainsKey(key))
            {
                // iterate and sum up the weights of each layer
                foreach(LayerWeight weight in objDict[key])
                {
                    influenceValues += weight.objWeight;
                    print(influenceValues);
                    influenceValues += weight.layerWeight;
                }
            }
        }

        return influenceValues;
    }

}
