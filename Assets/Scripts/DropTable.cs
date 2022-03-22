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

        [SerializeField] public bool forced = false;
        //if not forced
        [SerializeField] public int weight = 1;
        [SerializeField] public IntRange amountToDrop = new IntRange();

        //NOTE MAKE SURE TO MAKE AN OPTION TO ENABLE/DISABLE ALL
        [SerializeField] public int repetitionsAllowed = 1;
         public int totalReps = 0;
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

        //if we get here, something went wrong
        Debug.Log("No valid non-forced drops found");
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
        if (entry.totalReps >= entry.repetitionsAllowed) return false;

        return true;
    }

    private bool CheckIfEntryIsValid(DropTableEntry entry) {
        //check if entry is valid
        if (entry.prefab == null) return false;

        return true;
    }

    #if UNITY_EDITOR



    #endif
}