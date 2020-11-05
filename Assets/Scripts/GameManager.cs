using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int m_levelId;
    private static GameManager m_instance;
    [SerializeField]
    private GameObject m_hangGliderPlayer;
    [SerializeField]
    private GameObject m_mainCameraGO;
    [SerializeField]
    private GameObject m_playersGO;
    private List<Pair<Movements,float>> m_movementsPlayers = new List<Pair<Movements, float>>();
    
    #region Objectives_variables
    [SerializeField]
    private Objective[] m_objective = new Objective[3];
    private float m_highestDive = 0.0f;
    private int m_nbrOfCoinLevel = 0;
    private uint m_rankPlayer;
    private float m_distanceTraveledPlayer;
    #endregion

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

    public GameObject HangGliderPlayer { get => m_hangGliderPlayer; }
    public List<Pair<Movements, float>> MovementsPlayers { get => m_movementsPlayers;}
    public GameObject MainCameraGO { get => m_mainCameraGO; }
    public int NbrOfCoinLevel { get => m_nbrOfCoinLevel; set => m_nbrOfCoinLevel = value; }
    public Objective[] Objective { get => m_objective; set => m_objective = value; }
    public int LevelId { get => m_levelId; }

    public void Awake()
    {
        int nbrOfChild = m_playersGO.transform.childCount;
        for (int i = 0; i < nbrOfChild; i++)
        {
            Movements mov = m_playersGO.transform.GetChild(i).GetComponent<Movements>();
            m_movementsPlayers.Add(new Pair<Movements, float>(mov, 0.0f));
        }

        //found the objectives which are binded with this level
        List<Objectives3Achieve> currentObjectives = DataManager.Instance.ObjectivesAreAchieve;
        foreach (var objectives3Achieve in currentObjectives)
        {
            if(objectives3Achieve.levelAssociated == m_levelId)
            {
                m_objective[0].isAchieve = objectives3Achieve.objective1isAchieve;
                m_objective[1].isAchieve = objectives3Achieve.objective2isAchieve;
                m_objective[2].isAchieve = objectives3Achieve.objective3isAchieve;
                break;
            }
        }
    }

    public float calculateScore()
    {
        return m_hangGliderPlayer.transform.position.z;
    }

    public void endLevel()
    {
        definedScore();
        sortPlayerByScore();

        //The check has to be after the 2 lasts line, because they updates scores.
        checkIfObjectivesAreCompleted();
        UIManager.Instance.plotPanelEndGo();
    }

    public void calculateScoreForLandedHangGlider(GameObject go)
    {
        if (m_movementsPlayers != null)
        {
            
            foreach (Pair<Movements, float> player in m_movementsPlayers)
            {
                /*WARNING use gameObject.name to identify object but not sure to be unique. 
                 * Therefore take care to the uniqueness of them.
                */
                if (go.name == player.first.gameObject.name)
                {
                    player.second = player.first.HangGliderGO.transform.position.z;
                }

                if(go.tag == "Player")
                    m_distanceTraveledPlayer = player.first.HangGliderGO.transform.position.z;
            }
        }
    }

    private void sortPlayerByScore()
    {
        m_movementsPlayers.Sort(
            delegate (Pair<Movements, float> p1, Pair<Movements, float> p2)
            {
                return p1.second > p2.second ? -1 : 1;
            }
        );

        uint index = 1;
        foreach(var player in m_movementsPlayers)
        {
            if (player.first.gameObject.tag == "Player")
            {
                m_rankPlayer = index;
            }

            index++;
        }
    }

    private void definedScore()
    {
        if (m_movementsPlayers != null)
        {
            float score = m_hangGliderPlayer.transform.parent.transform.position.z;
            foreach (Pair<Movements, float> mov in m_movementsPlayers)
            {
                //value depending of the length of the terrain
                if(mov.first.IsLanded == false)
                    mov.second = score + Random.Range(10.0f, 500.0f);
                
            }
        }
    }

    public void restartLevel()
    {
        SceneManager.LoadScene(m_levelId);
    }

    public void goToMainMenu()
    {

        SceneManager.LoadScene(1);
    }

    public void goToNextLevel()
    {
        //add a check here
        SceneManager.LoadScene(m_levelId++);
    }

    public void launchPlayers()
    {
        if(m_movementsPlayers != null)
        {
            foreach(Pair<Movements, float> mov in m_movementsPlayers)
            {
                mov.first.enabled = true;
            }
        }
    }

    public void highestDive(float heightDive)
    {
        if (heightDive > m_highestDive)
            m_highestDive = heightDive;
    }

    #region Objectives

    public void checkIfObjectivesAreCompleted()
    {
        foreach (Objective objective in m_objective)
        {
            if (objective.isAchieve == false)
            {
                if (objective.condition.Invoke())
                {
                    objective.isAchieve = true;
                }
                DataManager.Instance.NbrOfStar++;
            }
        }

        List<Objectives3Achieve> currentObjectives = DataManager.Instance.ObjectivesAreAchieve;
        foreach (var objectives3Achieve in currentObjectives)
        {
            if (objectives3Achieve.levelAssociated == m_levelId)
            {
                objectives3Achieve.objective1isAchieve = m_objective[0].isAchieve;
                objectives3Achieve.objective1isAchieve = m_objective[1].isAchieve;
                objectives3Achieve.objective1isAchieve = m_objective[2].isAchieve;
                break;
            }
        }
    }

    public bool hasEarnedMoreCoinsThan(int x)
    {
        return m_nbrOfCoinLevel >= x;
    }

    public bool hasTravelledMoreThan(float distance)
    {
        return distance < m_distanceTraveledPlayer;
    }

    public bool isRankedGreaterThan(int x)
    {
        return m_rankPlayer <= x;
    }

    public bool isHighestDiveGreaterThan(float height)
    {
        return m_highestDive >= height;
    }

    #endregion

}

[System.Serializable]
public class Objective
{
    public Condition condition;
    public string associatedText;
    public bool isAchieve;
}


[System.Serializable]
public class Objectives3
{
    public Objective[] objectives = new Objective[3];
    public uint levelAssociated;
}
