using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField]
    private Button _focusButton;

    public void FocusButton()
    {
        _focusButton.Select();
    }

     private void SwipeIn()
    {
        
    }
}
