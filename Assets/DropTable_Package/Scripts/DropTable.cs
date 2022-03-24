using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A scriptable object to store and slightly manage drop tables
/// </summary>
[Serializable]
public class DropTable : ScriptableObject
{
    //rarity colours
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    //public dictionary of rarity colours
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

    /// <summary>
    /// the objects that the table will drop, along with some options
    /// </summary>
    [Serializable]
    public class DropTableEntry
    {
        [SerializeField] public GameObject prefab = null;
        [SerializeField] public Rarity rarity = Rarity.Common;
        [SerializeField] public bool forced = false;
        [SerializeField] public int weight = 1;
        [SerializeField] public float effectiveChance = 1.0f;
        [SerializeField] public IntRange amountToDrop = new IntRange();
        [SerializeField] public int repetitionsAllowed = 1;
        [SerializeField] public bool unlimitedRepsAllowed = false;
        [SerializeField] public int totalReps = 0;
        [SerializeField] public bool showDropInTable = false;
    }


    [SerializeField] public List<DropTableEntry> dropList = new List<DropTableEntry>();


    //basic public functions
    public void AddEntry(DropTableEntry entry) {
        dropList.Add(entry);
    }

    public void RemoveEntry(DropTableEntry entry) {
        dropList.Remove(entry);
    }

    public void ClearEntries() {
        dropList.Clear();
    }

