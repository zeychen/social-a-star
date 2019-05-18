using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Influences : MonoBehaviour
{
    // array that keeps track of each class which corresponds to a layer
    // all classes here currently require user input
    public List<Influence> influenceLayers;   // array that stores a collection of the layers that overlays the A* path finding
    public Dictionary<string, List<LayerWeight>> smartObjectDictionary = new Dictionary<string, List<LayerWeight>>();   // dictionary that uses layer as key and a list of smart object names

    private void Awake()
    {

        foreach (Influence influenceLayer in influenceLayers)
        {
            //layers.Add(influenceLayer.influenceLayer);
            //// add layers to Influences
            if (influenceLayer.smartObjects.Count > 0)
            {
                //List<string> objects = new List<string>();
                foreach(SmartObjects obj in influenceLayer.smartObjects)
                {
                    // create layerweight pair
                    LayerWeight pair = new LayerWeight();
                    pair.objWeight = obj.weight;
                    pair.layerTag = influenceLayer.influenceLayer;
                    pair.layerWeight = influenceLayer.layerWeight;
                    // check to see if key exists in dictionary
                    if (smartObjectDictionary.ContainsKey(obj.ObjectTag))
                    {
                        // if yes, then add to existing key value pair
                        smartObjectDictionary[obj.ObjectTag].Add(pair);
                    } else
                    {
                        // if not, then add new key value pair
                        List<LayerWeight> objLayers = new List<LayerWeight>();
                        objLayers.Add(pair);
                        smartObjectDictionary.Add(obj.ObjectTag, objLayers);
                    }

                }
            }
        }

    }
}

public class LayerWeight
{
    public int objWeight;
    public string layerTag;
    public int layerWeight;
}

//copy this below to make new class of elements that impacts the weight of a smart object
[System.Serializable]
public class Influence
{
    public string influenceLayer;
    public List<SmartObjects> smartObjects;
    public bool active;     // turns layer on or off
    public int layerWeight;     // relative weight between the layers
}

[System.Serializable]
public class SmartObjects
{
    public string ObjectTag;
    public int weight;
}

