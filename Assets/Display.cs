using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Display : MonoBehaviour
{
    public string symbol;
    public float value;

    private Slider slider;
    private Text display;

    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        display = transform.Find("Number").GetComponent<Text>();
        OnValueChanged();
    }

    public void OnValueChanged()
    {
        if (slider == null || display == null)
            return;
        value = slider.value;
        display.text = string.Format("{0}{1}", (int)value, symbol);
        EventSystem.current.SetSelectedGameObject(null);
    }
}
