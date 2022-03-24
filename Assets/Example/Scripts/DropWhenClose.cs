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

        if (objectToTrack == null) {
            objectToTrack = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void Update() {
        if (dropScript != null && objectToTrack && Vector3.Distance(transform.position, objectToTrack.position) < distance) {
            //do drop
            dropScript.QueueDrop();
        }
    }
}
