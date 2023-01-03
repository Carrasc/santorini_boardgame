using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPosition : MonoBehaviour
{
    private int myRowIndex;
    private int myColIndex; 

    private void Awake()
    {
        myRowIndex = transform.parent.GetSiblingIndex();
        myColIndex = transform.GetSiblingIndex();
    }

    /// <summary>
    /// When the user clicks a position on the board
    /// </summary>
    private void OnMouseUpAsButton()
    {
        //Debug.Log("Clicked : " + gameObject.name);
        BoardManager.Instance.SetSelectedPosition(this.gameObject, myRowIndex, myColIndex);
    }
}
