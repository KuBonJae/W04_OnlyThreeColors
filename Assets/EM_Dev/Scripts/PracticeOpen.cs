using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PracticeOpen : MonoBehaviour
{
    [SerializeField]
    Canvas practiceCanvas;

    public void OnPractice()
    {
        
        practiceCanvas.gameObject.SetActive(!practiceCanvas.gameObject.activeSelf);
    }
}
