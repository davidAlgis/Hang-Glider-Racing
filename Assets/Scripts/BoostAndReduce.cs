using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostAndReduce : MonoBehaviour
{
    [SerializeField]
    private TypeBoostAndReduce typeBoost;
    [SerializeField]
    private float m_boostSpeed;
    [SerializeField]
    private float m_heightToRise;
    [SerializeField]
    private float m_angleAddSlope;
    [SerializeField]
    private float m_factorSpeedReduce = 0.5f;

    private void Awake()
    {
        //convert to radian
        m_angleAddSlope = (m_angleAddSlope * Mathf.PI) / 180.0f;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "Player" || other.tag == "Opponent")
        {
            Movements playerMovements = other.transform.parent.GetComponent<Movements>();

            switch (typeBoost)
            {
                case TypeBoostAndReduce.speedBoost:
                    playerMovements.changeForces(m_boostSpeed);
                    break;
                case TypeBoostAndReduce.riseBoost:
                    playerMovements.riseBoost(m_heightToRise);
                    break;
                case TypeBoostAndReduce.speedReduce:
                    playerMovements.changeForces(m_factorSpeedReduce);
                    playerMovements.diveReduce(m_angleAddSlope);
                    break;

            }   
        }
    
    }

}

public enum TypeBoostAndReduce
{
    speedBoost,
    riseBoost,
    speedReduce
}
