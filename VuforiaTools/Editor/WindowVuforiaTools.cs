
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class WindowVuforiaTools : EditorWindow
{
    //Vuforia Tools and Configuration
    private VuforiaToolsConfiguration vtc;
    private VuforiaTools vuforiaTools;
    string searchField;

    //Scroll Position Vectors
    Vector2 scrollPos;
    Vector2 scrollPos2;
    Vector2 scrollPos3;

    bool accountConnected = false;

    int selectedIndex = -1;
    List<int> selectedIndexes;
    DuplicatesList duplicatesList;

    [MenuItem("Window/Vuforia Tools Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(WindowVuforiaTools));
    }

    private void Awake()
    {
        selectedIndexes = new List<int>();
        duplicatesList = new DuplicatesList();

        vtc = AssetDatabase.LoadAssetAtPath("Assets/Resources/VuforiaToolsConfiguration.asset", typeof(VuforiaToolsConfiguration)) as VuforiaToolsConfiguration;
        vuforiaTools = new VuforiaTools();
        UpdateAccountSummary();
        if (vtc.accountSummary.result_code == "Success"){
            accountConnected = true;
        }else{
            accountConnected = false;
        }
    }

    private void RefreshList(){
        vuforiaTools.GetAllTargetNames();
        UpdateAccountSummary();
        vtc.SaveRecoData();
        vtc.ExportToCSV();
    }

    private void UpdateAccountSummary()
    {
        string accountSumString = vuforiaTools.GetAccountSummary();
        VuforiaAccountSummary accountSummary = JsonUtility.FromJson<VuforiaAccountSummary>(accountSumString);
        vtc.accountSummary = accountSummary;
    }

    public bool Listen()
    {
        if(Event.current.type == EventType.KeyDown){
            switch (Event.current.keyCode)
            {
                case KeyCode.DownArrow:
                    if (selectedIndex == vtc.targetSummaryList.Count-1)
                    {
                        selectedIndex = -1;
                    }
                    duplicatesList = new DuplicatesList();
                    selectedIndexes = new List<int>();
                    selectedIndex++;
                    selectedIndexes.Add(selectedIndex);

                    if (selectedIndex > -1)
                    {
                        if ((selectedIndex + 1) * 20 > (scrollPos.y + 300)) scrollPos = new Vector2(scrollPos.x, (selectedIndex + 1) * 20 + 150);
                        if ((selectedIndex) * 20 < (scrollPos.y)) scrollPos = new Vector2(scrollPos.x, (selectedIndex) * 20 - 150);
                    }
                    return true;
                case KeyCode.UpArrow:
                    if(selectedIndex<=0){
                        selectedIndex = vtc.targetSummaryList.Count;
                    }
                    duplicatesList = new DuplicatesList();
                    selectedIndexes = new List<int>();
                    selectedIndex--;
                    selectedIndexes.Add(selectedIndex);

                    if (selectedIndex > -1)
                    {
                        if ((selectedIndex + 1) * 20 > (scrollPos.y + 300)) scrollPos = new Vector2(scrollPos.x, (selectedIndex+1)*20 + 150);
                        if ((selectedIndex) * 20 < (scrollPos.y)) scrollPos = new Vector2(scrollPos.x, (selectedIndex) * 20 - 150);
                    }
                    return true;
                case KeyCode.Escape:
                    duplicatesList = new DuplicatesList();
                    selectedIndexes = new List<int>();
                    selectedIndex = -1;
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    void OnGUI()
    {
        if(!File.Exists(Application.dataPath + "/Resources/VuforiaToolsConfiguration.asset"))
        {
            accountConnected = false;
        }
        if (accountConnected){
            DisplayAccount();
        }
        else{
            DisplaySetupInstructions();
        }
    }

    void DisplayAccount(){
        //Listens for up&down arrows as well as the escape key, calls Listen() method
        if (Event.current.isKey && Listen() && selectedIndex>=0)
        {
            Event.current.Use();
            Repaint();
        }
        //sets default color.. this is mostly to set the color back to normal after making the red delete button
        Color color_default = GUI.backgroundColor;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Account Settings", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (GUILayout.Button("Refresh", GUILayout.Width(65)))
        {
            UpdateAccountSummary();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Label("Database Name: " + vtc.accountSummary.name, EditorStyles.label);
        GUILayout.Label("Active_Images: " + vtc.accountSummary.active_images, EditorStyles.label);
        GUILayout.Label("Inactive_Images: " + vtc.accountSummary.inactive_images, EditorStyles.label);
        GUILayout.Label("Failed_Images: " + vtc.accountSummary.failed_images, EditorStyles.label);

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Target List", EditorStyles.boldLabel);
        GUILayout.Space(10);
        if (GUILayout.Button("Refresh", GUILayout.Width(65)))
        {
            Debug.Log("The Refresh Button was pressed.");
            vuforiaTools = new VuforiaTools();
            RefreshList();
        }
        if (vtc.targetSummaryList.Count>0)
        {
            if (GUILayout.Button("Add New Target", GUILayout.Width(100)))
            {
                ScriptableWizard.DisplayWizard<AddVuforiaTarget>("Add Vuforia Target", "Finish", "Select Meta Data File");
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.Label("Searach Field", EditorStyles.label);
        GUILayout.BeginHorizontal();
        searchField = GUILayout.TextField(searchField, GUILayout.Width(200));
        if(GUILayout.Button("Clear", GUILayout.Width(60))){
            searchField = "";
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        //WideMode
        if (vtc.wideMode) GUILayout.BeginHorizontal();
        if (vtc.targetSummaryList.Count > 0)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));
            //Changes the color of the button if the button is a selected item
            SelectItem();
            GUILayout.EndScrollView();
        }
        //For displaying the target settings if the image target is selected and it's the only one selected
        if (selectedIndex >= 0 && selectedIndexes.Count==1)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("Target Settings", EditorStyles.boldLabel);
            GUILayout.Label("Name: " + vtc.targetSummaryList[selectedIndex].target_name, EditorStyles.label);
            GUILayout.Label("Target_ID: " + vtc.targetSummaryList[selectedIndex].target_id, EditorStyles.label);
            GUILayout.Label("Upload_Date: " + vtc.targetSummaryList[selectedIndex].upload_date, EditorStyles.label);
            if(vtc.targetSummaryList[selectedIndex].tracking_rating == -2) GUILayout.Label("Tracking_Rating: Failed, Delete and Retry", EditorStyles.label);
            else if (vtc.targetSummaryList[selectedIndex].tracking_rating == -1) GUILayout.Label("Tracking_Rating: Processing, Refresh in a bit", EditorStyles.label);
            else GUILayout.Label("Tracking_Rating: " + vtc.targetSummaryList[selectedIndex].tracking_rating, EditorStyles.label);
            GUILayout.Label("Active_Flag: " + vtc.targetSummaryList[selectedIndex].active_flag, EditorStyles.label);
            GUILayout.Label("Current_Month_Recos: " + vtc.targetSummaryList[selectedIndex].current_month_recos, EditorStyles.label);
            GUILayout.Label("Previous_Month_Recos: " + vtc.targetSummaryList[selectedIndex].previous_month_recos, EditorStyles.label);
            GUILayout.Label("Total_Recos: " + vtc.targetSummaryList[selectedIndex].total_recos, EditorStyles.label);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(65)))
            {
                vuforiaTools = new VuforiaTools();
                vuforiaTools.UpdateTargetInformation(vtc.targetSummaryList[selectedIndex].target_id);
                UpdateAccountSummary();
            }
            if (GUILayout.Button("Modify", GUILayout.Width(65)))
            {
                ScriptableWizard.DisplayWizard<ModifyVuforiaTarget>(vtc.targetSummaryList[selectedIndex].target_id, "Finish", "Select Meta Data File");
            }
            if (GUILayout.Button("Check Similar", GUILayout.Width(95)))
            {
                vuforiaTools = new VuforiaTools();
                string jsonString = vuforiaTools.CheckDuplicates(vtc.targetSummaryList[selectedIndex].target_id);
                duplicatesList = JsonUtility.FromJson<DuplicatesList>(jsonString);
            }
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete", GUILayout.Width(65)))
            {
                bool delete = EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the " + vtc.targetSummaryList[selectedIndex].target_name + " image target from your cloud database?", "Ok", "Cancel");
                if (delete)
                {
                    vuforiaTools = new VuforiaTools();
                    string result = vuforiaTools.DeleteTarget(vtc.targetSummaryList[selectedIndex].target_id);
                    if (result != "fail")
                    {
                        if (selectedIndex == vtc.targetSummaryList.Count - 1)
                        {
                            selectedIndex--;
                            selectedIndexes.Remove(selectedIndex + 1);
                            UpdateAccountSummary();
                        }
                        else
                        {
                            selectedIndexes.Remove(selectedIndex);
                            UpdateAccountSummary();
                        }
                    }
                    delete = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (selectedIndexes.Count == 1 && duplicatesList.similar_targets.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical();
                GUILayout.Label("Similar Image Targets", EditorStyles.boldLabel);
                scrollPos3 = GUILayout.BeginScrollView(scrollPos3, GUILayout.Height(200), GUILayout.Width(300));
                foreach (string duplicateID in duplicatesList.similar_targets)
                {
                    int index = vtc.targetSummaryList.FindIndex(item => item.target_id == duplicateID);
                    GUILayout.Label(vtc.targetSummaryList[index].target_name, EditorStyles.label);
                }
                GUILayout.EndScrollView();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
            }

        }
        //Displays a list of the currently selected images targets instead of the target summary if more than one image target is selected.
        else if (vtc.targetSummaryList.Count > 1 && selectedIndexes.Count > 1)
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("Selected Targets List (Batch Edit): " + selectedIndexes.Count + "/" + vtc.targetSummaryList.Count, EditorStyles.boldLabel);
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2,GUILayout.Height(selectedIndexes.Count<16?18*selectedIndexes.Count + 5:300), GUILayout.Width(300));
            foreach (int index in selectedIndexes)
            {
                GUILayout.Label(vtc.targetSummaryList[index].target_name + ", Curr: " + vtc.targetSummaryList[index].current_month_recos + ", Prev: " + vtc.targetSummaryList[index].previous_month_recos, EditorStyles.label);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Batch Deactivate", GUILayout.Width(100)))
            {
                vuforiaTools = new VuforiaTools();
                foreach (int index in selectedIndexes)
                {
                    vuforiaTools.UpdateImageTarget(vtc.targetSummaryList[index].target_id, null, 1, null, false, null);
                }
                UpdateAccountSummary();
            }
            if (GUILayout.Button("Batch Activate", GUILayout.Width(100)))
            {
                vuforiaTools = new VuforiaTools();
                foreach (int index in selectedIndexes)
                {
                    vuforiaTools.UpdateImageTarget(vtc.targetSummaryList[index].target_id, null, 1, null, true, null);
                }
                UpdateAccountSummary();
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Batch Delete", GUILayout.Width(100)))
            {
                bool delete = EditorUtility.DisplayDialog("Confirm Delete", "Are you sure you want to delete the selected image targets from your cloud database?", "Ok", "Cancel");
                if (delete)
                {
                    vuforiaTools = new VuforiaTools();
                    List<VtTargetSummary> summariesToDelete = new List<VtTargetSummary>();
                    foreach (int index in selectedIndexes)
                    {
                        summariesToDelete.Add(vtc.targetSummaryList[index]);
                    }
                    foreach (VtTargetSummary summary in summariesToDelete)
                    {
                        vuforiaTools.DeleteTarget(summary.target_id);
                        vtc.targetSummaryList.Remove(summary);
                    }
                    selectedIndexes = new List<int>();
                    selectedIndex = -1;
                    UpdateAccountSummary();
                }
            }
            GUI.backgroundColor = color_default;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        if(vtc.wideMode) GUILayout.EndHorizontal();
        GUI.backgroundColor = color_default;
    }

    void DisplaySetupInstructions(){
        GUILayout.Label("Please go to the Assets>VuforiaTools folder and add the Access Key and Secret Key that belongs to your Vuforia Cloud Account", EditorStyles.label);
        //AssetDatabase.LoadAssetAtPath("Assets/Resources/VuforiaToolsConfiguration.asset", typeof(VuforiaToolsConfiguration)) as VuforiaToolsConfiguration;
        if(File.Exists(Application.dataPath + "/Resources/VuforiaToolsConfiguration.asset"))
        {
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                Awake();
            }
        }
        else
        {
            if (GUILayout.Button("Create Config File", GUILayout.Width(100)))
            {
                CreateVuforiaToolsConfigurationFile.CreateConfigFile();
            }
        }

    }

    void SelectItem(){
        //Sets up button color and style options
        Color color_default = GUI.backgroundColor;
        Color text_default = GUI.contentColor;
        Color color_selected = new Color(0,0.5f,1.0f,1);

        GUIStyle itemStyle = new GUIStyle(GUI.skin.button);  //make a new GUIStyle
        itemStyle.fixedHeight = 20;

        itemStyle.alignment = TextAnchor.MiddleLeft; //align text to the left
        itemStyle.active.background = itemStyle.normal.background;  //gets rid of button click background style.
        itemStyle.margin = new RectOffset(0, 0, 0, 0); //removes the space between items (previously there was a small gap between GUI which made it harder to select a desired item)

        //Iterates through the targetsummary list
        for (int i = 0; i < vtc.targetSummaryList.Count; i++)
        {
            Color button_color;
            if(vtc.targetSummaryList[i].active_flag){
                button_color = new Color(0.95f, 0.95f, 0.95f, 1);
            }else{
                button_color = new Color(0.6f, 0.6f, 0.6f, 1);
            }
            Color text_color;
            if (vtc.targetSummaryList[i].tracking_rating == -1) text_color = new Color(0, 0.917f, 0.921f, 1);
            else if (vtc.targetSummaryList[i].tracking_rating == -2) text_color = new Color(0.921f, 0.105f, 0.039f, 1);
            else text_color = text_default;
            //Only displays the button if the search field is empty, or if the name of the target contains the letter string searched for.
            if (searchField == null || searchField == "" || vtc.targetSummaryList[i].target_name.ToLower().Contains(searchField.ToLower()))
            {
                GUI.contentColor = text_color;
                GUI.backgroundColor = (selectedIndexes.Contains(i)) ? color_selected : button_color;
                //show a button using the new GUIStyle
                if (GUILayout.Button(vtc.targetSummaryList[i].target_name, itemStyle))
                {
                    //If the item has already been selected, it will remove it from the list.
                    if (selectedIndex == i)
                    {
                        duplicatesList = new DuplicatesList();
                        selectedIndex = -1;
                        selectedIndexes.Remove(i);
                    }
                    //If the item has already been selected, it will remove it from the list.
                    else if (selectedIndexes.Contains(i))
                    {
                        duplicatesList = new DuplicatesList();
                        selectedIndexes.Remove(i);
                    }
                    else
                    {
                        //If the item isn't already selected, it will be added to the list.
                        if (selectedIndex == -1)
                        {
                            duplicatesList = new DuplicatesList();
                            selectedIndex = i;
                            selectedIndexes.Add(i);
                        }
                        //If the item isn't already selected, and either the command, control, or shift buttons are pressed, it will be added to the list of selected indexes.
                        else if (Event.current.modifiers == EventModifiers.Command || Event.current.modifiers == EventModifiers.Control || Event.current.modifiers == EventModifiers.Shift)
                        {
                            duplicatesList = new DuplicatesList();
                            selectedIndex = i;
                            selectedIndexes.Add(i);
                        }
                        //It will be selected, and the list of selected will be started over.
                        else
                        {
                            selectedIndex = i;
                            duplicatesList = new DuplicatesList();
                            selectedIndexes = new List<int>();
                            selectedIndexes.Add(i);
                        }

                        //Scrolls down the selected items list to always display the most recent item that has been added to the list.
                        if (selectedIndexes.Count > 15)
                        {
                            scrollPos2 = new Vector2(0, selectedIndexes.Count * 18);
                        }
                    }

                    //do something else (e.g ping an object)
                }
                GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list
                GUI.contentColor = text_default;
            }

        }
    }
}
