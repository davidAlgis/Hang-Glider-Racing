using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Obstacle : MonoBehaviour
{
    private Material[] m_obstacleMaterials;
    [SerializeField]
    private float m_changeForces = 0.1f;
    private bool m_matHasBeenUpdated;
    [SerializeField]
    private float m_angleAddSlope;
    [SerializeField]
    private bool m_isMeteor;


    private void Awake()
    {
        m_matHasBeenUpdated = false;
        //convert to radian
        m_angleAddSlope = (m_angleAddSlope * Mathf.PI) / 180.0f;

        m_obstacleMaterials = GetComponent<MeshRenderer>().materials;
    }

    private void Update()
    {
        if(m_isMeteor && UIManager.Instance.TutorialPanelGO.activeSelf == false)
        {
            if (Mathf.Abs(GameManager.Instance.HangGliderPlayer.transform.position.z - transform.position.z) < 30.0f)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.useGravity = true;
                rb.AddForce(6.0f * transform.forward);
                rb.AddTorque(transform.forward);
            }

            if (transform.position.y < GameManager.Instance.HeightFloor - 10.0f)
                Destroy(gameObject);
        }
    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" || other.tag == "Opponent")
        {
            Movements movements = other.transform.parent.GetComponent<Movements>();
            movements.changeForces(m_changeForces);
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


}
