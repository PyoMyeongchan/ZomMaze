using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingUImanager : MonoBehaviour
{
    public static GameSettingUImanager instance;
    public GameObject settingsObj;

    public Text resolutionText;
    public Text graphicsText;
    public Text fullScreenText;

    public GameObject Main;
    public GameObject start;
    public GameObject settingUi;
    public GameObject exit;

    private bool isPause = false;

    public GameObject gameOver;
    public GameObject pauseObj;


    private int resolutionIndex = 0;
    private int qualityIndex = 0;
    private bool isFullScreen = true;

    public GameObject firstTab;
    public GameObject SecondTab;
    private bool isKeyguideOn = false;


    private string[] resolutions = { "1280 X 720", "1920 X 1080", "2560 X 1440", "3840 X 2160" };
    private string[] qualityOptions = { "Low", "Medieum", "High" };
    private bool isSetting;

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

    private void Update()
    {
        PauseController();
        KeyGuideOnOff();
    }



    void OnScenceLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScene")
        {
            Main.SetActive(true);
            start.SetActive(true);
            settingUi.SetActive(true);
            exit.SetActive(true);
            firstTab.SetActive(false);
            SecondTab.SetActive(false);
        }
        else
        {
            Main.SetActive(false);
            start.SetActive(false);
            settingUi.SetActive(false);
            exit.SetActive(false);
            firstTab.SetActive(true);
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
        SoundManager.instance.PlaySFX("ButtonSound");
        UpadateResolutionsText();

    }
    public void OnResolutionRightClick()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        SoundManager.instance.PlaySFX("ButtonSound");
        UpadateResolutionsText();
    }


    public void OnGraphicsLeftClick()
    {
        qualityIndex = Mathf.Max(0, qualityIndex - 1);
        SoundManager.instance.PlaySFX("ButtonSound");
        UpdateGraphicsQulityText();
    }

    public void OnGraphicsRightClick()
    {
        qualityIndex = Mathf.Min(qualityOptions.Length - 1, qualityIndex + 1);
        SoundManager.instance.PlaySFX("ButtonSound");
        UpdateGraphicsQulityText();
    }

    public void OnFullScreenTogleClick()
    {
        isFullScreen = !isFullScreen;
        SoundManager.instance.PlaySFX("ButtonSound");
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
        PlayerPrefs.SetInt("FullScreen", isFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSetting()
    {
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 1);
        qualityIndex = PlayerPrefs.GetInt("GraphicsQualityIndex", 1);

    }



    public void OnSettings()
    {
        isSetting = true;
        SoundManager.instance.PlaySFX("ButtonSound");
        settingsObj.SetActive(true);

    }

    public void OffSettings()
    {
        isSetting = false;
        SoundManager.instance.PlaySFX("ButtonSound");
        settingsObj.SetActive(false);
    }

    public void TutorialMapYes()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        MenuManager.Instance.isFirstLoad = true;

    }

    public void TutorialMapNo()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        MenuManager.Instance.isFirstLoad = false;
    }

    void PauseController()
    {
        if (SceneManager.GetActiveScene().name != "StartScene")
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !isSetting)
            {

                isPause = !isPause;


                if (isPause)
                {
                    Pause();
                }
                else
                {
                    ReGame();
                }
            }
        }
    }
    void ReGame()
    {
        // 소리 넣기        
        pauseObj.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1; // 게임시간 재개
        PlayerManager.Instance.isStop = false;


    }

    public void Regame() // 버튼에 쓰일 함수
    {
        // 소리 넣기
        pauseObj.SetActive(false);
        SoundManager.instance.PlaySFX("ButtonSound");
        Cursor.lockState = CursorLockMode.Locked;

        PlayerManager.Instance.isStop = false;

        Time.timeScale = 1; // 게임시간 재개

    }

    void Pause()

    {
        PlayerManager.Instance.isStop = true;

        pauseObj.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0; // 게임시간 정지
    }


    public void gameOverMenu()
    {
        Invoke("GameoverPause", 4.0f);
    }

    void GameoverPause()
    {
        isPause = true;
        gameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Time.timeScale = 0;
    }

    public void ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }


    public void Exit()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        pauseObj.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene(0);


    }


    void KeyGuideOnOff()
    {
        if (SceneManager.GetActiveScene().name != "StartScene" || SceneManager.GetActiveScene().name != "LoadingScene")
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SoundManager.instance.PlaySFX("ButtonSound");
                isKeyguideOn = !isKeyguideOn;

                if (isKeyguideOn)
                {
                    firstTab.SetActive(true);
                    SecondTab.SetActive(false);
                }
                else
                {
                    firstTab.SetActive(false);
                    SecondTab.SetActive(true);
                }
            }
        }

    }
}
