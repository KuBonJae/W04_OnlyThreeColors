using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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

    void Start()
    {
        inputFields = new TMP_InputField[rows, columns];
        PopulateGrid();
    }

    void PopulateGrid()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                GameObject cell = Instantiate(cellPrefab, transform); // °´Ã¼ »ý¼º

            }
        }
    }
}
