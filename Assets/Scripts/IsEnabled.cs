using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsEnabled : MonoBehaviour
{
    public int _needToUnlock;
    public Material _blackMaterial;

    private void Start()
    {
        if (PlayerPrefs.GetInt("score") < _needToUnlock)
            GetComponent<MeshRenderer>().material = _blackMaterial;
    }
}
