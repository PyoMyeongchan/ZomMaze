using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    public AudioSource bgmSource; 
    public AudioSource sfxSource;

    // public으로 되지않음
    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    // public으로 안되니 구조체 만들어서 지정
    [System.Serializable]
    public struct NamedAudioClip
    {     
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currentBGMCoroutine; // BGM이 나오고 있는지 확인



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioClip();
        }
        else 
        { 
            Destroy(gameObject);
        }
    }


    

    void InitializeAudioClip()
    {

        foreach (var bgm in bgmClipList)
        {
            if (!bgmClips.ContainsKey(bgm.name))
            { 
              bgmClips.Add(bgm.name, bgm.clip);  
            }
        }

        foreach (var sfx in sfxClipList)
        {
            if (!sfxClips.ContainsKey(sfx.name))
            {
                sfxClips.Add(sfx.name, sfx.clip);
            }
        }
    }

    public void PlayBGM(string name, float fadeDuration = 1.0f)
    {
        if (bgmClips.ContainsKey(name))
        {
            if (currentBGMCoroutine != null) // 이전 BGM이 있을 경우 충돌됨을 방지하는 코드, 코루틴 되는지 확인
            {
                StopCoroutine(currentBGMCoroutine);
            }

            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () => // 현재 BGM의 시작과 끝에서 정해진 시간동안 페이드인, 아웃하는 것을 반복하는 코드
            {
                bgmSource.spatialBlend = 0f; // 2D로 전역에 소리가 다남
                bgmSource.clip = bgmClips[name];
                bgmSource.Play();
                currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration));

            }));

        }
    }

    public void PlaySFX(string name, Vector3 position)
    {
        if (sfxClips.ContainsKey(name))
        {
            AudioSource.PlayClipAtPoint(sfxClips[name],position); // 특정위치의 소리 재생해주는 코드
            sfxSource.spatialBlend = 1f;
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxClips.ContainsKey(name))
        {
            sfxSource.PlayOneShot(sfxClips[name]); // 특정위치의 소리 재생해주는 코드
            sfxSource.spatialBlend = 1f;
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();   
    }

    public void StopSFX()
    {
        sfxSource.Stop();    
    }

    public void SetBGMVolume(float volume)
    { 
        bgmSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp(volume, 0, 1);
    }

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete) // BGM 페이드아웃 과정
    { 
    
        float startVolume = bgmSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t/duration);
            yield return null;
        }

        bgmSource.volume = 0;
        onFadeComplete?.Invoke(); // 페이드 아웃이 완료되면 다음 작업 실행
    }

    private IEnumerator FadeInBGM(float duration) // BGM 페이드인 과정
    {
        float startVolume = 0f;
        bgmSource.volume = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 1f, t / duration);
            yield return null;
        }
        bgmSource.volume = 1.0f;
    }
}
