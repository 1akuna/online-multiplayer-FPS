using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class networkManagerScript : NetworkManager
{
    private NetworkManagerHUD networkManagerHUD;

    public override void Start()
    {
        base.Start();
        networkManagerHUD = gameObject.GetComponent<NetworkManagerHUD>();
    }

    public void playOffline()
    {
        SceneManager.LoadScene("testingRoomOffline");
        NetworkServer.dontListen = true;
        NetworkManager.singleton.StartHost();
    }
    public void playOnline()
    {
        SceneManager.LoadScene("testingRoom");
        networkManagerHUD.enabled = true;
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
