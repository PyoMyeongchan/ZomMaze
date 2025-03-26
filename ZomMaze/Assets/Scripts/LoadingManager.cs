using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public Slider loadingSlider;

    private string nextSceneName;

    public void StartLoading(string sceneName) // �ε� �ڷ�ƾ�� �����ϴ� �Լ�
    {
        nextSceneName = sceneName;
        StartCoroutine(LoadLoadingSceneAndNextScene());
    }

    // �ε� ���� �ε��ϰ�  NextScene�� �ε�� ������ ����ϰ� �ڷ�ƾ
    IEnumerator LoadLoadingSceneAndNextScene()
    { 
        // �ε����� �񵿱������� �ε�(�ε� ����ǥ�ÿ����� ����ϴ¾�)
        AsyncOperation loadingSceneOp = SceneManager.LoadSceneAsync("LoadingScene", LoadSceneMode.Additive); // Additive : ���� ���� ���� �߰��ϴ� ���(���� �� ����)
        loadingSceneOp.allowSceneActivation = false;

        // �ε����� �ε�ɶ����� ���
        while (!loadingSceneOp.isDone) // �ε����� �ε�� ������ ���
        {
            if (loadingSceneOp.progress >= 0.9f) // �ε����� ���� �� �ε�ɶ����� (progress > 0.9�̻��̸� �غ� �Ϸ� ����)
            { 
                // �ε��� �غ� �Ϸ�Ǹ� �� Ȱ��ȭ
                loadingSceneOp.allowSceneActivation = true;
            }
            yield return null;

        }
        // �ε������� �ε� silder ã�ƿ���
        FindLoadingSliderInScene();

        // NextScene�� �񵿱������� �ε�
        AsyncOperation nextSceneOp = SceneManager.LoadSceneAsync(nextSceneName);

        // ���� �� �ε尡 �Ϸ� �ɶ����� ����ϸ鼭 ����� �����̴��� ǥ��
        while (!nextSceneOp.isDone)
        {
            // �ε����൵ ������Ʈ(0~1)
            loadingSlider.value = nextSceneOp.progress;
            yield return null;
        }
        // nextscene�� ������ �ε�� ��, �ε����� ���ε�
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    void FindLoadingSliderInScene()
    { 
        loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();
    }

}
