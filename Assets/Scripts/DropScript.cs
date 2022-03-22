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

    public bool debugMode = false;

    private void OnDrawGizmos() {
        if (!debugMode) return;

        //draw force and cone
        //calculate position
        Vector3 dropPos = Vector3.zero;
        if (dropLocation != null) {
            dropPos = dropLocation.position;
            dropPos += dropOffset;
        }
        else if (dropArea != null) {
            //get random point within area
            Vector3 randomPoint = dropArea.bounds.center + RandomPointInColliderBox(dropArea);

            dropPos = randomPoint;
            dropPos += dropOffset;
        }
        else {
            dropPos = transform.position;
        }

        Vector3 throwDir = throwForce;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(dropPos, dropPos + throwDir);
        Gizmos.DrawWireSphere(dropPos, throwDir.magnitude/10.0f);
        Gizmos.DrawWireSphere(dropPos + throwDir, throwDir.magnitude/10.0f);
        Gizmos.DrawWireSphere(dropPos, throwDir.magnitude);


        //draw random line
        Gizmos.color = Color.green;
        Vector3 start = throwDir;
        for (int i = 0; i < 50; i++) {
            Gizmos.DrawLine(dropPos, dropPos + RandomPointOnSphereRandomAngle(throwDir, randomAngleArc));
        }
        
    }


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

    //probably have to hook this up to update loop to do it with delays
    public void DoDrop(){
        if (!CheckTable(dropTable)) return;
        Debug.Log("DropScript: DoDrop " + totalReps);

        List<GameObject> toDrop = new List<GameObject>();
        toDrop = dropTable.GetDropPrefabs();
        if (toDrop == null || toDrop.Count <= 0) return;

        foreach (GameObject drop in toDrop){
            DropObject(drop);
        }

        totalReps++;
    }

    private void DropObject(GameObject drop){
        if (drop == null) return;
        
        //calculate position
        Vector3 dropPos = Vector3.zero;
        if (dropLocation != null) {
            dropPos = dropLocation.position;
            dropPos += dropOffset;
        }
        else if (dropArea != null) {
            //get random point within area
            Vector3 randomPoint = dropArea.bounds.center + RandomPointInColliderBox(dropArea);

            dropPos = randomPoint;
            dropPos += dropOffset;
        }
        else {
            dropPos = transform.position;
        }
        

        GameObject instance = Instantiate(drop, dropPos, Quaternion.identity);

        //calculate throw force
        Vector3 throwDir = throwForce;
        if (randomAngleArc > 0) {
            throwDir = RandomPointOnSphereRandomAngle(throwDir, randomAngleArc);
        }
        //apply force
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = throwDir;
        }

    }

    public Vector3 RandomPointInColliderBox(Collider collider){
        if (collider == null) return Vector3.zero;

        Vector3 randomPoint = new Vector3(UnityEngine.Random.Range(
            -collider.bounds.extents.x, collider.bounds.extents.x),
            UnityEngine.Random.Range(-collider.bounds.extents.y, collider.bounds.extents.y),
            UnityEngine.Random.Range(-collider.bounds.extents.z, collider.bounds.extents.z));
        return randomPoint;
    }

    public Vector3 RandomPointOnSphereRandomAngle(Vector3 direction, float anlge){
        if (direction == null || direction == Vector3.zero) return Vector3.zero;

        Vector3 start = direction;
        Vector3 target = RandomPointOnSphereFixedAngle(direction, anlge);
        float randLerp = UnityEngine.Random.Range(0.0f, 1.0f);
        Vector3 randLine = Vector3.Lerp(start, target, randLerp);
        randLine = randLine.normalized * direction.magnitude;
        return randLine;
    }

    public Vector3 RandomPointOnSphereFixedAngle(Vector3 direction, float anlge)
    {
        float radius = Mathf.Tan(Mathf.Deg2Rad*anlge/2) * direction.magnitude;
        Vector2 circle = UnityEngine.Random.insideUnitCircle * radius;
        //convert direction to quaternion
        Quaternion directionQuat = Quaternion.LookRotation(direction);
        Vector3 target = direction + directionQuat*new Vector3(circle.x, circle.y);
        return target.normalized * direction.magnitude;
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
