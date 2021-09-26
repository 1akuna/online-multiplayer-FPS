using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class weapons : NetworkBehaviour
{
    [SerializeField]private GameObject bulletTrail;
    [SerializeField]private GameObject bulletHolePrefab;
    [SerializeField]private Transform weaponHolster;
    [SerializeField]private Transform ADStransform;
    [SerializeField]private Transform hipFireTransform;
    [SerializeField]private TMP_Text bulletCountText;
    [SerializeField]private LayerMask canBeShot;
    [SerializeField]private Animator anim;

    [Header("Gun Cusomization: ")]
    [SerializeField]private Transform weapon;
    [SerializeField]private float fireRate;
    [SerializeField]private float bulletsInMag;
    [SerializeField]private float reloadTime;
    [SerializeField]private Transform gunBarrel;

    private AudioSource audioSource;
    private Camera mainCam;

    private bool isReloading;
    private float nextFire = 0f;
    private float bulletCount = 30f;

    private void Start() 
    {
        if(!isLocalPlayer){return;}
        audioSource = GameObject.Find("SFX").GetComponent<AudioSource>();
        mainCam = Camera.main;
    }

    private void Update() 
    {
        if(!isLocalPlayer){return;}
        bulletCountText.text = bulletCount.ToString();
        weaponADS();
        if(Input.GetKey(KeyCode.R))
        {
            StartCoroutine(reload());
        }
        if(Input.GetKey(KeyCode.Mouse0) && Time.time > nextFire && bulletCount > 0 && isReloading == false)
        {
            audioSource.Play();
            nextFire = Time.time + fireRate;
            bulletCount--;
            CmdShoot(mainCam.transform.position, mainCam.transform.forward, gunBarrel.position, gunBarrel.rotation);
        }
    }

    private IEnumerator reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        bulletCount = 30;
        isReloading = false;
    }

    [Command]
    private void CmdShoot(Vector3 _mainCamPos, Vector3 _mainCamDirection, Vector3 _gunBarrelPos, Quaternion _gunBarrelRot)
    {
        if(Physics.Raycast(_mainCamPos, _mainCamDirection, out RaycastHit hit, Mathf.Infinity))
        {
            //bulletHole
            GameObject bulletHoleInstance = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            NetworkServer.Spawn(bulletHoleInstance);
            bulletHoleInstance.transform.LookAt(hit.point + hit.normal);
            Destroy(bulletHoleInstance, 3f);

            //bulletTrail
            GameObject bulletTrailInstance = Instantiate(bulletTrail, _gunBarrelPos, _gunBarrelRot);
            NetworkServer.Spawn(bulletTrailInstance);
            spawnBulletTrail(bulletTrailInstance, _gunBarrelPos, hit.point);
            Destroy(bulletTrailInstance, 1f);

            //check if player was hit
            if(hit.transform.tag == "remotePlayer" || hit.transform.tag == "localPlayer")
            {
                Debug.Log(hit.transform.name + " has been hit");
                ClientChangeColor(hit.transform.gameObject);
            }
            if(hit.transform.tag == "target")
            {
                ClientChangeColor(hit.transform.gameObject);
            }
        }
    }
    [ClientRpc]
    private void spawnBulletTrail(GameObject bulletTrailInstance, Vector3 _gunBarrelPos, Vector3 hitPoint)
    {
        LineRenderer lineR = bulletTrailInstance.GetComponent<LineRenderer>();
        lineR.SetPosition(0, _gunBarrelPos);
        lineR.SetPosition(1, hitPoint);
    }

    private void weaponADS()
    {
        if(Input.GetKey(KeyCode.Mouse1))
        {
            //weapon.transform.position = ADStransform.position;
            anim.SetBool("isAiming", true);
        }
        else if(!Input.GetKey(KeyCode.Mouse1))
        {
            //weapon.transform.position = hipFireTransform.position;
            anim.SetBool("isAiming", false);
        }

    }
    [ClientRpc]
    private void ClientChangeColor(GameObject target)
    {
        if(target.tag == "target")
        {
            target.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        else
        {
            MeshRenderer targetMeshRenderer = target.transform.Find("playerModel").GetComponent<MeshRenderer>();
            Color playerStartColor = targetMeshRenderer.material.color;
            targetMeshRenderer.material.color = Color.red;
        }
    }
}