    /// <summary>
    /// Main way to access the drop table and get a list of items to drop (1 entire rep)
    /// </summary>
    public List<GameObject> GetDropPrefabs(){
        List<GameObject> prefabs = new List<GameObject>();

        //get whole list of drop entries
        List<DropTableEntry> entries = GetValidRepOfEntries();
        if (entries == null || entries.Count <= 0) return null;

        //copy out prefabs from all entires
        foreach (DropTableEntry entry in entries) {
            if (entry.prefab != null) {
                //get amount of prefabs per rep
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

    /// <summary>
    /// Gets all the entries that are valid for a single rep
    /// </summary>
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

    /// <summary>
    /// Gets all the forced entries that are valid for a single rep
    /// </summary>
    private List<DropTableEntry> GetAllValidForcedEntries() {
        List<DropTableEntry> forcedEntries = new List<DropTableEntry>();

        //find all forced entries
        foreach (DropTableEntry entry in dropList) {
            if (!CheckEntry(entry)) continue;

            if (entry.forced) {
                forcedEntries.Add(entry);
            }
        }

        return forcedEntries;
    }

    /// <summary>
    /// Gets a random entry that is valid for a single rep
    /// </summary>
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

    /// <summary>
    /// Checks if an entry is valid for a single rep
    /// </summary>
    private bool CheckEntry(DropTableEntry entry) {
        //check if entry is valid
        bool isValid = CheckIfEntryIsValid(entry);
        if (!isValid) return false;

        //check reps
        bool allowedRep = CheckIfAnotherRepAllowed(entry);
        if (!allowedRep) return false;

        return true;
    }

    /// <summary>
    /// Checks if an entry can do another rep
    /// </summary>
    private bool CheckIfAnotherRepAllowed(DropTableEntry entry) {
        if (entry.unlimitedRepsAllowed) return true;
        
        if (entry.totalReps >= entry.repetitionsAllowed) return false;

        return true;
    }

    /// <summary>
    /// Checks if an entry is valid
    /// </summary>
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

    /// <summary>
    /// Calculates the effective chance of an entry
    /// </summary>
    private void CalculateEffectiveChances() {
        //get total weight and valid entires
        int totalWeight = 0;
        foreach (DropTableEntry entry in dropList) {
            if (!CheckEntry(entry)) continue;
            if (entry.forced) continue;

            totalWeight += entry.weight;
        }

        //calculate effective chance
        foreach (DropTableEntry entry in dropList) {
            entry.effectiveChance = (float)entry.weight / (float)totalWeight;
            if (entry.forced) entry.effectiveChance = 1.0f;
        }
    }

    /// <summary>
    /// Sets reps done to 0
    /// </summary>
    public void ResetAllRepsDone() {
        foreach (DropTableEntry entry in dropList) {
            entry.totalReps = 0;
        }
    }

    #if UNITY_EDITOR

    /// <summary>
    /// Draws the drop table in the editor
    /// </summary>
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
            //styles
            boldCenterLabelStyle = new GUIStyle(EditorStyles.label);
            boldCenterLabelStyle.fontStyle = FontStyle.Bold;
            boldCenterLabelStyle.alignment = TextAnchor.MiddleCenter;

            boldLabelStyle = new GUIStyle(EditorStyles.label);
            boldLabelStyle.fontStyle = FontStyle.Bold;

            centerLabelStyle = new GUIStyle(EditorStyles.label);
            centerLabelStyle.alignment = TextAnchor.MiddleCenter;

            defaultBgColor = GUI.backgroundColor;

            //get drop table
            DropTable dropTable = (DropTable)target;
            
            //box around next elements
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;


            //drop list, foldable box
            showDropList = EditorGUILayout.Foldout(showDropList, new GUIContent("Drop List", "List of all the drops that can be dropped."));
            if (showDropList){
                //drop list
                for (int i = 0; i < dropTable.dropList.Count; i++) {
                    DropTableEntry entry = dropTable.dropList[i];

                    float tempWidth = EditorGUIUtility.currentViewWidth;

                    Color entryCol = RarityColors[entry.rarity];
                    GUI.backgroundColor = entryCol;

                    //drawing entry
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    GUI.backgroundColor = Color.white;

                        //horiz - drop and stats
                        EditorGUILayout.BeginHorizontal();
                            //drop
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * 0.5f;
                                entry.showDropInTable = EditorGUILayout.Foldout(entry.showDropInTable, new GUIContent("Drop", "Drop Prefab.\nClick arrow on left to expand/collapse!"));
                                entry.prefab = (GameObject)EditorGUILayout.ObjectField(entry.prefab, typeof(GameObject), false, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();

                            //forced
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 3.0f);
                                EditorGUILayout.LabelField(new GUIContent("Forced", "Forced entries will always drop"), boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                entry.forced = EditorGUILayout.Toggle(entry.forced, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();

                            //weight
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 2.0f);
                                EditorGUILayout.LabelField(new GUIContent("Weight", "Relates to the chance this entry is selected"), boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                entry.weight = EditorGUILayout.IntField(entry.weight, GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();
                            //effective chance
                            EditorGUILayout.BeginVertical();
                                tempWidth = (EditorGUIUtility.currentViewWidth - 20.0f) * (0.5f / 2.0f);
                                EditorGUILayout.LabelField(new GUIContent("Chance", "The effective chance an entry has of beng dropped per rep"), boldLabelStyle, GUILayout.MaxWidth(tempWidth));
                                EditorGUILayout.LabelField(new GUIContent((entry.effectiveChance * 100.0f).ToString("0.0") + "%", "The effective chance an entry has of beng dropped per rep"), GUILayout.MaxWidth(tempWidth));
                            EditorGUILayout.EndVertical();
                            //amount
                            GUILayout.FlexibleSpace();
                        
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        //only display if drop is open
                        if (entry.showDropInTable) {
                        // rarity
                        EditorGUILayout.LabelField(new GUIContent("Rarity", "Not actually used, just helps with visuals"));
                        entry.rarity = (Rarity)EditorGUILayout.EnumPopup(entry.rarity);

                        EditorGUILayout.Space();

                        //horiz - Amount
                        EditorGUILayout.BeginHorizontal();
                            tempWidth = (EditorGUIUtility.currentViewWidth - 50.0f);
                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField(new GUIContent("Rand Amount [min] [max]", "The amount of this prefab to drop if this entry is selected\n(min-max inclusive)"), boldLabelStyle , GUILayout.MaxWidth(tempWidth*0.8f));
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
                                EditorGUILayout.LabelField(new GUIContent("Repititions [total] [allowed]", "How many times this entry has been, and can be chosen"), boldLabelStyle , GUILayout.MaxWidth(tempWidth*0.8f));
                                EditorGUILayout.BeginHorizontal();
                                    entry.totalReps = EditorGUILayout.IntField(entry.totalReps, GUILayout.MaxWidth(100.0f));
                                    entry.repetitionsAllowed = EditorGUILayout.IntField(entry.repetitionsAllowed, GUILayout.MaxWidth(100.0f));
                                EditorGUILayout.EndHorizontal();
                                EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                                EditorGUILayout.LabelField(new GUIContent("Infinite?", "If true, this entry can be chosen an infinite amount of times"), boldLabelStyle, GUILayout.MaxWidth(tempWidth*0.2f));
                                entry.unlimitedRepsAllowed = EditorGUILayout.Toggle(entry.unlimitedRepsAllowed);
                            EditorGUILayout.EndVertical();
                            GUILayout.FlexibleSpace();
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                        }

                        //horiz - delete
                        EditorGUILayout.BeginHorizontal();
                            GUI.backgroundColor = Color.red;
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(new GUIContent("Delete", "Deletes this entry from the table"), GUILayout.MaxWidth(100.0f))) {
                                dropTable.dropList.RemoveAt(i);
                                i--;
                            }
                            GUI.backgroundColor = Color.white;
                        //end horiz
                        EditorGUILayout.EndHorizontal();

                    //end rect
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();

            //new entry button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent("Add New Entry", "Adds a new blank entry at the end of the table"))) {
                dropTable.dropList.Add(new DropTableEntry());
            }
            GUI.backgroundColor = Color.white;
                

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