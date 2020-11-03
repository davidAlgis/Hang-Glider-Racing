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


    public void Update()
    {
        m_distanceText.text = string.Format("{0,6:##0.00}", GameManager.Instance.calculateScore()) + " m";
    }

    public void debugText(string text)
    {
        m_textDebug.text += "\n" + text;
    }

    public void plotPanelEndGo()
    {
        m_panelEndGO.SetActive(true);
    }

    public void restartLevelButton()
    {
        GameManager.Instance.restartLevel();
    }


    public void setGauge(float x)
    {
        if (x < 0 || x > 1)
        { 
            Debug.LogWarning("Try to set the gauge to an impossible value " + x);
            x = 0.0f;
        }

        m_gaugeDive.fillAmount = x;
    }

    public void decreaseGaugeToZero()
    {
        StartCoroutine(decreaseGaugeToZeroCoroutine());
    }

    private IEnumerator decreaseGaugeToZeroCoroutine()
    {
        while(m_gaugeDive.fillAmount>0.0f)
        {
            m_gaugeDive.fillAmount -= 0.05f;
            yield return new WaitForSeconds(0.1f);
        }

    }
    


}
