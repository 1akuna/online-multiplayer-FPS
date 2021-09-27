using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class playerDeath : NetworkBehaviour
{
    [Header("values you can set: ")]
    [SerializeField]private float playerHealth;
    [Space(10)]

    [HideInInspector]
    public Transform respawnPoint;
    [HideInInspector]
    public GameObject weapon;
    [HideInInspector]
    public GameObject respawnButton;
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    private Transform target;
    [HideInInspector]
    public float health;

    private void Start() 
    {
        respawnPoint = GameObject.Find("spawnPoint1").transform;
        health = playerHealth;
    }

    public void Die(Transform _target)
    {
        if(health <= 0)
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
        else
        {
            health--;
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
        target.GetComponent<playerDeath>().health = playerHealth;
        target.transform.position = target.GetComponent<playerDeath>().respawnPoint.position;
        Physics.SyncTransforms();
        target.Find("playerModel").gameObject.SetActive(true);
        target.Find("playerModel").GetComponent<MeshRenderer>().material.color = Color.blue;
        target.GetComponent<playerDeath>().isDead = false;
        target.GetComponent<playerDeath>().respawnButton.SetActive(false);
        target.GetComponent<playerDeath>().weapon.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
    }
}
