using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class playerDeath : NetworkBehaviour
{
    public Transform respawnPoint;
    public GameObject weapon;
    public GameObject respawnButton;
    public bool isDead = false;
    private Transform target;

    private void Start() 
    {
        respawnPoint = GameObject.Find("spawnPoint1").transform;
    }

    public void Die(Transform _target)
    {
        target = _target;
        isDead = true;
        transform.Find("playerModel").gameObject.SetActive(false);
        respawnButton.SetActive(true);
        weapon.SetActive(false);
        if(isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    [Command]
    public void respawnButtonClick()
    {
        Respawn();
    }
    [ClientRpc]
    private void Respawn()
    {
        target.Find("playerModel").gameObject.SetActive(true);
        target.Find("playerModel").GetComponent<MeshRenderer>().material.color = Color.blue;
        target.GetComponent<playerDeath>().isDead = false;
        target.GetComponent<playerDeath>().respawnButton.SetActive(false);
        target.GetComponent<playerDeath>().weapon.SetActive(true);
        //target.transform.position = target.GetComponent<playerDeath>().respawnPoint.position;
        Cursor.lockState = CursorLockMode.Locked;
    }
}