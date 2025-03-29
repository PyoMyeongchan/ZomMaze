using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;


public enum ParticleType
{ 
    Explosion,
    WeaponFire,
    PistolFire,
    WeaponSmoke,
    ItemBlink,
    Blood


}

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    private Dictionary<ParticleType, GameObject> particleSystemDic = new Dictionary<ParticleType, GameObject>(); // 초기화
    private Dictionary<ParticleType, Queue<GameObject>> particlePools = new Dictionary<ParticleType, Queue<GameObject>>();

    public GameObject explosionParticle;
    public GameObject weaponFireParticle;
    public GameObject PistolFireParticle;
    public GameObject WeaponSmokeParticle;
    public GameObject BloodParticle;
    public GameObject ItemBlinkParticle;

    public int poolSize = 10;

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

        AddParticle();
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeParticlePools();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로딩 시마다 풀 초기화
        InitializeParticlePools();
    }

    private void InitializeParticlePools()
    {
        // 오브젝트 풀링
        foreach (var type in particleSystemDic.Keys)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(particleSystemDic[type]);
                obj.gameObject.SetActive(false);
                pool.Enqueue(obj);
            }
            particlePools[type] = pool;
        }
    }

    public void ParticlePlay(ParticleType type, Vector3 pos, Vector3 scale)
    {
        if (particleSystemDic.ContainsKey(type))
        {
            // 풀에서 오브젝트 꺼내기 전에 큐의 상태 확인
            if (particlePools[type].Count == 0)
            {
                // 큐가 비어있으면 새로운 파티클 생성하여 추가
                GameObject newParticle = Instantiate(particleSystemDic[type]);
                newParticle.SetActive(false);
                particlePools[type].Enqueue(newParticle);
            }

            // 큐에서 오브젝트 꺼내기
            GameObject particleObj = particlePools[type].Dequeue();
            if (particleObj != null)
            {
                Debug.Log($"Playing particle: {type}");
                particleObj.transform.position = pos;
                ParticleSystem particleSystem = particleObj.GetComponentInChildren<ParticleSystem>();

                if (particleSystem.isPlaying)
                {
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                particleObj.transform.localScale = scale;
                particleObj.SetActive(true);
                particleSystem.Play();
                StartCoroutine(particleEnd(type, particleObj, particleSystem));
            }
        }

    }

    private void AddParticle()
    {
        // 기존에 들어있던 데이터를 삭제하고 새로 추가
        particleSystemDic.Clear(); 
        if (!particleSystemDic.ContainsKey(ParticleType.Explosion))
        {
            particleSystemDic.Add(ParticleType.Explosion, explosionParticle);
        }

        if (!particleSystemDic.ContainsKey(ParticleType.WeaponFire))
        {
            particleSystemDic.Add(ParticleType.WeaponFire, weaponFireParticle);
        }

        if (!particleSystemDic.ContainsKey(ParticleType.WeaponSmoke))
        {
            particleSystemDic.Add(ParticleType.WeaponSmoke, WeaponSmokeParticle);
        }

        if (!particleSystemDic.ContainsKey(ParticleType.Blood))
        {
            particleSystemDic.Add(ParticleType.Blood, BloodParticle);
        }

        if (!particleSystemDic.ContainsKey(ParticleType.PistolFire))
        {
            particleSystemDic.Add(ParticleType.PistolFire, PistolFireParticle);
        }

        if (!particleSystemDic.ContainsKey(ParticleType.ItemBlink))
        {
            particleSystemDic.Add(ParticleType.ItemBlink, ItemBlinkParticle);
        }
    }

    IEnumerator particleEnd(ParticleType type, GameObject particleObj, ParticleSystem particleSystem)
    {
        while (particleSystem.isPlaying)
        {
            yield return null;
        }
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleObj.SetActive(false);
        particlePools[type].Enqueue(particleObj);
    }

    // 이벤트 리스너 제거
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

/*
if (particleSystemDic.ContainsKey(type))
{
    GameObject particle = Instantiate(particleSystemDic[type], pos, Quaternion.identity);
    particle.gameObject.transform.localScale = scale;
    Transform playerTransform = PlayerManager.Instance.transform;
    Vector3 directionToPlayer = playerTransform.localPosition - pos;
    Quaternion rotation = Quaternion.LookRotation(directionToPlayer); // 파티클이 카메라에 잘보이도록하는 코드
    particle.Play();
    Destroy(particle.gameObject, particle.main.duration); // 파티클 재생된 후 제거

}
*/