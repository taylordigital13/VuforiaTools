using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEditor;public class CreateVuforiaToolsConfigurationFile{    public static void CreateConfigFile(){        VuforiaToolsConfiguration asset = ScriptableObject.CreateInstance<VuforiaToolsConfiguration>();        AssetDatabase.CreateAsset(asset, "Assets/Resources/VuforiaToolsConfiguration.asset");        AssetDatabase.SaveAssets();        EditorUtility.FocusProjectWindow();        Selection.activeObject = asset;    }}