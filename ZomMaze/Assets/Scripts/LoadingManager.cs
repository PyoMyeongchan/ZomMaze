using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingSlider;

    private string nextSceneName;

    public void StartLoading(string sceneName) // 로딩 코루틴을 시작하는 함수
    {
        nextSceneName = sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene());
    }

    // 로딩 씬을 로드하고  NextScene이 로드될 때까지 대기하고 코루틴
    IEnumerator LoadLoadingSceneAndNextScene()
    { 
        // 로딩씬을 비동기적으로 로드(로딩 상태표시용으로 사용하는씬)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive); // Additive : 현재 씬에 씬을 추가하는 방식(기존 씬 유지)
        loadingSceneOp.allowSceneActivation = false;

        // 로딩씬이 로드될때까지 대기
        while (!loadingSceneOp.isDone) // 로딩씬이 로드될 때까지 대기
        {
            if (loadingSceneOp.progress >= 0.9f) // 로딩씬이 거의 다 로드될때까지 (progress > 0.9이상이면 준비 완료 상태)
            { 
                // 로딩씬 준비 완료되면 씬 활성화
                loadingSceneOp.allowSceneActivation = true;
            }
            yield return null;

        }
        // 로딩씬에서 로딩 silder 찾아오기
        FindLoadingSliderInScene();

        // NextScene을 비동기적으로 로드
        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(nextSceneName);

        // 다음 씬 로드가 완료 될때까지 대기하면서 진행률 슬라이더에 표시
        while (!nextSceneOp.isDone)
        {
            // 로딩진행도 업데이트(0~1)
            loadingSlider.value = nextSceneOp.progress;
            yield return null;
        }
        // nextscene이 완전히 로드된 후, 로딩씬을 업로드
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    void FindLoadingSliderInScene()
    { 
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }

}
