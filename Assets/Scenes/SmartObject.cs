using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartObject : MonoBehaviour
{
    public bool displayCollision;
    //public int weight;
    //public List<InfluenceLayer> influenceLayers = new List<InfluenceLayer>();
    //public string layers;
    //Dropdown layers;
    //public list<string> influenceLayers = new List<string>();
    //public LayerMask influenceLayer; // which layer this smart object should be in?
    //public string group;
    //public string type;
    //GameObject objectInfluence;

    //new BoxCollider collider;
    //float m_ScaleX, m_ScaleY, m_ScaleZ;
    //public Slider m_SliderX, m_SliderZ;


    //Vector3 worldPosition;

    //[System.Serializable]
    //public class InfluenceLayer
    //{
    //    public string layer;
    //    public int weight;
    //}

    // TODO: Need to make the area reflect correct collision area
    private void OnDrawGizmos()
    {

        //if (displayCollision)
        //{
        //    if (collider != null)
        //    {
        //        Gizmos.color = Color.black;
        //        //Gizmos.DrawWireCube(gameObject.transform.position, collider.size);
        //    }
        //}
    }
}
