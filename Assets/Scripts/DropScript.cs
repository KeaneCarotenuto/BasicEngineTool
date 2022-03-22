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
    //NOTE: perhaps make this multi tables? or add in combine tables elsewhere?
    [SerializeField] public DropTable dropTable = null;
    //[SerializeField] public bool combineTables = false;

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
     public int totalReps = 0;
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
        //create copy of drop table to edit
        if (dropTable != null)
        {
            string name = dropTable.name;
            dropTable = Instantiate(dropTable);
            dropTable.name = name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoDrop(){
        if (!CheckTable(dropTable)) return;

        List<GameObject> toDrop = new List<GameObject>();
        toDrop = dropTable.GetDropPrefabs();
        if (toDrop == null || toDrop.Count <= 0) return;

        foreach (GameObject obj in toDrop){
            Instantiate(obj, dropLocation.position + dropOffset, Quaternion.identity);
        }

        totalReps++;
    }

    private void TESTSimpleDrop(){
        //check reps
        if (totalReps >= repetitionsAllowed)
        {
            return;
        }

        List<GameObject> toDrop = new List<GameObject>();

        if (dropTable != null){
            toDrop = dropTable.GetDropPrefabs();
        }
        if (toDrop == null || toDrop.Count <= 0) return;

        if (toDrop.Count > 0){
            //drop
            
        }

        //increment total reps
        totalReps++;
    }

    private bool CheckTable(DropTable table){
        //is valid
        bool isValid = CheckIfTableIsValid(table);
        if (!isValid) return false;

        //rep allowed
        bool repAllowed = CheckIfAnotherRepAllowed();
        if (!repAllowed) return false;

        return true;
    }

    private bool CheckIfAnotherRepAllowed(){
        if (totalReps >= repetitionsAllowed)
        {
            return false;
        }

        return true;
    }

    private bool CheckIfTableIsValid(DropTable table){
        if (table == null) return false;

        if (table.dropList.Count <= 0) return false;

        return true;
    }

    #if UNITY_EDITOR

    

    #endif
}
