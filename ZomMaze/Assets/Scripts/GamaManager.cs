using UnityEngine;

public class GamaManager : MonoBehaviour
{
    // 게임의 전반적인 규칙 상태

    public static GamaManager Instance { get; private set; }

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
    }

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}
