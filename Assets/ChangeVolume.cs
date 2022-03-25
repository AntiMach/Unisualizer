using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeVolume : MonoBehaviour
{
    AudioSource source;
    Display volume;

    void Start()
    {
        source = Menu.source;
        volume = GetComponent<Display>();
    }

    
}
