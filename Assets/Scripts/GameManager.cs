using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager m_instance;
    [SerializeField]
    private GameObject m_hangGliderPlayer;
    [SerializeField]
    private GameObject m_mainCameraGO;
    [SerializeField]
    private GameObject m_playersGO;
    private List<Pair<Movements,float>> m_movementsPlayers = new List<Pair<Movements, float>>();


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

    public void Awake()
    {
        int nbrOfChild = m_playersGO.transform.childCount;
        for (int i = 0; i < nbrOfChild; i++)
        {
            Movements mov = m_playersGO.transform.GetChild(i).GetComponent<Movements>();
            m_movementsPlayers.Add(new Pair<Movements, float>(mov, 0.0f));
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
            }
        }
    }

    public void sortPlayerByScore()
    {
        m_movementsPlayers.Sort(
            delegate (Pair<Movements, float> p1, Pair<Movements, float> p2)
            {
                return p1.second > p2.second ? -1 : 1;
            }
        );
    }

    public void definedScore()
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
        SceneManager.LoadScene(0);
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
}
