using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stag : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer part in allRenderers)
        {
            part.color = Color.black;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
