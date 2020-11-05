using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//handle the data which is keep between the different level
public class DataManager : MonoBehaviour
{
    

    private static DataManager m_instance;
    private int m_nbrOfCoin = 0;
    private int m_nbrOfLevel;
    [SerializeField]
    private List<Objectives3Achieve> objectivesAreAchieve = new List<Objectives3Achieve>();
    private int m_nbrOfStar = 0;
    public static DataManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("DataManager").GetComponent<DataManager>();
            }
            return m_instance;
        }
    }

    public int NbrOfCoin { get => m_nbrOfCoin; set => m_nbrOfCoin = value; }
    public List<Objectives3Achieve> ObjectivesAreAchieve { get => objectivesAreAchieve; set => objectivesAreAchieve = value; }
    public int NbrOfStar { get => m_nbrOfStar; set => m_nbrOfStar = value; }
    public int NbrOfLevel { get => m_nbrOfLevel; }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        m_nbrOfLevel = objectivesAreAchieve.Count;
    }
}



[System.Serializable]
public class Objectives3Achieve
{
    public uint levelAssociated;
    public bool objective1isAchieve = false;
    public bool objective2isAchieve = false;
    public bool objective3isAchieve = false;
}