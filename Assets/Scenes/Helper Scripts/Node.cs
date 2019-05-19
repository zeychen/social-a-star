using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;   // keep track of grid position
    public int gridY;
    public int penalty;

    public int gCost;
    public int hCost;
    public int iCost;
    public Node parent;
    /* Node constructor
     * @param: {bool} walkable
     * @param: {Vector3 world position
     */
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;

    }

    public int fCost
    {
        get
        {
            return gCost + hCost + iCost;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
