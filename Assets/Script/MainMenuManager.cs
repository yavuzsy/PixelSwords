using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Pun;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    public AudioClip buttonS;
    public Button juiceButton; // Juice butonu
    public Text juiceButtonText; // Juice butonunun metni
    public Text playButtonText;
    public Button playButton; // Play butonu
    public Button creditButton; // Credit butonu
    public Button exitButton; // Çıkış butonu
    private bool isOn;

    //public GameObject waitingPanel;
    private bool player1Ready = false;
    private bool player2Ready = false;

    public GameObject roomPanel;
    public Button findRoomB, createRoomB;
    public Text quickSrcText, createLogText;

    private void Start()
    {
        //--Update_2402061429TKL--//
        //--An additional update has been made to this project.
        //--The aim is to provide online connection and enable players to experience it remotely simultaneously.
        //--Unless necessary, each update code is added to the bottom of the script.
        PhotonNetwork.ConnectUsingSettings();
        PlayerPrefs.DeleteAll();

        juiceButton.onClick.AddListener(OnClickJuiceButton);
        playButton.onClick.AddListener(OnClickPlayButton);
        creditButton.onClick.AddListener(OnClickCreditButton);
        exitButton.onClick.AddListener(OnClickExitButton);
        
        findRoomB.onClick.AddListener(RandomGameB);
        createRoomB.onClick.AddListener(CreateRoomB);
            
        // Juice butonunun durumunu yükle
        LoadJuiceButtonState();
        juiceButtonText.text = " ON";
        juiceButtonText.color = Color.green;
        isOn = true;
        PlayerPrefs.SetInt("JuiceButtonState", isOn ? 1 : 0);
    }

    private void OnClickJuiceButton()
    {
        this.gameObject.GetComponent<AudioSource>().PlayOneShot(buttonS);
        isOn = (PlayerPrefs.GetInt("JuiceButtonState", 0) == 1);

        // Durumu tersine çevir
        isOn = !isOn;

        // Durumu kaydet
        PlayerPrefs.SetInt("JuiceButtonState", isOn ? 1 : 0);

        // Duruma göre metni ve rengi güncelle
        if (isOn)
        {
            juiceButtonText.text = " ON";
            juiceButtonText.color = Color.green;
        }
        else
        {
            juiceButtonText.text = " OFF";
            juiceButtonText.color = Color.red;
        }
    }
    
    public void OnClickPlayButton()
    {
        this.gameObject.GetComponent<AudioSource>().PlayOneShot(buttonS);
        roomPanel.SetActive(true);
    }

    [PunRPC]
    private void GameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            player1Ready = true;
            photonView.RPC("UpdatePlayer1ReadyStatus", RpcTarget.All, true);
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        else
        {
            player2Ready = true;
            photonView.RPC("UpdatePlayer2ReadyStatus", RpcTarget.All, true);
        }
        //isready vardı sildim çünkü neden olmasın :/
    }

    private void OnClickCreditButton()
    {
        this.gameObject.GetComponent<AudioSource>().PlayOneShot(buttonS);
        Application.OpenURL("https://yavuzsy.itch.io");
    }

    private void OnClickExitButton()
    {
        this.gameObject.GetComponent<AudioSource>().PlayOneShot(buttonS);
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }

    private void LoadJuiceButtonState()
    {
        bool isOn = (PlayerPrefs.GetInt("JuiceButtonState", 0) == 1);
        if (isOn)
        {
            juiceButtonText.text = "ON";
            juiceButtonText.color = Color.green;
        }
        else
        {
            juiceButtonText.text = "OFF";
            juiceButtonText.color = Color.red;
        }
    }

    //--ProjectUpdateCode_2402061429TKL--//
    public override void OnConnectedToMaster()
    {
        playButtonText.text = ">> Play <<";
        playButton.interactable = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        quickSrcText.text=("> Room is not found! Please create a room.");
        
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            quickSrcText.text=("> Joined Room \nStarting game.. ");
        }
        photonView.RPC("GameStart", RpcTarget.All);
    }

    public void RandomGameB()
    {
        quickSrcText.text=("> Searching room..");
        PhotonNetwork.JoinRandomRoom(); 
    }

    public void CreateRoomB()
    {
        createLogText.text = "> Room creating..";
        PhotonNetwork.CreateRoom("gameRoom", new RoomOptions { MaxPlayers = 2, IsOpen = true, IsVisible = true }, TypedLobby.Default);
        //Odanın sorunsuz oluştuğunu kabul edelim yoksa bi ton şey var tabi :/
        //photonView.RPC("GameStart", RpcTarget.All);
    }

    [PunRPC]
    void UpdatePlayer1ReadyStatus(bool ready)
    {
        player1Ready = ready;
        IsReady();
    }

    [PunRPC]
    void UpdatePlayer2ReadyStatus(bool ready)
    {
        player2Ready = ready;
        IsReady();
    }

    [PunRPC]
    void IsReady()
    {
        if (player1Ready && player2Ready)
        {
            // Her iki oyuncu da hazır olduğunda
            PhotonNetwork.LoadLevel("GameLevel");
        }
        else
        {
            // Bekleme panelini göster
            createLogText.text = "> Player2 waiting..";
        }
    }
}