using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartObject : MonoBehaviour
{
    public bool displayCollision;

    private void OnDrawGizmos()
    {

        if (displayCollision)
        {
            if (gameObject.GetComponent<Collider>() != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(gameObject.transform.position, gameObject.GetComponent<Collider>().bounds.size);
            }
        }
    }
}
