using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

//Drop table scriptable object + Editor script
[Serializable]
public class DropTable : ScriptableObject
{

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    //public dictionary of rarity colors
    public static Dictionary<Rarity, Color> RarityColors = new Dictionary<Rarity, Color>()
    {
        {Rarity.Common, new Color(1.0f, 1.0f, 1.0f)},
        {Rarity.Uncommon, new Color(0.1176471f, 1.0f, 0f)},
        {Rarity.Rare, new Color(0f, 0.4784314f, 0.8666667f)},
        {Rarity.Epic, new Color(0.6313726f, 0.2078431f, 0.9333333f)},
        {Rarity.Legendary, new Color(1f, 0.5019608f, 0f)}
    };

    //basic int range for easier editor customising
    [Serializable]
    public class IntRange {
        [SerializeField] public int min = 0;
        [SerializeField] public int max = 0;

        public IntRange(int min = 0, int max = 0) {
            this.min = min;
            this.max = max;
        }
    }


    //the objects that the table will drop, along with some options
    [Serializable]
    public class DropTableEntry
    {
        //NOTE: only allow either prefab or drop table, not both
        [SerializeField] public GameObject prefab = null;
        //[SerializeField] public DropTable dropTable = null;

        [SerializeField] public Rarity rarity = Rarity.Common;

        [SerializeField] public bool forced = false;
        //if not forced
        [SerializeField] public int weight = 1;
        [SerializeField] public float effectiveChance = 1.0f;
        [SerializeField] public IntRange amountToDrop = new IntRange();

        //NOTE MAKE SURE TO MAKE AN OPTION TO ENABLE/DISABLE ALL
        [SerializeField] public int repetitionsAllowed = 1;
        [SerializeField] public bool unlimitedRepsAllowed = false;
        [SerializeField] public int totalReps = 0;

        [SerializeField] public bool showDropInTable = false;
    }

    [SerializeField] public Rarity rarity = Rarity.Common;
    [SerializeField] public List<DropTableEntry> dropList = new List<DropTableEntry>();

    //[SerializeField] public int tableWeight = 1;

    public void AddEntry(DropTableEntry entry) {
        dropList.Add(entry);
    }

    public void RemoveEntry(DropTableEntry entry) {
        dropList.Remove(entry);
    }

    public void ClearEntries() {
        dropList.Clear();
    }

    public List<GameObject> GetDropPrefabs(){
        List<GameObject> prefabs = new List<GameObject>();

        List<DropTableEntry> entries = GetValidRepOfEntries();
        if (entries == null || entries.Count <= 0) return null;

        foreach (DropTableEntry entry in entries) {
            if (entry.prefab != null) {
                int randAmount = UnityEngine.Random.Range(entry.amountToDrop.min, entry.amountToDrop.max + 1);
                for (int i = 0; i < randAmount; i++) {
                    prefabs.Add(entry.prefab);
                }

                entry.totalReps++;
            }
        }

        if (prefabs.Count > 0) return prefabs;

        return null;
    }

    private List<DropTableEntry> GetValidRepOfEntries() {
        List<DropTableEntry> validEntries = new List<DropTableEntry>();

        //get all forced
        List<DropTableEntry> forcedEntries = GetAllValidForcedEntries();

        //get random
        DropTableEntry randomEntry = GetRandomValidNoForcedEntry();

        //add to list
        if (forcedEntries.Count > 0) {
            validEntries.AddRange(forcedEntries);
        }
        if (randomEntry != null) {
            validEntries.Add(randomEntry);
        }

        if (validEntries.Count > 0) return validEntries;

        return null;
    }

    private List<DropTableEntry> GetAllValidForcedEntries() {
        List<DropTableEntry> forcedEntries = new List<DropTableEntry>();

        foreach (DropTableEntry entry in dropList) {
            if (!CheckEntry(entry)) continue;

            if (entry.forced) {
                forcedEntries.Add(entry);
            }
        }

        return forcedEntries;
    }

    private DropTableEntry GetRandomValidNoForcedEntry() {
        List<DropTableEntry> validEntries = new List<DropTableEntry>();

        //get total weight and valid entires
        int totalWeight = 0;
        foreach (DropTableEntry entry in dropList) {
            if (!CheckEntry(entry)) continue;
            if (entry.forced) continue;

            validEntries.Add(entry);
            totalWeight += entry.weight;
        }

        //get random number
        int randomNumber = UnityEngine.Random.Range(0, totalWeight);

        //get random entry
        int currentWeight = 0;
        foreach (DropTableEntry entry in validEntries) {
            currentWeight += entry.weight;
            if (randomNumber < currentWeight) {
                return entry;
            }
        }

        return null;
    }

    private bool CheckEntry(DropTableEntry entry) {
        //check if entry is valid
        bool isValid = CheckIfEntryIsValid(entry);
        if (!isValid) return false;

        //check reps
        bool allowedRep = CheckIfAnotherRepAllowed(entry);
        if (!allowedRep) return false;

        return true;
    }

    private bool CheckIfAnotherRepAllowed(DropTableEntry entry) {
        if (entry.unlimitedRepsAllowed) return true;
        
        if (entry.totalReps >= entry.repetitionsAllowed) return false;

        return true;
    }

    private bool CheckIfEntryIsValid(DropTableEntry entry) {
        //check if entry is valid
        if (entry.prefab == null) return false;

        return true;
    }

    private void ValidateAmount(){
        foreach (DropTableEntry entry in dropList) {
            if (entry.amountToDrop.min < 0) entry.amountToDrop.min = 0;
            if (entry.amountToDrop.max < 0) entry.amountToDrop.max = 0;

            if (entry.amountToDrop.min > entry.amountToDrop.max) {
                entry.amountToDrop.max = entry.amountToDrop.min;
            }
        }
    }

