using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class OpponentMovements : MonoBehaviour
{
    private Rigidbody m_rb;
    [SerializeField]
    private float m_slopeAngle;
    [SerializeField]
    private float m_forceDescent;
    private float m_forceDescentInit;
    [SerializeField]
    private float m_diveAngle;
    [SerializeField]
    private float m_forceDive;
    private float m_forceDiveInit;
    [SerializeField]
    private float m_riseAngle;
    [SerializeField]
    private float m_forceRise;
    private float m_forceRiseInit;
    private Vector3 m_posOriginToDive;

    private bool m_pressIsHold;
    private bool m_inTransition;
    private bool m_isRising;


    //to rotate the mesh
    private Vector3 m_oldPosition;
    private Vector3 m_oldDiff;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        //Convert the angle from degrees to radian (The degrees are easier to visualize)
        m_slopeAngle = (m_slopeAngle * Mathf.PI) / 180.0f;
        m_diveAngle = (m_diveAngle * Mathf.PI) / 180.0f;
        m_riseAngle = (m_riseAngle * Mathf.PI) / 180.0f;

        m_forceDescentInit = m_forceDescent;
        m_forceDiveInit = m_forceDive;
        m_forceRiseInit = m_forceRise;

        m_oldPosition = transform.position;
        m_oldDiff = Vector3.zero;
        m_pressIsHold = false;
        m_inTransition = false;

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        m_rb.velocity = m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);

    }


    private void Update()
    {

        updateRotationHangGlider();
        if(m_pressIsHold == false)
        {
            float x = Random.Range(0.0f, 100.0f);
            //print(x);
            //if (x > 99.9f)
                holdToDive();
            
        }
       
    }

    private void holdToDive()
    {

        m_pressIsHold = true;
        Pair<float, float> lengthSide;
        Vector3 forceDirection;

        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceDive * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 1.0f));
        m_posOriginToDive = transform.position;

        
        StartCoroutine(waitAndRelease());

    }

    private void releaseToRise()
    {
        print("opponent rise");
        Pair<float, float> lengthSide;
        Vector3 forceDirection;

        m_isRising = true;
        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceRise * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 0.5f));
        float height = Vector3.Distance(m_posOriginToDive, transform.position);
        StartCoroutine(rise(height));

        changeForces(1.0f + 0.01f * height);

        m_pressIsHold = false;
    }

    public void changeForces(float factor)
    {

        if (m_forceDescent * factor > 200.0f)
            m_forceDescent = 200.0f;
        else if (m_forceDive * factor < m_forceDescentInit)
            m_forceDescent = m_forceDescentInit;
        else
            m_forceDescent *= factor;

        if (m_forceDive * factor > 200.0f)
            m_forceDive = 200.0f;
        else if (m_forceDive * factor < m_forceDiveInit)
            m_forceDive = m_forceDiveInit;
        else
            m_forceDive *= factor;

        if (m_forceRise * factor > 200.0f)
            m_forceRise = 200.0f;
        else if (m_forceRise * factor < m_forceRiseInit)
            m_forceRise = m_forceRiseInit;
        else
            m_forceRise *= factor;
    }

    private void updateRotationHangGlider()
    {
        //rotate the hand glider to make it look where it goes.
        Vector3 diff = transform.position - m_oldPosition;

        //only use lookAt when he changes his direction
        if (Utilities.isCloseEpsilonVec3(diff, m_oldDiff, 0.01f) == false)
            transform.LookAt(transform.position + diff);

        m_oldPosition = transform.position;
        m_oldDiff = diff;
    }

    //convert an angle in a normalized length for adjacent and opposite side of a triangle
    private Pair<float, float> angleToLength(float angle)
    {
        /*     a
         *  -------
         *  \|t   |
         *   \    | 
         *  1 \   | b
         *     \  |
         *      \ |
         *       \|
         *  with the angle 1 return (b, a)
         */

        return new Pair<float, float>(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    //durationTransition in seconds
    private IEnumerator transitionBetweenVelocity(Vector3 v1, Vector3 v2, float durationTransition)
    {

        m_inTransition = true;
        Vector2 v1Proj = new Vector2(v1.y, v1.z);
        float v1Norm = v1Proj.magnitude;
        Vector2 v2Proj = new Vector2(v2.y, v2.z);
        float v2Norm = v2Proj.magnitude;

        const float iterationTime = 1 / 30;

        float rotationOfAngle = Vector2.SignedAngle(v1Proj, v2Proj) / (30.0f * durationTransition);

        for (int i = 0; i < 30 * durationTransition; i++)
        {

            v1Proj = Utilities.rotate(v1Proj, rotationOfAngle);

            //we update the scale of the vector
            if (v1Norm < v2Norm)
            {

                if (v1Proj.magnitude < v2Norm)
                    v1Proj *= 1.1f;
            }
            else
            {

                if (v1Proj.magnitude > v2Norm)
                    v1Proj *= 0.9f;
            }


            m_rb.velocity = new Vector3(0.0f, v1Proj.x, v1Proj.y);
            yield return new WaitForSeconds(iterationTime);
        }

        //to ensure it has the right magnitude.
        v1Proj = v1Proj.normalized;
        v1Proj *= v2Norm;
        m_rb.velocity = new Vector3(0.0f, v1Proj.x, v1Proj.y);

        m_inTransition = false;
    }

    private IEnumerator rise(float heightDive)
    {

        Vector3 posOriginRise = transform.position;
        while (Vector3.Distance(posOriginRise, transform.position) < (3.0f / 2.0f) * heightDive)
        {
            yield return new WaitForFixedUpdate();
        }

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        Vector3 direction = m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, direction, 1.0f));
        m_isRising = false;
    }

    private IEnumerator waitAndRelease()
    {
        while (m_inTransition)
            yield return new WaitForSeconds(0.1f);

        yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        releaseToRise();
    }
}
