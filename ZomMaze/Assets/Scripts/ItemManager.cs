using System.Collections;
using UnityEngine;



public class ItemManager : MonoBehaviour
{

    public static ItemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

    }


    void Update()
    {
        TurnItem();
    }

    void TurnItem()
    {
        transform.Rotate(new Vector3(0, 10f, 0) * Time.deltaTime*2, Space.World);
    }







}