    private void CalculateEffectiveChances() {
        //get total weight and valid entires
        int totalWeight = 0;
        foreach (DropTableEntry entry in dropList) {
            if (!CheckEntry(entry)) continue;
            if (entry.forced) continue;

            totalWeight += entry.weight;
        }

        foreach (DropTableEntry entry in dropList) {
            entry.effectiveChance = (float)entry.weight / (float)totalWeight;
            if (entry.forced) entry.effectiveChance = 1.0f;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DropTable))]
    public class DropTableEditor : Editor
    {
        //styles
        static GUIStyle boldCenterLabelStyle = new GUIStyle();
        static GUIStyle boldLabelStyle = new GUIStyle();
        static GUIStyle centerLabelStyle = new GUIStyle();
        static Color defaultBgColor = Color.white;

        static public bool showDropList = true;

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

            //DrawDefaultInspector();

            DropTable dropTable = (DropTable)target;

            //rarity
            //set color based on rarity
            Color color = RarityColors[dropTable.rarity];
            GUI.backgroundColor = color;
            dropTable.rarity = (Rarity)EditorGUILayout.EnumPopup("Table Rarity", dropTable.rarity);
            GUI.backgroundColor = Color.white;
            
            //box around next elements
            EditorGUI.indentLevel++;
            //GUI.backgroundColor = color;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            //NOTE PERHAPS ACTUALLY USE THE RECT TOOL HERE??

            //drop list, foldable box
            showDropList = EditorGUILayout.Foldout(showDropList, "Drop List");
            if (showDropList){
                //drop list
                for (int i = 0; i < dropTable.dropList.Count; i++) {
                    DropTableEntry entry = dropTable.dropList[i];

                    float tempWidth = EditorGUIUtility.currentViewWidth;

                    Color entryCol = RarityColors[entry.rarity];
                    GUI.backgroundColor = entryCol;

                    //draw rect
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    GUI.backgroundColor = Color.white;

                        //horiz - drop and stats
                        EditorGUILayout.BeginHorizontal();
                            //drop
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * 0.5f;
                                entry.showDropInTable = EditorGUILayout.Foldout(entry.showDropInTable, "Drop");
                                entry.prefab = (GameObject)EditorGUILayout.ObjectField(entry.prefab, typeof(GameObject), false, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();

                            //forced
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 3.0f);
                                EditorGUILayout.LabelField("Forced", boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                entry.forced = EditorGUILayout.Toggle(entry.forced, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();

                            //weight
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 2.0f);
                                EditorGUILayout.LabelField("Weight", boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                entry.weight = EditorGUILayout.IntField(entry.weight, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();
                            //effective chance
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 2.0f);
                                EditorGUILayout.LabelField("Chance", boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                EditorGUILayout.LabelField((entry.effectiveChance * 100.0f).ToString("0.0") + "%", GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();
                            //amount
                            GUILayout.FlexibleSpace();
                            
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        if (entry.showDropInTable) {

                        // rarity
                        EditorGUILayout.LabelField("Rarity");
                        entry.rarity = (Rarity)EditorGUILayout.EnumPopup(entry.rarity);

                        EditorGUILayout.Space();

                        //horiz - Amount
                        EditorGUILayout.BeginHorizontal();
                            tempWidth = (EditorGUIUtility.currentViewWidth - 50.0f);
                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField("Rand Amount [min][max]", boldLabelStyle , GUILayout.MaxWidth(tempWidth*0.8f));
                                EditorGUILayout.BeginHorizontal();
                                    entry.amountToDrop.min = EditorGUILayout.IntField(entry.amountToDrop.min, GUILayout.MaxWidth(100.0f));
                                    entry.amountToDrop.max = EditorGUILayout.IntField(entry.amountToDrop.max, GUILayout.MaxWidth(100.0f));

                                EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();    
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        //horiz - reps
                        EditorGUILayout.BeginHorizontal();
                            tempWidth = (EditorGUIUtility.currentViewWidth - 50.0f);
                            EditorGUILayout.BeginVertical();
                                EditorGUI.BeginDisabledGroup(entry.unlimitedRepsAllowed);
                                EditorGUILayout.LabelField("Repititions [total][allowed]", boldLabelStyle , GUILayout.MaxWidth(tempWidth*0.8f));
                                EditorGUILayout.BeginHorizontal();
                                    EditorGUI.BeginDisabledGroup(true); entry.totalReps = EditorGUILayout.IntField(entry.totalReps, GUILayout.MaxWidth(100.0f)); EditorGUI.EndDisabledGroup();
                                    entry.repetitionsAllowed = EditorGUILayout.IntField(entry.repetitionsAllowed, GUILayout.MaxWidth(100.0f));
                                EditorGUILayout.EndHorizontal();
                                EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField("Infinite?");
                                entry.unlimitedRepsAllowed = EditorGUILayout.Toggle(entry.unlimitedRepsAllowed);
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        }

                        //horiz - delete
                        EditorGUILayout.BeginHorizontal();
                            tempWidth = (EditorGUIUtility.currentViewWidth - 50.0f);
                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField("Delete", boldLabelStyle , GUILayout.MaxWidth(tempWidth*0.8f));
                                if (GUILayout.Button("Delete", GUILayout.MaxWidth(100.0f))) {
                                    dropTable.dropList.RemoveAt(i);
                                    i--;
                                }
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                    //end rect
                    EditorGUILayout.EndVertical();
                    
                }
            }

            EditorGUILayout.EndVertical();

            //on change save
            if (GUI.changed) {
                //calculate all effective chances
                dropTable.CalculateEffectiveChances();
                dropTable.ValidateAmount();

                EditorUtility.SetDirty(dropTable);
            }
        }
    }

    #endif
}