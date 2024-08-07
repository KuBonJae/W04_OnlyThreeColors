using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    enum Type
    {
        Draw,
        Text
    }
    public GameObject cellPrefab;
    public int rows = 10;
    public int columns = 10;
    [SerializeField]
    Type cellType;

    public TMP_InputField[,] inputFields;

    List<TMP_InputField> inputFieldList = new List<TMP_InputField>();

    private int currentIndex = 0;
    void Start()
    {
        inputFields = new TMP_InputField[rows, columns];
        PopulateGrid();
        SetupTabOrder();
    }

    void PopulateGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject cell = Instantiate(cellPrefab, transform); // ��ü ����
                if (cell.TryGetComponent(out TMP_InputField inputField)) //try - get
                {
                    inputFields[i, j] = inputField;
                    Debug.Log("����");
                }
            }
        }
    }

    void SetupTabOrder()
    {
        for(int i = 0; i  < rows;i++)
        {
            for(int j = 0; j < columns;j++)
            {
                if (inputFields[i,j] != null)
                {
                    inputFieldList.Add(inputFields[i,j]);
                    Debug.Log("��ǲ�ʵ� ����");
                }
            }
        }
        //// �ʱ� �ε����� ����Ʈ�� ��ȿ�� ������ �ִ��� Ȯ��
        //if (inputFieldList.Count > 0)
        //{
        //    currentIndex = 0;
        //    inputFieldList[currentIndex].Select();
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)|| Input.GetKeyDown(KeyCode.LeftShift))
        {
            TMP_InputField currentField = EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>();

            if (inputFieldList.Count > 0 && currentField != null)
            {

                 currentIndex = inputFieldList.IndexOf(currentField); //indexof?
                int nexttIndex = 0;
                if (Input.GetKey(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
                {
                    if (currentIndex == 0) currentIndex = inputFieldList.Count;
                    nexttIndex = (currentIndex - 1) % inputFieldList.Count;

                    TMP_InputField nextField = inputFieldList[nexttIndex];
                    if (nextField != null)
                    {
                        nextField.Select();
                        nextField.ActivateInputField();
                        EventSystem.current.SetSelectedGameObject(nextField.gameObject); // EventSystem�� ���õ� ���� ������Ʈ ����
                    }
                }
                else if(Input.GetKey(KeyCode.Tab))
                {
                    nexttIndex = (currentIndex + 1) % inputFieldList.Count;

                    TMP_InputField nextField = inputFieldList[nexttIndex];
                    if (nextField != null)
                    {
                        nextField.Select();
                        nextField.ActivateInputField();
                        EventSystem.current.SetSelectedGameObject(nextField.gameObject); // EventSystem�� ���õ� ���� ������Ʈ ����
                    }
                }
               
                
            }
        }
    }
}
