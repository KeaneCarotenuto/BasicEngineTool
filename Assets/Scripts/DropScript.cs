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
    [SerializeField] public List<DropTable> dropTables = new List<DropTable>();
    [SerializeField] public bool combineTables = false;

    [SerializeField] public Transform dropLocation = null;
    [SerializeField] public Vector3 dropOffset = Vector3.zero;

    //Drop options
    [SerializeField] public bool dropOnDestroy = false;
    [SerializeField] public bool allowMultipleDrops = false;
    [SerializeField] public bool canDrop = true;
    [SerializeField] public int maxDrops = 1;

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
