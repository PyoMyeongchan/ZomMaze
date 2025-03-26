using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingUImanager : MonoBehaviour
{
    private static GameSettingUImanager instance;
    public GameObject settingsObj;

    public Text resolutionText;
    public Text graphicsText;
    public Text fullScreenText;

    public GameObject Main;
    public GameObject start;
    public GameObject settingUi;
    public GameObject exit;


    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;
    

    private string[] resolutions = { "1280 X 720", "1920 X 1080", "2560 X 1440", "3840 X 2160" };
    private string[] qualityOptions = { "Low", "Medieum", "High" };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject); 
            // 씬이 로드될 때마다 OnSceneLoaded 메서드 호출
            SceneManager.sceneLoaded += OnScenceLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void OnScenceLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScene")
        {
            Main.SetActive(true);
            start.SetActive(true);
            settingUi.SetActive(true);
            exit.SetActive(true);
        }
        else 
        {
            Main.SetActive(false);
            start.SetActive(false);
            settingUi.SetActive(false);
            exit.SetActive(false);
        }
    }
    

    public void GameStart()
    {
        SoundManager.instance.SetSFXVolume(0.5f);
        SoundManager.instance.PlaySFX("ButtonSound");

        SceneManager.LoadScene(1);

    }

    public void GameQuit()
    {
        Application.Quit();

    }

    public void OnResolutionLeftClick()
    { 
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpadateResolutionsText();

    }
    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpadateResolutionsText();
    }


    public void OnGraphicsLeftClick()
    {
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        UpdateGraphicsQulityText();
    }

    public void OnGraphicsRightClick()
    {
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        UpdateGraphicsQulityText();
    }

    public void OnFullScreenTogleClick()
    { 
        isFullScreen = !isFullScreen;
        UpdateFullScreenText();
    }

    private void UpadateResolutionsText()
    { 
        resolutionText.text = resolutions[resolutionIndex];
    
    }

    private void UpdateGraphicsQulityText()
    { 
        graphicsText.text = qualityOptions[qualityIndex];
    
    }

    private void UpdateFullScreenText()
    {
        fullScreenText.text = isFullScreen ? "On" : "Off";
    }



    public void OnApplyButtonClick()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        ApplySetting();
        SaveSetting();
    }

    private void ApplySetting()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        string[] res = resolutions[resolutionIndex].Split('X');
        int width = int.Parse(res[0]); // 오류 확인할 것
        int height = int.Parse(res[1]);
        Screen.SetResolution(width, height, isFullScreen);
        QualitySettings.SetQualityLevel(qualityIndex);

    }

    private void SaveSetting()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("GraphicsQualityIndex", qualityIndex);
        PlayerPrefs.SetInt("FullScreen",isFullScreen ? 1:0);
        PlayerPrefs.Save();
    }

    private void LoadSetting()
    { 
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);

    }



    public void OnSettings()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        settingsObj.SetActive(true);

    }

    public void OffSettings()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        settingsObj.SetActive(false); 
    }
}
