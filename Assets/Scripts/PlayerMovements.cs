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
    private bool m_inTransition;
    private bool m_isRising;

    private void Awake()
    {
        
        m_rb = GetComponent<Rigidbody>();
        //Convert the angle from degrees to radian (The degrees are easier to visualize)
        m_slopeAngle = (m_slopeAngle * Mathf.PI) / 180.0f;
        m_diveAngle = (m_diveAngle * Mathf.PI) / 180.0f;
        m_oldPosition = m_hangGliderGO.transform.position;
        m_oldDiff = Vector3.zero;
        m_pressIsHold = false;
        m_inTransition = false;
        m_isRising = false;

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        m_rb.velocity = m_forceDescentConstant * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
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
        //print(m_rb.velocity);

        if(m_inTransition == false)
        {
            if (Input.GetKey(KeyCode.Space) && m_pressIsHold == false)
            {
                lengthSide = angleToLength(m_diveAngle);
                forceDirection = m_forceDive * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
                StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 1.0f));
                m_posOriginToDive = transform.position;
                m_pressIsHold = true;
            }
            else if (Input.GetKey(KeyCode.Space) && m_pressIsHold)
            {

            }
            else if (Input.GetKey(KeyCode.Space) == false && m_pressIsHold)
            {

                m_heightDive = Vector3.Distance(m_posOriginToDive, transform.position);

                lengthSide = angleToLength(m_diveAngle);
                forceDirection = m_forceRise * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
                StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 1.0f));
                StartCoroutine(rise(m_heightDive));

                m_pressIsHold = false;

            }
        }
        

        
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


    //durationTransition in seconds
    private IEnumerator transitionBetweenVelocity(Vector3 v1, Vector3 v2, float durationTransition)
    {
        m_inTransition = true;
        Vector2 v1Proj = new Vector2(v1.y, v1.z);
        float v1Norm = v1Proj.magnitude;
        Vector2 v2Proj = new Vector2(v2.y, v2.z);
        float v2Norm = v2Proj.magnitude;
        print("v2Norm " + v2Norm);
        const float iterationTime = 1 / 30;
           
        float rotationOfAngle = Vector2.SignedAngle(v1Proj, v2Proj) / (30.0f * durationTransition);

        for(int i=0; i<30*durationTransition; i++)
        {
            
            v1Proj = Utilities.rotate(v1Proj, rotationOfAngle);

            print(v1Norm);
            //we update the scale of the vector
            /*if (v1Norm > v2Norm)
            {
                print((v1Norm - Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition)));
                v1Proj.Scale()
                v1Proj.x = Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition);
                v1Proj.y += Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition);

            }
            else
            {
                print((v1Norm + Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition)));
                v1Proj.x -= Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition);
                v1Proj.y -= Mathf.Abs(v1Norm - v2Norm) * (float)i / (30.0f * durationTransition);
            }*/
            if(v1Norm < v2Norm)
            {

                if (v1Proj.magnitude < v2Norm)
                    v1Proj *= 1.1f;
                else
                    print("obtained the speed");
            }
            else
            {
                if (v1Proj.magnitude > v2Norm)
                    v1Proj *= 0.9f;
                else
                    print("obtained the speed");
            }

            m_rb.velocity = new Vector3(0.0f, v1Proj.x, v1Proj.y);
            yield return new WaitForSeconds(iterationTime);
        }

        m_inTransition = false;

    }

    private IEnumerator rise(float heightDive)
    {
        m_isRising = true;
        Vector3 posOriginRise = transform.position;
        while (Vector3.Distance(posOriginRise, transform.position) < 2*heightDive)
        {
            yield return new WaitForFixedUpdate();
        }
        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        Vector3 direction = m_forceDescentConstant * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, direction, 5.0f));
        m_isRising = false;
    }
}
