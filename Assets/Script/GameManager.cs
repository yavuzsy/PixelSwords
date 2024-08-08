using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    private AudioSource managerS;
    public AudioClip gameS, winS, buttonS;
    public float countdownDuration = 120f; // Geri sayım süresi (saniye)
    public Text countdownText, lvlText, p1ScoreT, p2ScoreT;
    private int lvl, p1Score, p2Score;
    public GameObject Lvl1;
    public GameObject Lvl2;
    public GameObject Lvl3;
    public GameObject Lvl4;
    public GameObject Lvl5;
    public GameObject startPanel; // Başlangıç paneli
    public GameObject WinRedPanel; // Kırmızı kazanan paneli
    public GameObject WinBluePanel; // Mavi kazanan paneli
    public GameObject WinNullPanel; // Berabere Paneli
    private float playTime; // Geri sayım zamanlayıcısı
    private bool isGameRunning; // Oyunun çalışıp çalışmadığını kontrol etmek için
    private bool isJuice; // Juice durumunu tutmak için
    private GameObject winp;
    public Button mainmenu1, mainmenu2, mainmenu3, mainmenu4, mainmenu5;
    public Button contiune1, contiune2, contiune3;
    public GameObject P1Cntrl,P2Cntrl;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        
        lvl = PlayerPrefs.GetInt("Level", 1);
        p1Score = PlayerPrefs.GetInt("p1S", 0);
        p2Score = PlayerPrefs.GetInt("p2S", 0);
        p1ScoreT.text = "Score: " + p1Score.ToString();
        p2ScoreT.text = p2Score.ToString() + " :Score";
        managerS = GetComponent<AudioSource>();
        managerS.clip = gameS;
        managerS.Play();
        lvlText.text = "FIGHT!";
        if (lvl == 1)
        {
            Lvl1.SetActive(true);
            //lvlText.text = "LEVEL 1";
        }
        else if (lvl == 2)
        {
            Lvl2.SetActive(true);
            //lvlText.text = "LEVEL 2";
        }
        else if (lvl == 3)
        {
            Lvl3.SetActive(true);
            //lvlText.text = "LEVEL 3";
        }
        else if (lvl == 4)
        {
            Lvl4.SetActive(true);
            //lvlText.text = "LEVEL 4";
        }
        else if (lvl == 5)
        {
            Lvl5.SetActive(true);
            //lvlText.text = "LEVEL 5";
        }
        else
        {
            lvl = 1;
            Lvl1.SetActive(true);
            //lvlText.text = "LEVEL 1";
        }
    }

    private void Start()
    {
        playTime = countdownDuration;
        isGameRunning = true;
        isJuice = PlayerPrefs.GetInt("JuiceButtonState", 0) == 1;
        if (startPanel != null)
        {
            startPanel.SetActive(true);

            if (isJuice)
            {
                StartCoroutine(MovePanelDown(startPanel));
            }
            else
            {
                StartCoroutine(DisablePanel(startPanel));
            }
        }
        
        if (PhotonNetwork.IsMasterClient) {
            P1Cntrl.SetActive(true);
            P2Cntrl.SetActive(false);
        }
        else {
            P2Cntrl.SetActive(true);
            P1Cntrl.SetActive(false); 
        }

        mainmenu1.onClick.AddListener(homepage);
        mainmenu2.onClick.AddListener(homepage);
        mainmenu3.onClick.AddListener(homepage);

        contiune1.onClick.AddListener(nextlevel);
        contiune2.onClick.AddListener(nextlevel);
        contiune3.onClick.AddListener(nextlevel);
    }

    void Update()
    {
        if (isGameRunning)
        {
            playTime -= Time.deltaTime;
            // Geri sayım metnini güncelle
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            // Geri sayım süresi dolduysa oyunu durdur
            if (playTime <= 0f)
            {
                isGameRunning = false;
                countdownText.text = "00:00";
                //lvl++;
                //PlayerPrefs.SetInt("Level", lvl);
                managerS.Stop();
                managerS.PlayOneShot(winS);
                winner(WinNullPanel);
            }
        }
    }

    public void winner(GameObject winP)
    {
        isGameRunning = false;
        /*if (lvl == 5)
        {
            if (winP == WinRedPanel)
            {
                p1Score++;
                PlayerPrefs.SetInt("p1S", p1Score);
            }
            else if (winP == WinBluePanel)
            {
                p2Score++;
                PlayerPrefs.SetInt("p2S", p2Score);
            }

            StartCoroutine(nameof(ShowResult));
        }*/
        //else
        {
            //lvl++;
            //PlayerPrefs.SetInt("Level", lvl);

            /*if (winP == WinRedPanel)
            {
                p1Score++;
                PlayerPrefs.SetInt("p1S", p1Score);
            }
            else if (winP == WinBluePanel)
            {
                p2Score++;
                PlayerPrefs.SetInt("p2S", p2Score);
            }*/

            managerS.Stop();
            managerS.PlayOneShot(winS);
            PlayerPrefs.SetInt("notPlayable", 1);

            winp = winP;
            if (isJuice)
                StartCoroutine(nameof(ShowWinPanel));
            else
            {
                winp.SetActive(true);
            }
        }
    }

    IEnumerator MovePanelDown(GameObject panel)
    {
        yield return new WaitForSeconds(1f); // Panelin ekranda görünmesi için 1 saniye bekle

        while (panel.transform.position.y > -Screen.height)
        {
            panel.transform.Translate( Time.deltaTime*1000f*Vector3.down ); // Yavaşça aşağıya kaydır
            yield return null;
        }

        // Panel ekranın dışına çıktığında durdur ve kapat
        panel.transform.position = new Vector3(panel.transform.position.x, -Screen.height, panel.transform.position.z);
        panel.SetActive(false);
    }

    IEnumerator DisablePanel(GameObject panel)
    {
        yield return new WaitForSeconds(1f); // Panelin ekranda görünmesi için 1 saniye bekle
        panel.SetActive(false); // Paneli kapat
    }

    IEnumerator ShowWinPanel()
    {
        yield return new WaitForSeconds(2f);
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

    IEnumerator ShowResult()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("GameResult");
    }

    void homepage()
    {
        managerS.GetComponent<AudioSource>().PlayOneShot(buttonS);
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(0);
    }

    void nextlevel()
    {
        managerS.GetComponent<AudioSource>().PlayOneShot(buttonS);
        PlayerPrefs.SetInt("notPlayable", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}