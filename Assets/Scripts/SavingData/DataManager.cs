using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public string filePath;
    private int totalStageNum = 40;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "SaveData.json");
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void SaveData(List<List<int>> fromList, List<List<int>> toList, List<int> restartList, List<bool> isCleared)
    {
        DataWrapper data = new DataWrapper
        {
            fromList_tut = fromList[0],
            fromList_e1 = fromList[10],
            fromList_e2 = fromList[11],
            fromList_e3 = fromList[12],
            fromList_e4 = fromList[13],
            fromList_e5 = fromList[14],
            fromList_n1 = fromList[20],
            fromList_n2 = fromList[21],
            fromList_n3 = fromList[22],
            fromList_n4 = fromList[23],
            fromList_n5 = fromList[24],
            fromList_h1 = fromList[30],
            fromList_h2 = fromList[31],
            fromList_h3 = fromList[32],
            fromList_h4 = fromList[33],
            fromList_h5 = fromList[34],
            toList_tut = toList[0],
            toList_e1 = toList[10],
            toList_e2 = toList[11],
            toList_e3 = toList[12],
            toList_e4 = toList[13],
            toList_e5 = toList[14],
            toList_n1 = toList[20],
            toList_n2 = toList[21],
            toList_n3 = toList[22],
            toList_n4 = toList[23],
            toList_n5 = toList[24],
            toList_h1 = toList[30],
            toList_h2 = toList[31],
            toList_h3 = toList[32],
            toList_h4 = toList[33],
            toList_h5 = toList[34],
            restartList = restartList,
            isCleared = isCleared
        };
    
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(filePath, json);
    }
    
    public (List<List<int>> fromList, List<List<int>> toList,List<int> restartList, List<bool> isCleared) LoadData()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            DataWrapper data = JsonUtility.FromJson<DataWrapper>(json);
            //List<List<int>> fromList, List<List<int>> toList, List<int> restartList, List<bool> isCleared
            List<List<int>> fromList = new List<List<int>>();
            List<List<int>> toList = new List<List<int>>();
            for (int i = 0; i < totalStageNum; i++) 
            {
                fromList.Add(new List<int>());
                toList.Add(new List<int>());
            }
            // 이쁘게 데이터 가져오는 법 없나...
            // tutorial data
            fromList[0] = data.fromList_tut;
            toList[0] = data.toList_tut;
            // easy data
            fromList[10] = data.fromList_e1;
            fromList[11] = data.fromList_e2;
            fromList[12] = data.fromList_e3;
            fromList[13] = data.fromList_e4;
            fromList[14] = data.fromList_e5;
            toList[10] = data.toList_e1;
            toList[11] = data.toList_e2;
            toList[12] = data.toList_e3;
            toList[13] = data.toList_e4;
            toList[14] = data.toList_e5;
            // normal data
            fromList[20] = data.fromList_n1;
            fromList[21] = data.fromList_n2;
            fromList[22] = data.fromList_n3;
            fromList[23] = data.fromList_n4;
            fromList[24] = data.fromList_n5;
            toList[20] = data.toList_n1;
            toList[21] = data.toList_n2;
            toList[22] = data.toList_n3;
            toList[23] = data.toList_n4;
            toList[24] = data.toList_n5;
            // hard data
            fromList[30] = data.fromList_h1;
            fromList[31] = data.fromList_h2;
            fromList[32] = data.fromList_h3;
            fromList[33] = data.fromList_h4;
            fromList[34] = data.fromList_h5;
            toList[30] = data.toList_h1;
            toList[31] = data.toList_h2;
            toList[32] = data.toList_h3;
            toList[33] = data.toList_h4;
            toList[34] = data.toList_h5;
            //
            return (fromList, toList, data.restartList, data.isCleared);
        }
        else
            return (null, null, null, null);
    }
}
