using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeBehavior : MonoBehaviour
{
    // Transform of the GameObject you want to shake
    private Transform m_transform;

    // Desired duration of the shake effect
    private float m_shakeDuration = 0f;

    private float m_shakeBegin = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    private float m_shakeMagnitude = 0.7f;

    // A measure of how quickly the shake effect should evaporate
    private float m_dampingSpeed = 1.0f;

    // The initial position of the GameObject
    Vector3 m_initialPosition;

    void Awake()
    {
        if (m_transform == null)
        {
            m_transform = GetComponent(typeof(Transform)) as Transform;
        }
    }

    void OnEnable()
    {
        m_initialPosition = m_transform.localPosition;
    }


    // Update is called once per frame
    void Update()
    {
        if (m_shakeDuration > 0)
        {
            m_shakeBegin -= Time.deltaTime;

            if ((m_shakeBegin - Time.deltaTime) < 0)
            {
                m_transform.localPosition = m_initialPosition + Random.insideUnitSphere * m_shakeMagnitude;

                m_shakeDuration -= Time.deltaTime * m_dampingSpeed;

            }
        }
        else
        {
            m_transform.localPosition = m_initialPosition + Random.insideUnitSphere * 0.01f;
            m_shakeDuration = 0f;
            m_shakeBegin = 0f;
            //m_transform.localPosition = m_initialPosition;
        }
    }

    public void TriggerShake(float duree = 2.0f, float shakeMagnitude = 0.7f, float begin = 0f)
    {
        m_shakeMagnitude = shakeMagnitude;
        m_shakeDuration = duree;
        m_shakeBegin = begin;
     //   StartCoroutine(Shake(duree, shakeMagnitude));
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        
        float elapsed = 0f;

        while (elapsed < duration)
        {
            m_transform.localPosition = m_initialPosition + Random.insideUnitSphere * m_shakeMagnitude;
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = m_initialPosition;

    }
}
