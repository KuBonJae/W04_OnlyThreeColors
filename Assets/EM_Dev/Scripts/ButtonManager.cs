using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    Button[] buttons;

    [SerializeField]
    Button clearButton;

    public Transform gridParent;

    public Color[] colors;
    private Color selectedColor;

    private void Start()
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(()=> OnColorButton(index));
        }
        clearButton.onClick.AddListener(OnClearButton);
        //초기 색상
        selectedColor = colors[0];
    }
    void OnClearButton()
    {
        Debug.Log("초기화!!");
        foreach(Transform gridParent in gridParent)
        {
            Image cellImage = gridParent.GetComponent<Image>();
            if(cellImage!= null)
            {
                cellImage.color = Color.white;
            }
        }
    }
    void OnColorButton(int color)
    {
        selectedColor = colors[color];

    }

    public Color GetSelectedColor()
    {
        return selectedColor;
    }
}
