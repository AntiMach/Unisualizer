using UnityEngine;
using UnityEngine.UI;

public class LimitFreq : MonoBehaviour
{
    private Slider sliderMin;
    private Slider sliderMax;

    void Start()
    {
        sliderMin = transform.Find("Minimum").GetComponentInChildren<Slider>();
        sliderMax = transform.Find("Maximum").GetComponentInChildren<Slider>();
    }

    public void LimitMinimum()
    {
        if (sliderMax == null || sliderMin == null)
            return;

        sliderMin.value = Mathf.Min(sliderMin.value, sliderMax.value);
    }

    public void LimitMaximum()
    {
        if (sliderMax == null || sliderMin == null)
            return;

        sliderMax.value = Mathf.Max(sliderMin.value, sliderMax.value);
    }
}
