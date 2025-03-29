using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    public Image panel;
    public float fadeDuration = 1.0f;
    public string nextSceneName;
    public bool isFirstLoad = true;
    private bool isFading = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name == "StartScene")
        {
            SoundManager.instance.PlayBGM("MainBGM", 1.0f);
            nextSceneName = "LoadingScene";

        }
        else if (SceneManager.GetActiveScene().name == "LoadingScene")
        {
            if (isFirstLoad)
            {
                nextSceneName = "Tutorial";

            }
            else
            {
                nextSceneName = "Map1";

            }
        }
        else if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            nextSceneName = "LoadingScene";
            SoundManager.instance.PlayBGM("NoBGM");

        }
        else if (SceneManager.GetActiveScene().name == "Map1")
        {
            SoundManager.instance.PlayBGM("MapBGM", 1.0f);
            nextSceneName = "EndScene";

        }
        else if (SceneManager.GetActiveScene().name == "EndScene")
        {
            SoundManager.instance.PlayBGM("MainBGM", 1.0f);
        }
    }

    public void SetFirstLoadFalse()
    {
        isFirstLoad = false;
    }


    public void FadeOutScene()
    {
        if (!isFading)
        {
            SoundManager.instance.PlaySFX("LoadingSound");
            StartCoroutine(FadeInAndLoadScene(nextSceneName));
        }
    }



    public IEnumerator FadeInAndLoadScene(string nextSceneName)
    { 
        isFading = true;

        yield return StartCoroutine(FadeImage(0,1,fadeDuration));

        SceneManager.LoadScene(nextSceneName);

        yield return StartCoroutine(FadeImage(1,0,fadeDuration));

        isFading = false ;
    }

    IEnumerator FadeImage(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0.0f;

        Color panelColor = panel.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            panelColor.a = newAlpha;
            panel.color = panelColor;
            yield return null;
        }

        panelColor.a = endAlpha;
        panel.color = panelColor;

        if (isFading)
        {
            SceneManager.LoadScene(nextSceneName);

        }
    }



}
