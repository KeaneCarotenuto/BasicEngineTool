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
        [SerializeField] public GameObject prefab = null;
        [SerializeField] bool forced = false;
        [SerializeField] public int weight = 1;
        [SerializeField] public IntRange amountToDrop = new IntRange();
        [SerializeField] public bool allowDuplicates = true;

        public DropTableEntry(GameObject prefab = null, int weight = 0, bool forced = false, IntRange amountRange = null) {
            this.prefab = prefab;
            this.weight = weight;
            this.forced = forced;
            this.amountToDrop = amountRange;
        }
    }

    [SerializeField] public Rarity rarity = Rarity.Common;
    [SerializeField] public List<DropTableEntry> dropList = new List<DropTableEntry>();
    [SerializeField] public IntRange numberOfDrops = new IntRange();
    [SerializeField] public bool allowDuplicates = true;

    [SerializeField] public int tableWeight = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #if UNITY_EDITOR



    #endif
}
