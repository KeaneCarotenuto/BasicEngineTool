using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.IO;
#endif

public static class DropTableEditor
{
    #if UNITY_EDITOR

    //Toolbar Button: Create New Drop Table
    [MenuItem("DropTable/Create New")]
    public static void CreateDropTable()
    {
        Debug.Log("CreateDropTable");

        string assetType = "DropTable";
        string path = EditorPrefs.GetString("DropTablePath");

        //if path does not exist, create it
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            //refresh the asset database
            AssetDatabase.Refresh();
        }

        ScriptableObject asset = ScriptableObject.CreateInstance(assetType);

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetType + ".asset");

        Debug.Log("assetPathAndName " + assetType);

        ProjectWindowUtil.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        //start editing name right away
        Selection.activeObject.name = "NewTable";
    }

    [MenuItem("DropTable/ ")]

    //Toolbar Button: Open unity preferences
    [MenuItem("DropTable/Edit Settings")]
    public static void OpenPreferences()
    {
        SettingsService.OpenUserPreferences("Preferences/DropTable");
    }

    //Prefs Option: default location
    [PreferenceItem("DropTable")]
    public static void PreferencesGUI()
    {
        //display the default location
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

            Debug.Log("New DropTable Path: " + path);
        }
        else {
            Debug.LogError("Path not valid!");
        }
    }

    #endif
}
