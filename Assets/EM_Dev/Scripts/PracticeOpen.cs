using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PracticeOpen : MonoBehaviour
{
    [SerializeField]
    Canvas practiceCanvas;
    bool isOpen = false;
    public void OnPractice()
    {
        isOpen = !isOpen;
        practiceCanvas.gameObject.SetActive(isOpen);
    }
}
