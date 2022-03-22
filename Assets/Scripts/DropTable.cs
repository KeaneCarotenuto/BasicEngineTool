using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//Drop table scriptable object
[CreateAssetMenu(fileName = "DropTable", menuName = "DropTable", order = 1)]
public class DropTable : ScriptableObject
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #if UNITY_EDITOR
    [MenuItem("Assets/Create/DropTable")]
    public static void CreateDropTable()
    {
        ScriptableObjectUtility.CreateAsset("DropTable");
    }
    #endif
}
