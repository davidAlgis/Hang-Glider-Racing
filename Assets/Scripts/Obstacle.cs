using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Obstacle : MonoBehaviour
{
    private Material[] m_obstacleMaterials;
    private bool m_matHasBeenUpdated;
    [SerializeField]
    private float m_angleAddSlope;

    private void Awake()
    {
        m_matHasBeenUpdated = false;
        //convert to radian
        m_angleAddSlope = (m_angleAddSlope * Mathf.PI) / 180.0f;

        m_obstacleMaterials = GetComponent<MeshRenderer>().materials;
    }

    private void Update()
    {
        if(m_matHasBeenUpdated == false)
        {
            if (transform.position.z < GameManager.Instance.HangGliderPlayer.transform.position.z)
            {
                if (m_obstacleMaterials != null)
                {
                    foreach (var mat in m_obstacleMaterials)
                    {
                        Color color = mat.color;
                        color.a = 0.1f;
                        mat.color = color;
                    }
                }
                m_matHasBeenUpdated = true;
            }
        }

    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" || other.tag == "Opponent")
        {
            Movements movements = other.transform.parent.GetComponent<Movements>();
            movements.changeForces(0.1f);
            movements.diveReduce(m_angleAddSlope);
        }
        
        if(other.tag == "Player")
        { 
            if(m_obstacleMaterials != null)
            {
                foreach(var mat in m_obstacleMaterials)
                {
                    Color color = mat.color;
                    color.a = 0.1f;
                    mat.color = color;
                }
            }
            m_matHasBeenUpdated = true;
        }
    }

    /*private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (m_obstacleMaterials != null)
            {
                foreach (var mat in m_obstacleMaterials)
                {
                    Color color = mat.color;
                    color.a = 1.0f;
                    mat.color = color;
                }
            }

        }
    }*/

}
