using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTextureImage : MonoBehaviour
{
    [SerializeField]
    Texture overlayImage;

    // Start is called before the first frame update
    void Start()
    {
        if (overlayImage)
        {
            var material = GetComponent<MeshRenderer>().material;
            material.mainTexture = overlayImage;
            material.SetTexture("_EmissionTex", overlayImage);        
        }
    }
}