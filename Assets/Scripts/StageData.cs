using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StageDataSO : ScriptableObject
{
    public List<StageData> stageDatas;
}

[Serializable]
public class StageData
{
    [field : SerializeField]
    public List<int> beakerSize { get; private set; }
    [field: SerializeField]
    public List<string> beakerRGB { get; private set; }
    [field: SerializeField]
    public string answerBeaker {  get; private set; }
}
