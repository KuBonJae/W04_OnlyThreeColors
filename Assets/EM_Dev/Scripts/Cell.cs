using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Cell : MonoBehaviour
{
    private Image cellImage;
    private Color originalColor;

    void Start()
    {
        cellImage = GetComponent<Image>();
        originalColor = cellImage.color;

        GetComponent<Button>().onClick.AddListener(OnCellClicked);
    }


    void OnCellClicked()
    {
        cellImage.color = cellImage.color == originalColor ? Color.black : originalColor; //일단 클릭시 검정색으로 변하도록

    }
}
