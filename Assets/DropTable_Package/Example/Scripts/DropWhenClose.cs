using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An example script to show how to use the DropScript
/// </summary>
public class DropWhenClose : MonoBehaviour
{
    [Tooltip("The DropScript to use")]
    public DropScript dropScript;

    [Tooltip("When this object comes within the distance, it will drop")]
    public Transform objectToTrack;

    [Tooltip("The distance to drop")]
    public float distance;

    private void Start() {
        //if no script, try get it from the object
        if (dropScript == null) {
            dropScript = GetComponent<DropScript>();
        }

        //if no object to track, try get it from the scene
        if (objectToTrack == null && GameObject.FindGameObjectWithTag("Player")) objectToTrack = GameObject.FindGameObjectWithTag("Player").transform;
        if (objectToTrack == null && GameObject.Find("Player")) objectToTrack = GameObject.Find("Player").transform;
    }

    private void Update() {
        //if within distace, drop
        if (dropScript != null && objectToTrack && Vector3.Distance(transform.position, objectToTrack.position) < distance) {
            //do drop
            dropScript.QueueDrop();
        }
    }
}
