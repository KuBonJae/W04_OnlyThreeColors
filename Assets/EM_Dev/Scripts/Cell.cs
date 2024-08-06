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
        cellImage.color = cellImage.color == originalColor ? Color.black : originalColor; //�ϴ� Ŭ���� ���������� ���ϵ���

    }
}
