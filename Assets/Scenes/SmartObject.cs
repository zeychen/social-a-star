using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject : MonoBehaviour
{
    public bool displayCollision;
    public LayerMask influenceLayer; // which layer this smart object should be in?
    //public string group;
    //public string type;
    //GameObject objectInfluence;
    new BoxCollider collider;
    Vector3 worldPosition;

    void Awake()
    {
        GameObject seeker = GameObject.FindGameObjectWithTag("Player");
        Influences seekerInfluences = seeker.GetComponent<Influences>();
        
        if(influenceLayer > 0)
        {
            gameObject.layer = (int)Mathf.Log(influenceLayer.value, 2); // place smart object in correct layer
            seekerInfluences.influenceXDictionary.TryGetValue(gameObject.layer, out int resizeX);
            seekerInfluences.influenceZDictionary.TryGetValue(gameObject.layer, out int resizeZ);
            //print("layer");
            //print(gameObject.layer);
            //print(resizeX);
            //print(resizeZ);
            collider = gameObject.GetComponent<BoxCollider>();
            collider.size += new Vector3(resizeX, 0, resizeZ);
        }
    }

    // TODO: Need to make the area reflect correct collision area
    private void OnDrawGizmos()
    {

        if (displayCollision)
        {
            if (collider != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(gameObject.transform.position, collider.size);
            }
        }
    }
}
