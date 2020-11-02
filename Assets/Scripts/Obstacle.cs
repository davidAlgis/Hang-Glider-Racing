using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Material m_obstacleMaterial;
    private float m_fadeSpeed = 0.1f;

    private void Awake()
    {
        m_obstacleMaterial = GetComponent<MeshRenderer>().material;
    }


    private void Update()
    {
        /*if(GameManager.Instance.HangGliderPlayer.transform.position.z > transform.position.z)
        {
            Color color = m_obstacleMaterial.color;
            color.a = 0.9f;
            m_obstacleMaterial.color = color;
        }*/
    }

    private void OnTriggerEnter(Collider other)
    {

        if(other.tag == "Player")
        {
            other.transform.parent.GetComponent<PlayerMovements>().changeForces(0.1f);
        }
    }

}
