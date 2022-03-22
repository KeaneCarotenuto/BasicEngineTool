using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropWhenClose : MonoBehaviour
{
    
    public DropScript dropScript;
    public Transform objectToTrack;
    public float distance;

    private void Start() {
        if (dropScript == null) {
            dropScript = GetComponent<DropScript>();
        }
    }

    private void Update() {
        if (dropScript != null && Vector3.Distance(transform.position, objectToTrack.position) < distance) {
            //do drop
            dropScript.DoDrop();
        }
    }
}
