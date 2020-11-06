using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelButtonScript : MonoBehaviour
{
    [SerializeField]
    private int m_nbrOfStarToUnlock = 0;
    private bool m_isLock = false;
    [SerializeField]
    private int m_indexLevelAssociated = 1;
    [SerializeField]
    private GameObject m_loadingGO;
    private Button m_levelButton;
    private GameObject m_levelText;
    private Image m_star1;
    private Image m_star2;
    private Image m_star3;
    private GameObject m_lock;
    private Text m_lockText;

    private void Awake()
    {
        m_levelButton = GetComponent<Button>();
        m_levelText = transform.Find("LevelText").gameObject;
        m_star1 = transform.Find("Star1").GetComponent<Image>();
        m_star2 = transform.Find("Star2").GetComponent<Image>();
        m_star3 = transform.Find("Star3").GetComponent<Image>();
        m_lock = transform.Find("Lock").gameObject;
        m_lockText = m_lock.transform.Find("LockText").GetComponent<Text>();


        if (m_nbrOfStarToUnlock <= DataManager.Instance.NbrOfStar)
        {
            areObjectivesAchieve();
            m_isLock = false;
            m_lock.SetActive(false);
            m_levelText.SetActive(true);
            m_levelButton.interactable = true;
            m_levelButton.onClick.AddListener(() => loadLevel(m_indexLevelAssociated));

        }
        else
        {
            m_levelButton.interactable = false;
            m_lockText.text = m_nbrOfStarToUnlock.ToString();
            m_isLock = true;

        }
    }

    private void areObjectivesAchieve()
    {
        foreach(var objective in DataManager.Instance.ObjectivesAreAchieve)
        {
            if(objective.levelAssociated == m_indexLevelAssociated)
            {
                Color yellow = new Color(0.99f, 1.0f, 0.25f);
                if (objective.objective1isAchieve)
                    m_star1.color = yellow;
                if (objective.objective2isAchieve)
                    m_star2.color = yellow;
                if (objective.objective3isAchieve)
                    m_star3.color = yellow;
                break;
            }
        }
    }

    private void loadLevel(int index)
    {
        m_loadingGO.SetActive(true);
        SceneManager.LoadScene(index);
    }

}
