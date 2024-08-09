using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Cell : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Image cellImage;
    private Color originalColor;
    private Color prevColor;
    private ButtonManager buttonManager;

    private bool isDragging = false;
    private Color changeColor;

    void Start()
    {
        cellImage = GetComponent<Image>();
        originalColor = Color.white;
        buttonManager = FindObjectOfType<ButtonManager>();
        //GetComponent<Button>().onClick.AddListener(OnCellClicked);
    }
    //void OnCellClicked()
    //{
    //    changeColor = buttonManager.GetSelectedColor();
    //    changeColor.a = 1;
        
        
    //    cellImage.color = cellImage.color == changeColor ? originalColor : changeColor; 

    //}
    public void OnPointerDown(PointerEventData eventData)
    {
        #region 1차 수정 : 그림판 우클릭 삭제
        if (eventData.button == PointerEventData.InputButton.Right)
            return; // 이쪽에서 우클릭 처리 안함
        #endregion
        isDragging = true;
        changeColor = buttonManager.GetSelectedColor();
        changeColor.a = 1;
        UpdateCellColor(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(isDragging)
        {
            UpdateCellColor(eventData);
            
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging= false;
    }
    private void UpdateCellColor(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        foreach (Transform child in transform.parent)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform.rect.Contains(localPoint - (Vector2)rectTransform.localPosition))
            {
                Image childImage = child.GetComponent<Image>();
                childImage.color = changeColor;

            }
        }
    }
}
