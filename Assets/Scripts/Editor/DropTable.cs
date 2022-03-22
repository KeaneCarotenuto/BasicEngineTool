using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//Drop table scriptable object + Editor script
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

    //Toolbar Button: Create New Drop Table
    [MenuItem("DropTable/Create New")]
    public static void CreateDropTable()
    {
        Debug.Log("CreateDropTable");
        ScriptableObjectUtility.CreateAsset("DropTable", EditorPrefs.GetString("DropTablePath"));
    }

    //Prefs Option: default location
    [PreferenceItem("DropTable")]
    public static void PreferencesGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Default Location:", GUILayout.Width(150));
        EditorGUILayout.LabelField(EditorPrefs.GetString("DropTablePath", "Assets/Resources/DropTables"));

        //button to select new default location
        if (GUILayout.Button("Change Default Location"))
        {
            ChangeDefaultLocation();
        }
        EditorGUILayout.EndHorizontal();
    }

    //Choose New Location for DropTables
    public static void ChangeDefaultLocation()
    {
        Debug.Log("ChangeDefaultLocation");
        string path = EditorUtility.OpenFolderPanel("Select Folder", EditorPrefs.GetString("DropTablePath"), "");
        //shorten to only after project folder
        if (path.Contains(Application.dataPath))
        {
            path = path.Substring(Application.dataPath.Length - 6);
        }
        else {
            path = "" ;
        }

        if (path.Length != 0)
        {
            //save asset path
            EditorPrefs.SetString("DropTablePath", path);
        }
        else {
            Debug.LogError("Path not valid!");
        }
    }

    #endif
}
