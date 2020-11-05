using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager m_instance;
    [SerializeField]
    private AudioSource m_soundDive;
    private float m_startVolumeSoundDive;
    [SerializeField]
    private AudioSource m_soundRise;
    private float m_startVolumeSoundRise;
    [SerializeField]
    private AudioSource m_soundFlight;
    private float m_startVolumeSoundFlight;
    private float m_timeOfEndSoundDive;


    public static SoundManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            }
            return m_instance;
        }
    }


    private void Awake()
    {
        m_startVolumeSoundDive = m_soundDive.volume;
        m_startVolumeSoundRise = m_soundRise.volume;
        m_startVolumeSoundFlight = m_soundFlight.volume;

    }

    public void playSoundFlight()
    {

        playFadeIn(m_soundFlight, m_startVolumeSoundFlight);
    }

    public void stopSoundFlight()
    {

        stopFadeOut(m_soundFlight, m_startVolumeSoundFlight,3.0f);
    }

    public void playSoundDive()
    {
        playFadeIn(m_soundDive, m_startVolumeSoundDive);
    }

    public void stopSoundDiveAndPlaySoundRise()
    {
        m_timeOfEndSoundDive = m_soundDive.time;
        //stop sound
        stopFadeOut(m_soundDive, m_startVolumeSoundDive, 0.5f);
        m_soundRise.time = Mathf.Abs(m_soundDive.clip.length - m_timeOfEndSoundDive);
        playFadeIn(m_soundRise, m_startVolumeSoundRise, 0.5f);
    }

    public void stopSoundRise()
    {

        stopFadeOut(m_soundRise, m_startVolumeSoundRise, 1.0f);
    }

    public void stopAllAudioSources()
    {
        StopAllCoroutines();
        stopFadeOut(m_soundDive, m_startVolumeSoundDive, 0.5f);
        stopFadeOut(m_soundRise, m_startVolumeSoundRise, 0.5f);

    }

    public void playFadeIn(AudioSource audioSource, float startVolume, float fadeTime = 1.0f)
    {
        StartCoroutine(FadeIn(audioSource, startVolume, fadeTime));
    }

    public void stopFadeOut(AudioSource audioSource, float startVolume, float fadeTime = 1.0f)
    {
        StartCoroutine(FadeOut(audioSource, startVolume, fadeTime));
    }

    public IEnumerator FadeOut(AudioSource audioSource, float startVolume, float fadeTime)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    public IEnumerator FadeIn(AudioSource audioSource, float startVolume, float fadeTime)
    {
        audioSource.volume = 0.0f;

        if (audioSource.clip.length < audioSource.time)
            yield break;
        
        audioSource.Play();
        
        
        while (audioSource.volume < startVolume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.volume = startVolume;
    }

}
