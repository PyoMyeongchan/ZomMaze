using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum ObjectState
{
    Near,
    Far

}


public class AllObjectManager : MonoBehaviour
{
    public static AllObjectManager Instance { get; private set; }
    public ObjectState currentstate = ObjectState.Near;
    private Coroutine stateRoutine;
    public float TrackingRange = 5.0f;

    public GameObject Text1;
    public GameObject Text2;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


    }

    void Start()
    {
        Text1.SetActive(false);
        Text2.SetActive(false);
        ChangeState(currentstate);


    }


    void Update()
    {
        float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

    }



    public void ChangeState(ObjectState newState)
    {
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
        }
        currentstate = newState;

        switch (currentstate)
        {
            case ObjectState.Near:
                stateRoutine = StartCoroutine(Near());
                break;
            case ObjectState.Far:
                stateRoutine = StartCoroutine(Far());
                break;
        }

    }

    private IEnumerator Near()
    {
        Debug.Log("캐릭터 가까운상태");
        while (currentstate == ObjectState.Near)
        {

            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

            if (distance <= TrackingRange)
            {
                Text1.SetActive(true);
                Text2.SetActive(true);
            }
            else
            {
                ChangeState(ObjectState.Far);


            }
            yield return null;


        }

    }

    private IEnumerator Far()
    {
        Debug.Log("캐릭터 먼상태");
        while (currentstate == ObjectState.Far)
        {

            float distance = Vector3.Distance(transform.position, PlayerManager.Instance.transform.position);

            if (distance > TrackingRange)
            {
                Text1.SetActive(false);
                Text2.SetActive(false);
            }
            else
            {
                ChangeState(ObjectState.Near);


            }
            yield return null;
        }
    }
}
