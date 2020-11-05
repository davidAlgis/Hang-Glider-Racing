using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private AudioSource m_coinSound;
    [SerializeField]
    private MeshRenderer m_meshRenderer;

    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            DataManager.Instance.NbrOfCoin++;
            m_meshRenderer.enabled = false;
            StartCoroutine(getCoin());
        }
    }

    private IEnumerator getCoin()
    {
        float lengthWait = m_coinSound.clip.length;
        m_coinSound.Play();
        yield return new WaitForSeconds(lengthWait);

        Destroy(gameObject);
    }
}
