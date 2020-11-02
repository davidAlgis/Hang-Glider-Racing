using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;
    [SerializeField]
    private GameObject m_hangGliderPlayer;
    //opponents
    [SerializeField]
    private int m_nbrOfOpponents;
    [SerializeField]
    private GameObject m_opponentGO;
    [SerializeField]
    private List<Material> m_matHandGlider = new List<Material>();



    public static GameManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("GameManager").GetComponent<GameManager>();
            }
            return m_instance;
        }
    }

    public GameObject HangGliderPlayer { get => m_hangGliderPlayer; set => m_hangGliderPlayer = value; }

    public void Start()
    {
        Vector3 posPlayer = m_hangGliderPlayer.transform.position;

        for(int i=0;i<m_nbrOfOpponents;i++)
        {
            
            if(i< m_nbrOfOpponents)
            {
                if (i % 2 == 0)
                    posPlayer.y += 5.0f;
                else
                    posPlayer.x += 5.0f;
            }

            //Instantiate(m_opponentGO, posPlayer, m_opponentGO.transform.rotation);
        }
    }

}
