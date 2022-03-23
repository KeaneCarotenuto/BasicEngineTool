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

    static private bool showLocationSettings = false;

    //custom editor
    [CustomEditor(typeof(DropScript))]
    public class DropScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DropScript myScript = (DropScript)target;

            GUIStyle boldCenterLabelStyle = new GUIStyle(EditorStyles.label);
            boldCenterLabelStyle.fontStyle = FontStyle.Bold;
            boldCenterLabelStyle.alignment = TextAnchor.MiddleCenter;

            GUIStyle centerLabelStyle = new GUIStyle(EditorStyles.label);
            centerLabelStyle.alignment = TextAnchor.MiddleCenter;

            //DrawDefaultInspector();

            //Custom display starts here
            DrawDropTable(boldCenterLabelStyle, myScript);

            EditorGUILayout.Space();

            //create coloured box around next elements
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            //begin with some padding
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = oldColor;

            //indent
            EditorGUI.indentLevel++;
            //foldout for location and offset
            showLocationSettings = EditorGUILayout.Foldout(showLocationSettings, "Drop Settings");
            if (showLocationSettings)
            {
                DrawLocation(boldCenterLabelStyle, myScript);
                DrawOffset(myScript);
                DrawThrowForce(myScript);

                if (myScript.throwForce.magnitude > 0)
                {
                    DrawRandomAngleArc(myScript);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void OnSceneGUI() {
            //draw throw force line with handle
            DropScript myScript = (DropScript)target;

            if (myScript.throwForce.magnitude > 0)
            {
                Vector3 start = myScript.dropLocation.position + myScript.dropOffset;
                myScript.throwForce = Handles.PositionHandle(start + myScript.throwForce, Quaternion.LookRotation(myScript.throwForce)) - start;
                Handles.color = Color.green;
                Handles.DrawLine(start, myScript.throwForce + start);

            }
        }

        private static void DrawDropTable(GUIStyle boldCenterLabelStyle, DropScript myScript)
        {
            //center aligned label with bold font "Drop Table"
            EditorGUILayout.LabelField("Drop Table", boldCenterLabelStyle);
            //drop table field
            myScript.dropTable = (DropTable)EditorGUILayout.ObjectField(myScript.dropTable, typeof(DropTable), true);
        }

        private static void DrawLocation(GUIStyle boldCenterLabelStyle, DropScript myScript)
        {
            //center aligned label with bold font "Drop Location"
            EditorGUILayout.LabelField("Location", boldCenterLabelStyle);
            //two options for drop location side by side, transofrm or collider
            EditorGUILayout.BeginHorizontal();

            //drop location transform
            if (myScript.dropLocation != null)
            {
                //enabled drop location field
                myScript.dropLocation = (Transform)EditorGUILayout.ObjectField(myScript.dropLocation, typeof(Transform), true);
            }
            else if (myScript.dropArea != null)
            {
                //disabled drop location field
                EditorGUI.BeginDisabledGroup(true);
                myScript.dropLocation = (Transform)EditorGUILayout.ObjectField(myScript.dropLocation, typeof(Transform), true);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                //enabled drop location field
                myScript.dropLocation = (Transform)EditorGUILayout.ObjectField(myScript.dropLocation, typeof(Transform), true);
            }

            //center "or"
            EditorGUILayout.LabelField("or", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter }, GUILayout.MaxWidth(30));


            //drop area collider
            if (myScript.dropArea != null && myScript.dropLocation == null)
            {
                //enabled drop area field
                myScript.dropArea = (Collider)EditorGUILayout.ObjectField(myScript.dropArea, typeof(Collider), true);
            }
            else if (myScript.dropLocation != null)
            {
                //disabled drop area field
                EditorGUI.BeginDisabledGroup(true);
                myScript.dropArea = (Collider)EditorGUILayout.ObjectField(myScript.dropArea, typeof(Collider), true);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                //enabled drop area field
                myScript.dropArea = (Collider)EditorGUILayout.ObjectField(myScript.dropArea, typeof(Collider), true);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawOffset(DropScript myScript)
        {
            //if transform or area, draw offset
            if (myScript.dropLocation != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(100));
                myScript.dropOffset = EditorGUILayout.Vector3Field("", myScript.dropOffset);
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawThrowForce(DropScript myScript)
        {
            //throw force
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Throw Force", GUILayout.MaxWidth(100));
            myScript.throwForce = EditorGUILayout.Vector3Field("", myScript.throwForce);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRandomAngleArc(DropScript myScript)
        {
            //random angle arc
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Random Angle Arc", GUILayout.MaxWidth(100));
            myScript.randomAngleArc = EditorGUILayout.Slider(myScript.randomAngleArc, 0, 360);
            EditorGUILayout.EndHorizontal();
        }
    }

    #endif
}
