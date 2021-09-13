using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressHeader : MonoBehaviour
{
    public int index;
    public string[] headerName = new string[2];

    private void Start() {
        Text t = GetComponent<Text>();
        t.text = GlobalSettings.instance.IsEnglish ? headerName[0] : headerName[1];
    }
}
