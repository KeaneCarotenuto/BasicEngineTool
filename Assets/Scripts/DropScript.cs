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
    [SerializeField] public bool unlimitedRepsAllowed = false;
    [SerializeField] public int totalReps = 0;
    //if multiple {
    [SerializeField] public float dropDelay = 0.0f;
    private float dropDelayTimer = 0.0f;
    //}

    //events
    [SerializeField] public UnityEvent onPrefabDropped = new UnityEvent();
    [SerializeField] public UnityEvent onSuccessfulDropQueued = new UnityEvent();
    [SerializeField] public UnityEvent onFirstDropQueued = new UnityEvent();
    [SerializeField] public UnityEvent onLastDropQueued = new UnityEvent();
    [SerializeField] public UnityEvent onFailedDropQueued = new UnityEvent();

    private List<GameObject> toDrop = new List<GameObject>();

    public bool debugMode = false;
    public List<Vector3> debugDropLocations = new List<Vector3>();
    public int debugLocaitonCount = 5000;

    private void OnDrawGizmosSelected() {
        if (!DropScriptEditor.isEditingForce) return;

        //draw force and cone
        //calculate position
        Vector3 dropPos = Vector3.zero;
        if (dropLocation != null) {
            dropPos = dropLocation.position;
            dropPos += dropOffset;
        }
        else if (dropArea != null) {
            //get random point within area
            Vector3 randomPoint = dropArea.bounds.center;

            dropPos = randomPoint;
        }
        else {
            dropPos = transform.position;
        }

        Vector3 throwDir = throwForce;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(dropPos, dropPos + throwDir);
        //Gizmos.DrawWireSphere(dropPos, throwDir.magnitude/10.0f);
        //Gizmos.DrawWireSphere(dropPos + throwDir, throwDir.magnitude/10.0f);
        //Gizmos.DrawWireSphere(dropPos, throwDir.magnitude);
        if (debugLocaitonCount < 0) debugLocaitonCount = 0;
        if (debugLocaitonCount > 10000) debugLocaitonCount = 10000;
        debugDropLocations.Add(dropPos + RandomPointOnSphereRandomAngle(throwDir, randomAngleArc));
        while (debugDropLocations.Count > debugLocaitonCount) {
            debugDropLocations.RemoveAt(0);
        }


        //draw drop locations as spheres
        Gizmos.color = Color.red;
        foreach (Vector3 pos in debugDropLocations) {
            Gizmos.DrawSphere(pos, 0.1f);
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

        debugMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        //use drop delay to drop items
        if (toDrop.Count > 0)
        {
            dropDelayTimer += Time.deltaTime;
            if (dropDelayTimer >= dropDelay)
            {
                dropDelayTimer = 0.0f;
                DropObject(toDrop[0]);
                toDrop.RemoveAt(0);
            }
        }
    }

    //probably have to hook this up to update loop to do it with delays
    public void QueueDrop(){
        if (!CheckTable(dropTable)) goto fail;

        List<GameObject> dropsToAdd = dropTable.GetDropPrefabs();
        if (dropsToAdd == null || dropsToAdd.Count <= 0) goto fail;

        //add all drops to list
        foreach (GameObject drop in dropsToAdd)
        {
            if (drop == null) continue;
            toDrop.Add(drop);
        }

        totalReps++;
        if (totalReps == 1) onFirstDropQueued.Invoke();
        else if (!unlimitedRepsAllowed && totalReps == repetitionsAllowed) onLastDropQueued.Invoke();
        onSuccessfulDropQueued.Invoke();
        return;

        fail: 
        onFailedDropQueued.Invoke();
        return;
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

        onPrefabDropped.Invoke();
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
        if (!unlimitedRepsAllowed && totalReps >= repetitionsAllowed)
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

    //custom editor
    [CustomEditor(typeof(DropScript))]
    public class DropScriptEditor : Editor
    {
        static Quaternion tempRot = Quaternion.identity;
        static public bool isEditingForce = false;
        static public bool showEvents = false;

        //styles
        static GUIStyle boldCenterLabelStyle = new GUIStyle();
        static GUIStyle boldLabelStyle = new GUIStyle();
        static GUIStyle centerLabelStyle = new GUIStyle();
        static Color defaultBgColor = Color.white;

        public override void OnInspectorGUI()
        {
            boldCenterLabelStyle = new GUIStyle(EditorStyles.label);
            boldCenterLabelStyle.fontStyle = FontStyle.Bold;
            boldCenterLabelStyle.alignment = TextAnchor.MiddleCenter;

            boldLabelStyle = new GUIStyle(EditorStyles.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;

            centerLabelStyle = new GUIStyle(EditorStyles.label);
            centerLabelStyle.alignment = TextAnchor.MiddleCenter;

            defaultBgColor = GUI.backgroundColor;

            DropScript myScript = (DropScript)target;

            //DrawDefaultInspector();

            //create coloured box around next elements
            GUI.backgroundColor = Color.blue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = defaultBgColor;

            //Custom display starts here
            DrawDropTable(myScript);

            EditorGUILayout.Space();

            //reps
            DrawReps(myScript);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            
            DrawLocationSettings(myScript);

            EditorGUILayout.Space();

            //events
            DrawEvents(myScript);



            //on change, save
            if (GUI.changed)
            {
                myScript.debugDropLocations.Clear();
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawEvents(DropScript myScript)
        {
            //red box around next elements
            GUI.backgroundColor = Color.red;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = defaultBgColor;

            //create serialized object for events
            SerializedObject so = new SerializedObject(myScript);
            SerializedProperty prefabDropped = so.FindProperty("onPrefabDropped");
            SerializedProperty sucDropQue = so.FindProperty("onSuccessfulDropQueued");
            SerializedProperty firstDropQue = so.FindProperty("onFirstDropQueued");
            SerializedProperty lastDropQue = so.FindProperty("onLastDropQueued");
            SerializedProperty failDropQue = so.FindProperty("onFailedDropQueued");


            //foldable header
            showEvents = EditorGUILayout.Foldout(showEvents, "Events");
            if (showEvents)
            {
                EditorGUILayout.PropertyField(prefabDropped, true);
                EditorGUILayout.PropertyField(sucDropQue, true);
                EditorGUILayout.PropertyField(firstDropQue, true);
                EditorGUILayout.PropertyField(lastDropQue, true);
                EditorGUILayout.PropertyField(failDropQue, true);
            }

            EditorGUILayout.EndVertical();

            so.ApplyModifiedProperties();

        }

        private void DrawReps(DropScript myScript)
        {
            EditorGUILayout.LabelField("Repetitions", boldCenterLabelStyle);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            //total
            EditorGUILayout.LabelField("Total Done", GUILayout.MaxWidth(65));
            myScript.totalReps = EditorGUILayout.IntField(myScript.totalReps, GUILayout.MaxWidth(100));
            //allowed
            EditorGUI.BeginDisabledGroup(myScript.unlimitedRepsAllowed);
            EditorGUILayout.LabelField("Allowed", GUILayout.MaxWidth(50));
            myScript.repetitionsAllowed = EditorGUILayout.IntField(myScript.repetitionsAllowed, GUILayout.MaxWidth(100));
            EditorGUI.EndDisabledGroup();
            //unlimited
            EditorGUILayout.LabelField("Unlimited", GUILayout.MaxWidth(60));
            myScript.unlimitedRepsAllowed = EditorGUILayout.Toggle(myScript.unlimitedRepsAllowed, GUILayout.MaxWidth(20));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawLocationSettings(DropScript myScript)
        {
            //create coloured box around next elements
            GUI.backgroundColor = Color.green;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = defaultBgColor;

            //indent
            EditorGUI.indentLevel++;

            //foldout for location and offset
            EditorGUILayout.LabelField("Drop Settings", boldCenterLabelStyle);

            DrawLocation(myScript);
            DrawOffset(myScript);
            DrawThrowForce(myScript);

            if (myScript.throwForce.magnitude > 0)
            {
                DrawRandomAngleArc(myScript);
            }

            DrawEditThrowButton(myScript);

            EditorGUILayout.Space();

            DrawDelay(myScript);

            EditorGUILayout.EndVertical();
        }

        private static void DrawDelay(DropScript myScript)
        {
            //delay
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Delay", GUILayout.MaxWidth(100));
            myScript.dropDelay = EditorGUILayout.FloatField(myScript.dropDelay);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawEditThrowButton(DropScript myScript)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //button to toggle isEditingForce, if true, make button green
            GUI.backgroundColor = isEditingForce ? Color.green : Color.white;
            if (GUILayout.Button("Toggle Edit Throw Vector"))
            {
                isEditingForce = !isEditingForce;
                if (!isEditingForce)
                {
                    //round throwforce to nearest 0.001
                    myScript.throwForce = new Vector3(Mathf.Round(myScript.throwForce.x * 100) / 100, Mathf.Round(myScript.throwForce.y * 100) / 100, Mathf.Round(myScript.throwForce.z * 100) / 100);
                }
                else
                {
                    //focus on throw force
                    //SceneView.lastActiveSceneView.LookAt(myScript.transform.position + myScript.throwForce, SceneView.lastActiveSceneView.rotation);
                }
                //make temprot equal to current throw force as Angle
                tempRot = Quaternion.LookRotation(myScript.throwForce);

                SceneView.RepaintAll();
            }
            GUI.backgroundColor = defaultBgColor;
            //debugLocaitonCount
            if (isEditingForce){
                EditorGUILayout.LabelField("Debug Arc Points", GUILayout.MaxWidth(150));
                myScript.debugLocaitonCount = EditorGUILayout.IntField(myScript.debugLocaitonCount, GUILayout.MaxWidth(100));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI() {
            //draw throw force line with handle
            DropScript myScript = (DropScript)target;

            //calculate position
            Vector3 dropPos = Vector3.zero;
            if (myScript.dropLocation != null) {
                dropPos = myScript.dropLocation.position;
                dropPos += myScript.dropOffset;
            }
            else if (myScript.dropArea != null) {
                //get random point within area
                Vector3 centerPoint = myScript.dropArea.bounds.center;

                dropPos = centerPoint;
            }
            else {
                dropPos = myScript.transform.position;
            }

            Vector3 start = dropPos;

            //if this object is selected, draw force line
            if (Selection.activeGameObject == myScript.gameObject)
            {
                //draw line
                Handles.color = Color.green;
                Handles.DrawLine(start, myScript.throwForce + start);
                //draw sphere at start and end
                Handles.SphereHandleCap(0, start, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.SphereHandleCap(0, myScript.throwForce + start, Quaternion.identity, 0.1f, EventType.Repaint);
            }

            if (isEditingForce && myScript.throwForce.magnitude > 0)
            {
                myScript.debugMode = true;

                //select hand tool
                Tools.current = Tool.View;

                Handles.color = Color.green;
                Handles.DrawLine(start, myScript.throwForce + start);

                //magnitude
                float val = Handles.ScaleValueHandle(myScript.throwForce.magnitude, start + myScript.throwForce, Quaternion.LookRotation(myScript.throwForce), 2.0f, Handles.ConeHandleCap, 1.0f);
                myScript.throwForce = myScript.throwForce.normalized * val;

                //rotation
                tempRot = Handles.RotationHandle(tempRot, start);
                myScript.throwForce = (tempRot * Vector3.forward).normalized * myScript.throwForce.magnitude;
            }
            else{
                myScript.debugMode = false;
            }

            //if changes made, save them
            if (GUI.changed)
            {
                myScript.debugDropLocations.Clear();
                EditorUtility.SetDirty(myScript);
            }
        }

        private static void DrawDropTable(DropScript myScript)
        {
            //center aligned label with bold font "Drop Table"
            EditorGUILayout.LabelField("Drop Table", boldCenterLabelStyle);

            EditorGUILayout.BeginHorizontal();
            //drop table field
            myScript.dropTable = (DropTable)EditorGUILayout.ObjectField(myScript.dropTable, typeof(DropTable), true);

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawLocation(DropScript myScript)
        {
            //center aligned label with bold font "Drop Location"
            
            //two options for drop location side by side, transofrm or collider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Location", "Here is a tooltip"), boldLabelStyle, GUILayout.MaxWidth(100));

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
            //begin disabled group
            EditorGUI.BeginDisabledGroup(isEditingForce);

            //throw force
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Throw Force", GUILayout.MaxWidth(100));
            myScript.throwForce = EditorGUILayout.Vector3Field("", myScript.throwForce);
            myScript.throwForce = new Vector3(Mathf.Round(myScript.throwForce.x * 100) / 100, Mathf.Round(myScript.throwForce.y * 100) / 100, Mathf.Round(myScript.throwForce.z * 100) / 100);
            EditorGUILayout.EndHorizontal();

            //end disabled group
            EditorGUI.EndDisabledGroup();
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