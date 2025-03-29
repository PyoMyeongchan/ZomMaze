using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndSceneManager : MonoBehaviour
{
    public GameObject returnMenu;
    private void Start()
    {
        Invoke("MousePointOn", 2.0f);
        Invoke("ReturnButtonOn", 2.0f);
    }
    public void ExitScene()
    {
        SoundManager.instance.PlaySFX("ButtonSound");
        SceneManager.LoadScene(0);

    }

    public void MousePointOn()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ReturnButtonOn()
    {
        returnMenu.SetActive(true);
    }


}
