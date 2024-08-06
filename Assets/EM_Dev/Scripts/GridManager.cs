using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab;
    public int rows = 10;
    public int columns = 10;

    void Start()
    {
        PopulateGrid();
    }

    void PopulateGrid()
    {
        for (int i = 0; i < rows ; i++) 
        {
            for(int j = 0; j < columns ; j++)
                Instantiate(cellPrefab, transform); // °´Ã¼ »ý¼º´À³¦
        }
    }
}
