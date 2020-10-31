using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovements : MonoBehaviour
{
    private Rigidbody m_rb;
    [SerializeField]
    private float m_slopeAngle;
    [SerializeField]
    private float m_forceDescentConstant;
    [SerializeField]
    private float m_diveAngle;
    [SerializeField]
    private float m_forceDive;
    [SerializeField]
    private float m_riseAngle;
    [SerializeField]
    private float m_forceRise;
    private float m_heightDive;
    private Vector3 m_posOriginToDive;

    //to rotate the mesh
    [SerializeField]
    private GameObject m_hangGliderGO;
    private Vector3 m_oldPosition;
    private Vector3 m_oldDiff;

    //Press is Hold
    private bool m_pressIsHold;


    private void Start()
    {
        
        m_rb = GetComponent<Rigidbody>();
        //Convert the angle from degrees to radian (The degrees are easier to visualize)
        m_slopeAngle = (m_slopeAngle * Mathf.PI) / 180.0f;
        m_diveAngle = (m_diveAngle * Mathf.PI) / 180.0f;
        m_oldPosition = m_hangGliderGO.transform.position;
        m_oldDiff = Vector3.zero;
        m_pressIsHold = false;
    }


    private void Update()
    {
        Utilities.instantiateSphereAtPosition(transform.position);
        //rotate the hand glider to make it look where it goes.
        Vector3 diff = m_hangGliderGO.transform.position - m_oldPosition;

        //only use lookAt when he changes his direction
        if (Utilities.isCloseEpsilonVec3(diff, m_oldDiff, 0.01f) == false)
            m_hangGliderGO.transform.LookAt(m_hangGliderGO.transform.position + diff);
        
        m_oldPosition = m_hangGliderGO.transform.position;
        m_oldDiff = diff;

        Pair<float, float> lengthSide;
        Vector3 forceDirection;
        print(m_rb.velocity);


        if (Input.GetKey(KeyCode.Space) && m_pressIsHold==false)
        {
            m_rb.Sleep();
            m_rb.WakeUp();
            m_posOriginToDive = transform.position;

            lengthSide = angleToLength(m_diveAngle);
            forceDirection = m_forceDive *(transform.forward * lengthSide.first - transform.up * lengthSide.second);
            m_rb.AddForce(forceDirection, ForceMode.VelocityChange);

            m_pressIsHold = true;
        }
        else if(Input.GetKey(KeyCode.Space) && m_pressIsHold)
        {

        }
        else if(Input.GetKey(KeyCode.Space) == false && m_pressIsHold)
        {
            m_heightDive = Vector3.Distance(m_posOriginToDive, transform.position);
            //m_rb.AddForce(10.0f*transform.forward, ForceMode.VelocityChange);
            /*m_rb.Sleep();
            m_rb.WakeUp();*/
            /*lengthSide = angleToLength(m_diveAngle);
            forceDirection = m_forceRise * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
            print(forceDirection);
            m_rb.AddForce(forceDirection, ForceMode.VelocityChange);*/
            StartCoroutine(riseCoroutine());
            m_pressIsHold = false;

        }
        else
        {
            

        }

        lengthSide = angleToLength(m_slopeAngle);
        forceDirection = m_forceDescentConstant *(transform.forward * lengthSide.first - transform.up * lengthSide.second);
        m_rb.AddForce(forceDirection);
    }



    //convert an angle in a normalized length for adjacent and opposite side of a triangle
    private Pair<float,float> angleToLength(float angle)
    {
        /*    a
         * -------
         * \|t   |
         *  \    | 
         * 1 \   | b
         *    \  |
         *     \ |
         *      \|
         *  with the angle 1 return (b, a)
         */

        return new Pair<float, float>(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private IEnumerator riseCoroutine()
    {
        Vector3 posOriginToRise = transform.position;
        m_rb.Sleep();
        m_rb.WakeUp();
        m_rb.AddForce(10.0f * transform.forward, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.25f);
        m_rb.Sleep();
        m_rb.WakeUp();
        float heightRise = 0.0f;
        Pair<float, float> lengthSide;

        Vector3 forceDirection;
        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceRise * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
        m_rb.AddForce(forceDirection, ForceMode.VelocityChange);

        while (heightRise < 2*m_heightDive)
        {
            yield return new WaitForSeconds(0.1f);
            heightRise = Vector3.Distance(posOriginToRise, transform.position);
        }

        m_rb.Sleep();
        m_rb.WakeUp();

    }
}
