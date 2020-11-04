using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Movements : MonoBehaviour
{
    private Rigidbody m_rb;
    [SerializeField]
    private float m_slopeAngle;
    [SerializeField]
    private float m_forceDescent;
    [SerializeField]
    private float m_forceDescentMax;
    private float m_forceDescentInit;
    [SerializeField]
    private float m_diveAngle;
    [SerializeField]
    private float m_forceDive;
    [SerializeField]
    private float m_forceDiveMax;
    private float m_forceDiveInit;
    [SerializeField]
    private float m_riseAngle;
    [SerializeField]
    private float m_forceRise;
    [SerializeField]
    private float m_forceRiseMax;
    private float m_forceRiseInit;
    private float m_riseFactor;
    private Vector3 m_posOriginToDive;

    private bool m_pressIsHold;
    private bool m_inTransition;
    private bool m_isRisingBoostOrDiveReduce;
    private bool m_isWaitingForDiving;
    private bool m_isLanded;

    //to rotate the mesh
    [SerializeField]
    private GameObject m_hangGliderGO;
    private Vector3 m_oldPosition;
    private Vector3 m_oldDiff;

    //visual effect
    private ShakeBehavior m_shakeBehaviorCamera;
    private ParticleSystem m_particleTrails;

    //configuration for AI
    [SerializeField]
    private bool m_handleByAI;
    private Color m_associatedColor;
    [SerializeField]
    private int m_randomSeed;
    [SerializeField]
    private string m_name;

    public GameObject HangGliderGO { get => m_hangGliderGO; set => m_hangGliderGO = value; }
    public string Name { get => m_name; }
    public bool IsLanded { get => m_isLanded; }

    private void OnEnable()
    {
        if(m_handleByAI == false)
        {

            m_shakeBehaviorCamera = transform.Find("Main Camera").GetComponent<ShakeBehavior>();
            m_particleTrails = m_hangGliderGO.transform.Find("ParticleTrails").GetComponent<ParticleSystem>();
            SoundManager.Instance.playSoundFlight();
            m_name = "You";
        }



        m_rb = GetComponent<Rigidbody>();

        //Convert the angle from degrees to radian (The degrees are easier to visualize)
        m_slopeAngle = (m_slopeAngle * Mathf.PI) / 180.0f;
        m_diveAngle = (m_diveAngle * Mathf.PI) / 180.0f;
        m_riseAngle = (m_riseAngle * Mathf.PI) / 180.0f;

        m_forceDescentInit = m_forceDescent;
        m_forceDiveInit = m_forceDive;
        m_forceRiseInit = m_forceRise;
        m_riseFactor = 1.1f;

        m_oldPosition = m_hangGliderGO.transform.position;
        m_oldDiff = Vector3.zero;

        m_pressIsHold = false;
        m_inTransition = false;
        m_isLanded = false;
        m_isWaitingForDiving = false;

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);

        if (m_handleByAI)
        {
            Random.InitState(m_randomSeed);
            m_forceDescent *= 5.0f;
            StartCoroutine(catchUpPlayer());
            //change the color of the wing and the character
            m_associatedColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            m_hangGliderGO.transform.Find("Bar").GetComponent<MeshRenderer>().material.color = m_associatedColor;
            m_hangGliderGO.transform.Find("Character/Stickman_heads_sphere.014").GetComponent<SkinnedMeshRenderer>().material.color = m_associatedColor; 
        }

        m_rb.velocity = m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);

            
    }

    private void Update()
    {

        updateRotationHangGlider();

        //If the hangGlider reachTheFloor
        if(m_hangGliderGO.transform.position.y <= 2.0f || (m_isLanded && m_hangGliderGO.transform.position.y <= 3.0f))
        {
            landOnFloor();
            return;
        }

        if(m_handleByAI)
        {

            if (m_pressIsHold == false && m_isWaitingForDiving == false && m_isRisingBoostOrDiveReduce == false)
                StartCoroutine(waitAndDive());

            return;
        }



#if UNITY_EDITOR
        if (m_inTransition == false && m_isRisingBoostOrDiveReduce == false)
        {
            //hold button
            if (Input.GetKey(KeyCode.Space) && m_pressIsHold == false)
                holdToDive();
            

            //release button
            if (Input.GetKey(KeyCode.Space) == false && m_pressIsHold)
                releaseToRise();
            

        }
#endif


        if (m_inTransition == false && Input.touchCount > 0 && m_isRisingBoostOrDiveReduce == false)
        {
            Touch touch = Input.GetTouch(0);
            //hold button
            if (touch.phase == TouchPhase.Began && m_pressIsHold == false)
                holdToDive();

            //release button
            if (touch.phase == TouchPhase.Ended && m_pressIsHold)
                releaseToRise();
        }

        /* The input touch could be faster than the first transition. 
         * However, it is necessary to release at the end of the touch.
         * Therefore, a coroutine will wait the end of the transition,
         * and release.*/
        if (m_inTransition == true && Input.touchCount >0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && m_pressIsHold)
            {
                StartCoroutine(releaseAtTheEndOfTransition());
            }
        }

        var main = m_particleTrails.main;
        main.startSpeed = m_forceDescent / 10.0f;

        if (m_pressIsHold)
        {
            float gaugeAmount = Mathf.Min((m_posOriginToDive.y - transform.position.y) / UIManager.Instance.MeasureFullGauge, 1.0f);;
            UIManager.Instance.setGauge(gaugeAmount);
        }

    }


    private void LateUpdate()
    {
        if (GameManager.Instance.HangGliderPlayer.transform.position.y <= 10.0f)
        {
            Vector3 newPos = new Vector3(GameManager.Instance.MainCameraGO.transform.position.x, 10.0f, GameManager.Instance.MainCameraGO.transform.position.z);
            GameManager.Instance.MainCameraGO.transform.position = newPos;
        }
    }

    private void holdToDive()
    {
        if(m_handleByAI == false)
        {
            UIManager.Instance.setGauge(0.0f);
            SoundManager.Instance.playSoundDive();
        }

        Pair<float, float> lengthSide;
        Vector3 forceDirection;

        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceDive * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 1.0f));
        m_posOriginToDive = transform.position;
        m_pressIsHold = true;

        //visual effect
        if (m_handleByAI == false)
            StartCoroutine(shakeCamera());
        else
            StartCoroutine(waitAndRelease());

    }

    private void landOnFloor()
    {
        Vector3 currentPosHangGlider = m_hangGliderGO.transform.position;

        if (currentPosHangGlider.y <= 2.5f)
            m_hangGliderGO.transform.position = Vector3.MoveTowards(currentPosHangGlider, new Vector3(currentPosHangGlider.x, 3.0f, currentPosHangGlider.z), 0.5f);

        if (m_isLanded == false)
        {

            myStopAllCoroutine();
            StartCoroutine(landOnFloorCoroutine());
        }

        SoundManager.Instance.stopSoundFlight();
        m_isLanded = true;

    }

    private IEnumerator landOnFloorCoroutine()
    {
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, transform.forward, 0.5f));
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, Vector3.zero, 2.5f));

        GameManager.Instance.calculateScoreForLandedHangGlider(gameObject);

        if (m_handleByAI == false)
        {
            GameManager.Instance.endLevel();
        }
    }


    /*The opponent start with a slightly drawback to let the player 
      recognize his hangglider. Therefore they have to catch up the 
      player.*/
    private IEnumerator catchUpPlayer()
    {
        while(m_hangGliderGO.transform.position.z < GameManager.Instance.HangGliderPlayer.transform.position.z)
            yield return new WaitForSeconds(0.1f);

        m_forceDescent = m_forceDescentInit;

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        m_rb.velocity = m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
    }

    private void releaseToRise()
    {
        if(m_handleByAI == false)
            SoundManager.Instance.stopSoundDiveAndPlaySoundRise();
        

        Pair<float, float> lengthSide;
        Vector3 forceDirection;

        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceRise * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 0.5f));
        float height = m_posOriginToDive.y - transform.position.y;
        StartCoroutine(rise(height));

        if(m_forceDescent < 0.1f * m_forceDescentMax)
            changeForces(1.0f + 0.05f*height);
        else
            changeForces(1.0f + 0.01f * height);

        m_pressIsHold = false;
    }

    public void riseBoost(float height)
    {
        m_isRisingBoostOrDiveReduce = true;
        myStopAllCoroutine();

        /*if (m_handleByAI == false)
            SoundManager.Instance.stopSoundDiveAndPlaySoundRise();*/

        Pair<float, float> lengthSide;
        Vector3 forceDirection;

        lengthSide = angleToLength(m_diveAngle);
        forceDirection = m_forceRise*2.0f * (transform.forward * lengthSide.first + transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, forceDirection, 0.5f));


        if (m_pressIsHold)
            height += Vector3.Distance(m_posOriginToDive, transform.position);

        StartCoroutine(rise(height, true));

        if (m_forceDescent < 0.1f * m_forceDescentMax)
            changeForces(1.0f + 0.05f * height);
        else
            changeForces(1.0f + 0.01f * height);

        m_pressIsHold = false;

    }

    public void diveReduce(float angleToAdd)
    {
        m_isRisingBoostOrDiveReduce = true;
        myStopAllCoroutine();

        Pair<float, float> lengthSide = angleToLength(m_slopeAngle + angleToAdd);
        Vector3 direction = m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);

        float timeTransition = 1.0f;
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, direction, timeTransition));
        StartCoroutine(diveReduceCoroutine(timeTransition));

        m_pressIsHold = false;
    }

    public void changeForces(float factor)
    {

        if (m_forceDescent * factor > m_forceDescentMax)
            m_forceDescent = m_forceDescentMax;
        else if (m_forceDive * factor < m_forceDescentInit)
            m_forceDescent = m_forceDescentInit;
        else
            m_forceDescent *= factor;

        if (m_forceDive * factor > m_forceDiveMax)
            m_forceDive = m_forceDiveMax;
        else if (m_forceDive * factor < m_forceDiveInit)
            m_forceDive = m_forceDiveInit;
        else
            m_forceDive *= factor;

        if (m_forceRise * factor > m_forceRiseMax)
            m_forceRise = m_forceRiseMax;
        else if (m_forceRise * factor < m_forceRiseInit)
            m_forceRise = m_forceRiseInit;
        else
            m_forceRise *= factor;
    }

    private void updateRotationHangGlider()
    {
        //rotate the hand glider to make it look where it goes.
        Vector3 diff = m_hangGliderGO.transform.position - m_oldPosition;

        //only use lookAt when he changes his direction
        if (Utilities.isCloseEpsilonVec3(diff, m_oldDiff, 0.01f) == false)
            m_hangGliderGO.transform.LookAt(m_hangGliderGO.transform.position + diff);

        m_oldPosition = m_hangGliderGO.transform.position;
        m_oldDiff = diff;
    }

    //convert an angle in a normalized length for adjacent and opposite side of a triangle
    private Pair<float,float> angleToLength(float angle)
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

        for(int i=0; i<30*durationTransition; i++)
        {
            
            v1Proj = Utilities.rotate(v1Proj, rotationOfAngle);

            //we update the scale of the vector
            if(v1Norm < v2Norm)
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

    private IEnumerator releaseAtTheEndOfTransition()
    {
        while(m_inTransition)
        {
            yield return new WaitForEndOfFrame();
        }

        releaseToRise();
    }

    private IEnumerator rise(float heightDive, bool isRisingWithBoost = false)
    {

        Vector3 posOriginRise = transform.position;
        while (transform.position.y - posOriginRise.y  < m_riseFactor*heightDive)
        {
            if(m_handleByAI == false)
            {

                float gaugeAmount = Mathf.Max((m_posOriginToDive.y - transform.position.y)/ UIManager.Instance.MeasureFullGauge,0.0f);
                UIManager.Instance.setGauge(gaugeAmount);
            }
            yield return new WaitForFixedUpdate();
        }

        UIManager.Instance.setGauge(0.0f);
        Pair<float, float> lengthSide = angleToLength(m_slopeAngle);
        Vector3 direction =  m_forceDescent * (transform.forward * lengthSide.first - transform.up * lengthSide.second);
        StartCoroutine(transitionBetweenVelocity(m_rb.velocity, direction, 1.0f));

        if(m_handleByAI == false)
        {
            SoundManager.Instance.stopSoundRise();
        }

        if (isRisingWithBoost)
            m_isRisingBoostOrDiveReduce = false;
    }

    private IEnumerator shakeCamera()
    {
        float durationShake = 0.05f;
        float magnitudeShake = 0.01f;

        while(m_pressIsHold)
        {
            m_shakeBehaviorCamera.TriggerShake(durationShake, magnitudeShake);
            magnitudeShake += (0.001f * m_forceDive);
            yield return new WaitForSeconds(durationShake);
        }
    }

    private IEnumerator waitAndRelease()
    {
        while (m_inTransition)
            yield return new WaitForSeconds(0.1f);

        Random.InitState(m_randomSeed);
        yield return new WaitForSeconds(Random.Range(0.5f, 3.0f));
        releaseToRise();
    }

    private IEnumerator waitAndDive()
    {
        m_isWaitingForDiving = true;
        Random.InitState(m_randomSeed);
        yield return new WaitForSeconds(Random.Range(2.0f, 4.0f));
        holdToDive();

        m_isWaitingForDiving = false;
    }

    private IEnumerator diveReduceCoroutine(float timeForReduce)
    {
        yield return new WaitForSeconds(timeForReduce);
        m_isRisingBoostOrDiveReduce = false;
    }

    public void myStopAllCoroutine()
    {
        UIManager.Instance.setGauge(0.0f);

        if (m_handleByAI == false)
            SoundManager.Instance.stopAllAudioSources();
        
        StopAllCoroutines();
    }

}
