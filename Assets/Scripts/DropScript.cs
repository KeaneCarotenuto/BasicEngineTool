using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DropScript : MonoBehaviour
{
    //NOTE: perhaps make this just one table? and add in combine tables elsewhere?
    [SerializeField] public List<DropTable> dropTables = new List<DropTable>();
    [SerializeField] public bool combineTables = false;

    //position options
    //NOTE: only allow one of these (transform or collider)
    [SerializeField] public Transform dropLocation = null;
    //NOTE: perhaps make some 3D tool to make this easier?
    [SerializeField] public Collider dropArea = null;
    [SerializeField] public Vector3 dropOffset = Vector3.zero;
    [SerializeField] public Vector3 throwForce = Vector3.zero;
    [SerializeField] public float randomAngleArc = 0;

    //Drop options
    [SerializeField] public int repetitionsAllowed = 1;
    [HideInInspector] public int totalReps = 0;
    //if multiple {
    [SerializeField] public float dropDelay = 0.0f;
    //}

    //events
    [SerializeField] public UnityEvent onSuccessfulDrop = new UnityEvent();
    [SerializeField] public UnityEvent onFirstDrop = new UnityEvent();
    [SerializeField] public UnityEvent onLastDrops = new UnityEvent();
    [SerializeField] public UnityEvent onFailedDrop = new UnityEvent();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoDrop(){
        //do drop
        Debug.Log("Drop");
    }

    #if UNITY_EDITOR

    

    #endif
}
