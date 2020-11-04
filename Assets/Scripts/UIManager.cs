using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager m_instance;
    [SerializeField]
    private Text m_textDebug;
    [SerializeField]
    private Text m_distanceText;
    [SerializeField]
    private GameObject m_panelEndGO;
    [SerializeField]
    private Image m_gaugeDive;
    [SerializeField]
    private float m_measureFullGauge = 200.0f;
    [SerializeField]
    private float m_measureOnGaugeEach = 50.0f;
    private List<Pair<GameObject, float>> m_measureOnGauge = new List<Pair<GameObject,float>>();
    [SerializeField]
    private RectTransform m_circleCountRect;
    [SerializeField]
    private Text m_countText;
    [SerializeField]
    private List<Text> m_scoresTexts = new List<Text>();

    public float MeasureFullGauge { get => m_measureFullGauge; set => m_measureFullGauge = value; }

    public static UIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = GameObject.Find("UIManager").GetComponent<UIManager>();
            }
            return m_instance;
        }
    }

    public void Awake()
    {
        int nbrOfMeasure = (int)(m_measureFullGauge / m_measureOnGaugeEach);
        for (float i = 0;i < nbrOfMeasure; i++)
        {
            GameObject go = new GameObject();
            go.name = "Measure" + (i + 1) * m_measureOnGaugeEach;
            go.transform.parent = m_gaugeDive.gameObject.transform;
            RectTransform rect = go.AddComponent<RectTransform>();

            float minPosY = 1.0f - (float)(i + 1) * (m_measureOnGaugeEach/ m_measureFullGauge);
            rect.anchorMin = new Vector2(0.0f, minPosY-0.1f);
            rect.anchorMax = new Vector2(1.0f, minPosY);
            rect.anchoredPosition = new Vector2(0.0f, 0.0f);
            rect.offsetMin = new Vector2(-130.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);
            
            Text text = go.AddComponent<Text>();
            text.font = m_distanceText.font;
            text.color = new Color(0.98f, 0.78f, 0.1f);
            text.fontSize = 50;
            text.text = string.Format("{0,6:##0}", (i+1) * m_measureOnGaugeEach) + " m";
            go.SetActive(false);

            m_measureOnGauge.Add(new Pair<GameObject, float>(go, (float)(i + 1) * (m_measureOnGaugeEach / m_measureFullGauge)));
        }

        StartCoroutine(beginCounter());
    }

    public void Update()
    {
        m_distanceText.text = string.Format("{0,6:##0.0}", GameManager.Instance.calculateScore()) + " m";
    }

    public void debugText(string text)
    {
        m_textDebug.text += "\n" + text;
    }

    public void plotPanelEndGo()
    {
        m_panelEndGO.SetActive(true);
        m_distanceText.gameObject.SetActive(false);
        int i = 0;
        foreach (Pair<Movements, float> player in GameManager.Instance.MovementsPlayers)
        {
            if(i < m_scoresTexts.Count)
                m_scoresTexts[i].text = player.first.Name + " " + string.Format("{0,6:##0}", player.second) + " m";

            if(player.first.gameObject.tag == "Player")
                m_scoresTexts[i].gameObject.transform.parent.GetComponent<Image>().color = new Color(0.40f, 0.97f, 0.27f);
            

            i++;
        }
    }

    public void restartLevelButton()
    {
        GameManager.Instance.restartLevel();
    }

    public void setGauge(float x)
    {
        if (x < 0)
        {
            Debug.LogWarning("Try to set the gauge to an impossible value " + x);
            x = 0.0f;
        }
        else if(x > 1)
        {
            Debug.LogWarning("Try to set the gauge to an impossible value " + x);
            x = 1.0f;
        }

        foreach(var measure in m_measureOnGauge)
        {
            if (x >= measure.second)
            {

                //m_breakGlass.Play();
                measure.first.SetActive(true);
            }
            else
            {
                measure.first.SetActive(false);
            }
        }

        m_gaugeDive.fillAmount = x;
    }


    private IEnumerator beginCounter()
    {
        int count = 3;
        float iterationTime = 0.05f;

        while(count > 0)
        {
            m_circleCountRect.localRotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));

            float angleRotate = 0.0f;
            float reduceAngle = 180.0f / (1 / iterationTime);

            while (angleRotate <= 180.0f)
            {
                angleRotate += reduceAngle;
                m_circleCountRect.Rotate(new Vector3(0.0f,-reduceAngle,0.0f));
                yield return new WaitForSeconds(iterationTime);

            }
            count--;
            m_countText.text = "";
            m_circleCountRect.localRotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));
            if (count > 0)
                m_countText.text = count.ToString();
            
        }
        m_circleCountRect.gameObject.SetActive(false);

        //launch players at the end of the counters
        GameManager.Instance.launchPlayers();
    }


}
