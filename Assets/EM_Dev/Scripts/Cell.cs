using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Cell : MonoBehaviour
{
    private Image cellImage;
    private Color originalColor;
    private Color prevColor;
    private ButtonManager buttonManager;

    void Start()
    {
        cellImage = GetComponent<Image>();
        originalColor = Color.white;
        buttonManager = FindObjectOfType<ButtonManager>();
        GetComponent<Button>().onClick.AddListener(OnCellClicked);
    }

    void OnCellClicked()
    {
        Color changeColor = buttonManager.GetSelectedColor();
        changeColor.a = 1;
        
        
        cellImage.color = cellImage.color == changeColor ? originalColor : changeColor; 

    }
}
