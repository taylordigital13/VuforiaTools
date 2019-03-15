using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class VuforiaToolsConfiguration : ScriptableObject
{
    [Header("Server Access Keys")]
    public string accessKey = "";
    public string secretKey = "";

    [Header("Window Settings")]
    public bool wideMode;

    [HideInInspector]
    public VuforiaAccountSummary accountSummary;

    [HideInInspector]
    public List<VtTargetSummary> targetSummaryList = new List<VtTargetSummary>();

    // Saves the data for each Image target in json format. If the file already exists, it loads that data, then overwrites the file.
    public void SaveRecoData(){
        string filePath = Application.dataPath + "/Resources/VuforiaToolsData/" + accountSummary.name + "RecoDataSave.txt";
        if(!File.Exists(filePath)){
            Directory.CreateDirectory(Application.dataPath + "/Resources/VuforiaToolsData/");
            foreach (VtTargetSummary target in targetSummaryList)
            {
                target.recoRecords = new List<RecoRecord>();
                RecoRecord record = new RecoRecord();
                record.date = Convert.ToInt32(DateTime.Now.Date.Year.ToString().PadLeft(4,'0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0'));
                record.recoCount = target.total_recos;
                target.recoRecords.Add(record);
            }
        }
        else{
            VtTargetSummaryExportData importedData = JsonUtility.FromJson<VtTargetSummaryExportData>(File.ReadAllText(filePath));
            foreach(VtTargetSummary target in targetSummaryList){
                int index = importedData.targets.FindIndex(item => item.target_id == target.target_id);
                if(index>=0){
                    target.recoRecords = importedData.targets[index].recoRecords;
                    List<RecoRecord> recordsToRemove = new List<RecoRecord>();
                    foreach(RecoRecord recoRecord in target.recoRecords){
                        string nowDate = DateTime.Now.Year.ToString().PadLeft(4,'0') + DateTime.Now.Month.ToString().PadLeft(2,'0') + DateTime.Now.Day.ToString().PadLeft(2,'0');
                        if(nowDate.Equals(recoRecord.date.ToString())){
                            recordsToRemove.Add(recoRecord);
                        }
                    }
                    foreach(RecoRecord recoRecord in recordsToRemove){
                        target.recoRecords.Remove(recoRecord);
                    }
                }
                else{
                    target.recoRecords = new List<RecoRecord>();
                }
                RecoRecord record = new RecoRecord();
                record.date = Convert.ToInt32(DateTime.Now.Date.Year.ToString().PadLeft(4, '0') + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0'));
                record.recoCount = target.total_recos;
                target.recoRecords.Add(record);

            }
        }
        VtTargetSummaryExportData exportData = new VtTargetSummaryExportData();
        exportData.targets = targetSummaryList;
        string jsonExport = JsonUtility.ToJson(exportData);
        File.WriteAllText(filePath, jsonExport);
    }

    //Reads in the json data for the image targets and generates a .csv file for data
    public void ExportToCSV(){
        string importFilePath = Application.dataPath + "/Resources/VuforiaToolsData/" + accountSummary.name + "RecoDataSave.txt";
        VtTargetSummaryExportData importedData = JsonUtility.FromJson<VtTargetSummaryExportData>(File.ReadAllText(importFilePath));
        string csvFilePath = Application.dataPath + "/Resources/VuforiaToolsData/" + accountSummary.name + "RecoDataSave.csv";
        StreamWriter writer = new StreamWriter(csvFilePath);
        StringBuilder sb = new StringBuilder();
        List<int> dates = new List<int>();
        sb.Append("Date");
        foreach(VtTargetSummary target in importedData.targets){
            sb.Append("," + target.target_name);
            foreach(RecoRecord record in target.recoRecords){
                if(!dates.Contains(record.date)){
                    dates.Add(record.date);
                }
            }
        }
        writer.WriteLine(sb.ToString());
        sb.Clear();
        dates.Sort((a, b) => -1 * a.CompareTo(b));
        bool hasDate = false;
        foreach(int date in dates){
            sb.Append(date.ToString());
            foreach (VtTargetSummary target in importedData.targets)
            {
                foreach(RecoRecord record in target.recoRecords){
                    if (record.date == date)
                    {
                        sb.Append("," + record.recoCount);
                        hasDate = true;
                    }
                }
                if(!hasDate){
                    sb.Append(",N/A");
                }
                else{
                    hasDate = false;
                }

            }
            writer.WriteLine(sb.ToString());
            sb.Clear();
        }
        writer.Flush();
        writer.Close();

        AssetDatabase.Refresh();
        ExportTransposeCSV();
    }

    //Reads in the .csv that has been created, then transposes that data and saves it as another .csv file
    public void ExportTransposeCSV()
    {
        //Reads in the file and gets a count of how many lines are in the text document.
        string csvFilePath = Application.dataPath + "/Resources/VuforiaToolsData/" + accountSummary.name + "RecoDataSave.csv";
        string[,] cellsArray = new string[0,0];
        string line;
        int lineCount = 0;
        StreamReader file = new StreamReader(csvFilePath);
        while ((line = file.ReadLine()) != null)
        {
            lineCount++;
        }
        file.Close();
        //Reads the file again in order to construct a 2d matrix from the lines of the text file.
        file = new StreamReader(csvFilePath);
        int iterCount = 0;
        bool firstRow = true;
        while ((line = file.ReadLine()) != null)
        {
            string[] row = line.Split(',');
            if (firstRow)
            {
                cellsArray = new string[lineCount, row.Length];
                firstRow = false;
            }


            for(int i = 0; i < row.Length; i++)
            {
                cellsArray[iterCount, i] = row[i];
            }
            iterCount++;
        }

        int rowCount = cellsArray.GetLength(0);
        int columnCount = cellsArray.GetLength(1);
        string transposePath = Application.dataPath + "/Resources/VuforiaToolsData/transpose" + accountSummary.name + "RecoDataSave.csv";
        StreamWriter writer = new StreamWriter(transposePath);
        StringBuilder sb = new StringBuilder();
        //Creates a transpose matrix from the regular matrix
        for(int i = 0; i < columnCount; i++)
        {
            for(int j = 0; j<rowCount; j++)
            {
                sb.Append(cellsArray[j, i] + ",");
            }
            sb.Remove(sb.Length - 1, 1);
            writer.WriteLine(sb.ToString());
            sb.Clear();
        }


        writer.Flush();
        writer.Close();
        AssetDatabase.Refresh();
    }
}
