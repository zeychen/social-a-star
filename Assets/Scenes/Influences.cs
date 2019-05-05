using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Influences : MonoBehaviour
{
    // array that keeps track of each class which corresponds to a layer
    // all classes here currently require user input
    public Influence[] influenceLayers;   // array that stores a collection of the layers that overlays the A* path finding
    public Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();    // Dictionary that keeps track of weight of each layer
    public Dictionary<int, int> influenceXDictionary = new Dictionary<int, int>();
    public Dictionary<int, int> influenceZDictionary = new Dictionary<int, int>();

    int penalty = 0;

    private void Awake()
    {

        foreach (Influence influenceLayer in influenceLayers)
        {
            // add layers to Influences
            int layer = 0;
            if (influenceLayer.influenceMask.value > 0)
            {
                layer = (int)Mathf.Log(influenceLayer.influenceMask.value, 2);  // need this to convert the layer bit values to int
                penalty = (influenceLayer.positivePenalty == true) ? influenceLayer.weight : influenceLayer.weight*(-1);      // takes positive and negative influences into account
                try
                {
                    walkableRegionsDictionary.Add(layer, penalty);
                    influenceXDictionary.Add(layer, influenceLayer.distanceX);
                    influenceZDictionary.Add(layer, influenceLayer.distanceZ);
                }
                catch { }
            }
        }

    }


}

//copy this below to make new class of elements that impacts the weight of a smart object
[System.Serializable]
public class Influence
{
    //public GameObject smartObject;
    public LayerMask influenceMask;
    public bool active;     // turns layer on or off
    //public string type;
    public int weight;
    public bool positivePenalty;   // true for positive weight values = positive penalty
    public int distanceX;
    public int distanceZ;
}
