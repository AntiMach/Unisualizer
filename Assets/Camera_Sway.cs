using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Sway : MonoBehaviour
{
    private Camera cam;

    private Display bgHue;
    private Display bgLight;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        bgHue = GameObject.Find("Hue").GetComponent<Display>();
        bgLight = GameObject.Find("Light").GetComponent<Display>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Mathf.Sin(Time.time / 2) / 4, Mathf.Cos(Time.time) / 8, -10);
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time) * 2);
        cam.backgroundColor = Color.HSVToRGB(bgHue.value / 360f, 1, bgLight.value / 100f);
    }
}
