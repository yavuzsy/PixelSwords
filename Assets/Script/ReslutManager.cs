using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReslutManager : MonoBehaviour
{
    public GameObject winp, WinnerP1, WinnerP2, WinnerNull;
    public AudioClip winS, buttonS;
    private bool isJuice, run = true;
    public Text winnerRed, winnerBlue, winnerNull;
    public Button main1, main2, main3;

    private void Awake()
    { 
        isJuice = PlayerPrefs.GetInt("JuiceButtonState", 0) == 1;
        main1.onClick.AddListener(homepage);
        main2.onClick.AddListener(homepage);
        main3.onClick.AddListener(homepage);
        
        run = true;
    }

    void Update()
    {
        if (PlayerPrefs.GetInt("p1S") > PlayerPrefs.GetInt("p2S") && run)
        {
            winnerRed.text = "RED " + PlayerPrefs.GetInt("p1S").ToString() + "-" + PlayerPrefs.GetInt("p2S") + " BLUE";
            winp = WinnerP1;
            gameObject.GetComponent<AudioSource>().Stop();
            gameObject.GetComponent<AudioSource>().PlayOneShot(winS);
            if (isJuice)
            {
                StartCoroutine(ShowWinPanel());
            }
            else
            {
                winp.SetActive(true);
            }

            run = false;
        }
        else if (PlayerPrefs.GetInt("p1S") < PlayerPrefs.GetInt("p2S") && run)
        {
            winnerBlue.text = "BLUE " + PlayerPrefs.GetInt("p2S").ToString() + "-" + PlayerPrefs.GetInt("p1S") + " RED";
            winp = WinnerP2;
            gameObject.GetComponent<AudioSource>().Stop();
            gameObject.GetComponent<AudioSource>().PlayOneShot(winS);
            if (isJuice)
            {
                StartCoroutine(ShowWinPanel());
            }
            else
            {
                winp.SetActive(true);
            }

            run = false;
        }
        else if (PlayerPrefs.GetInt("p1S") == PlayerPrefs.GetInt("p2S") && run)
        {
            winnerNull.text = "RED " + PlayerPrefs.GetInt("p1S").ToString() + "-" + PlayerPrefs.GetInt("p2S") + " BLUE";
            winp = WinnerNull;
            gameObject.GetComponent<AudioSource>().Stop();
            gameObject.GetComponent<AudioSource>().PlayOneShot(winS);
            if (isJuice)
            {
                StartCoroutine(ShowWinPanel());
            }
            else
            {
                winp.SetActive(true);
            }

            run = false;
        }
    }
    

    IEnumerator ShowWinPanel()
    {
        // Panel aktif hale getirilir
        winp.SetActive(true);

        // Panelin başlangıç konumu ve hedef konumu belirlenir
        Vector3 startPos = new Vector3(Screen.width / 2,
            Screen.height + winp.GetComponent<RectTransform>().rect.height, 0);
        Vector3 targetPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // Panelin kayma süresi ve hızı belirlenir
        float duration = 1f;
        float speed = 1.5f / duration;

        // Panel kaydırma işlemi
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            winp.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Panel hedef konumunda durur
        winp.transform.position = targetPos;
    }
    
    void homepage()
    {
        gameObject.GetComponent<AudioSource>().GetComponent<AudioSource>().PlayOneShot(buttonS);
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("HomePage");
    }
}