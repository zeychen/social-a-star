using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarGrid : MonoBehaviour
{

    // NOTE: expose CiF overlays as masks

    public bool onlyDisplayPathGizmos;
    public Transform player;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;   // NOTE: this is the toggle layer that can have CiF weights
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
    LayerMask walkableMask;
    public GameObject seeker;
    private GameObject[] seekers;
    private List<Attribute> attrArray;


    Node[,] grid;   // Grid is made up of a 2D array of Nodes

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    int blurSize = 1000;

    private void Awake()
    {
        // calculate how many nodes can fit into grid
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        // save seeker
        try
        {
            Unit sAttr = seeker.GetComponent<Unit>();
            
            print(sAttr);
        }
        catch
        {
            print("Need Game Object Name Seeker");
        }
        

        foreach (TerrainType region in walkableRegions)
        {
            // add layers to TerrainType
            walkableMask.value |= region.terrainMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }


    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        // loop through all positions of node for collision check
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                // check object collision
                //bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));  // true when there is no collision in unwalkable mask

                int movementPenalty = 0;

                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, unwalkableMask))
                {
                    // ray cast detects object from unwalkable layer
                    // each game object within unwalkable layer needs to be a SmartObject
                    // find game object group
                    // use group to find layer of corresponding movement penalty
                    SmartObject sob = hit.collider.gameObject.GetComponent<SmartObject>();
                    // need attributes from seeker to calculate movement penalty and blur size (extent of social impact)
                    if (sob.type == SmartObject.GroupType.Dog)
                    {
                        walkableRegionsDictionary.TryGetValue(10, out movementPenalty);
                    }

                }

                // NOTE: add CiF overlay here?
                // pass parameters needed to CiF - return social value of area = movementPenalty

                // raycast to find layers to calculate movementPenalty for each node
                // currently loops through all of the layers defined - can only set values for one layer at a time
                //if (walkable)
                //{
                //    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                //    RaycastHit hit;
                //    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                //    {
                //        // hits object from unwalkable layer
                //        // find game object group
                //        // use group to find layer of corresponding movement penalty
                //        SmartObject sob = hit.collider.gameObject.GetComponent<SmartObject>();
                //        if(sob.type == SmartObject.GroupType.Cat)
                //        {
                //            walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                //            print(movementPenalty);
                //        }

                //    }

                //    // set blur size to control distance 
                //}

                //print(movementPenalty);
                grid[x, y] = new Node(true, worldPoint, x ,y, movementPenalty);
            }
        }

        BlurPenaltyMap(blurSize);
    }

    // blur distance of penalty
    void BlurPenaltyMap(int bSize)
    {
        int kernelSize = bSize * 2 + 1;
        int kernelExtents = (kernelSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++)
        {
            for (int x = -kernelExtents; x <= kernelExtents; x++)
            {
                int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++)
            {
                int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = -kernelExtents; y <= kernelExtents; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
            }
        }

    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        for(int x = -1; x <=1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >=0 && checkX<gridSizeX && checkY >=0 && checkY < gridSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbors;
    }



    // convert world position to grid position
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public List<Node> path;

    // draw
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if(onlyDisplayPathGizmos)
        {
            if (path !=null)
            {
                foreach (Node n in path)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }
        }
        else
        {
            if(grid != null)
            {
                Node playerNode = NodeFromWorldPoint(player.position);
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    //if (playerNode == n)
                    //{
                    //    Gizmos.color = Color.black;
                    //}
                    if (path != null)
                        if (path.Contains(n))
                            Gizmos.color = Color.black;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
                }
            }

        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

}
