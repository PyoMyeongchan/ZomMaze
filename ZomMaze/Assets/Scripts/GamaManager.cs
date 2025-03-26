using UnityEngine;

public class GamaManager : MonoBehaviour
{
    // ������ �������� ��Ģ ����

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
