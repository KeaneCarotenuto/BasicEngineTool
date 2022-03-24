using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.IO;
#endif

/// <summary>
/// A class to manage drop table menu stuff
/// </summary>
public static class DropTableEditor
{
    #if UNITY_EDITOR

    /// <summary>
    /// The menu item to create a new drop table, places it in default location
    /// </summary>
    [MenuItem("DropTable/Create New")]
    public static void CreateDropTable()
    {
        string assetType = "DropTable";

        //get the default path
        string path = EditorPrefs.GetString("DropTablePath");

        //if path does not exist, create it
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            //refresh the asset database
            AssetDatabase.Refresh();
        }

        //make the asset
        ScriptableObject asset = ScriptableObject.CreateInstance(assetType);

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + assetType + ".asset");

        //save the asset
        ProjectWindowUtil.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;

        //start editing
        Selection.activeObject.name = "NewTable";
    }

    //a list of all the drop tables :)
    [MenuItem("DropTable/ ")]

    /// <summary>
    /// A menu item to show the settings for the drop table package
    /// </summary>
    [MenuItem("DropTable/Edit Settings")]
    public static void OpenPreferences()
    {
        SettingsService.OpenUserPreferences("Preferences/DropTable");
    }

    /// <summary>
    /// Preferences for the drop table package
    /// </summary>
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

    /// <summary>
    /// A method to change the default location for drop tables
    /// </summary>
    public static void ChangeDefaultLocation()
    {
        //get path from user
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
