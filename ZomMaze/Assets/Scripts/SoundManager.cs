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

    // public���� ��������
    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    // public���� �ȵǴ� ����ü ���� ����
    [System.Serializable]
    public struct NamedAudioClip
    {     
        public string name;
        public AudioClip clip;
    }

    public NamedAudioClip[] bgmClipList;
    public NamedAudioClip[] sfxClipList;

    private Coroutine currentBGMCoroutine; // BGM�� ������ �ִ��� Ȯ��



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
            if (currentBGMCoroutine != null) // ���� BGM�� ���� ��� �浹���� �����ϴ� �ڵ�, �ڷ�ƾ �Ǵ��� Ȯ��
            {
                StopCoroutine(currentBGMCoroutine);
            }

            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () => // ���� BGM�� ���۰� ������ ������ �ð����� ���̵���, �ƿ��ϴ� ���� �ݺ��ϴ� �ڵ�
            {
                bgmSource.spatialBlend = 0f; // 2D�� ������ �Ҹ��� �ٳ�
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
            AudioSource.PlayClipAtPoint(sfxClips[name],position); // Ư����ġ�� �Ҹ� ������ִ� �ڵ�
            sfxSource.spatialBlend = 1f;
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxClips.ContainsKey(name))
        {
            sfxSource.PlayOneShot(sfxClips[name]); // Ư����ġ�� �Ҹ� ������ִ� �ڵ�
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

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete) // BGM ���̵�ƿ� ����
    { 
    
        float startVolume = bgmSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t/duration);
            yield return null;
        }

        bgmSource.volume = 0;
        onFadeComplete?.Invoke(); // ���̵� �ƿ��� �Ϸ�Ǹ� ���� �۾� ����
    }

    private IEnumerator FadeInBGM(float duration) // BGM ���̵��� ����
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
