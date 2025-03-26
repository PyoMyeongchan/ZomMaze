using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivityManager : MonoBehaviour
{
    public static MouseSensitivityManager instance;

    public Scrollbar sensitivityScrollbar;
    private float mouseSensitivity = 100.0f;
    private float minSensitivity = 1f;
    private float maxSensitivity = 200f;
    public Text mouseSensitivityText;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 100.0f);
        sensitivityScrollbar.value = (mouseSensitivity - minSensitivity) / (maxSensitivity - minSensitivity);
        MouseSensitivityText(mouseSensitivity);
        sensitivityScrollbar.onValueChanged.AddListener(UpdateSensitivity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSensitivity(float value)
    {
        mouseSensitivity = minSensitivity + value * (maxSensitivity - minSensitivity);
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.Save();
        MouseSensitivityText(mouseSensitivity);
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }

    public void MouseSensitivityText(float value)
    {
        mouseSensitivityText.text = $"{value}";
    }
}
