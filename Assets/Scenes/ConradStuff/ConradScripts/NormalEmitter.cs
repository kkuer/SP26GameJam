using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class NormalEmitter : MonoBehaviour
{
    //Delay between shots
    public float reloadTime;
    //Bool to wait between shots
    public bool isLoaded;
    //Bullet prefab
    public GameObject bulletPrefab;
    public GameObject spawnedBullet;
    //Bullet list
    public List<GameObject> bullets = new List<GameObject>();
    //Ints and floats for the attack bursts (think battle rifle three-tap rat-tat-tat)
    public int burstAmount;
    public float burstWait;
    //Fire rate for the emitter/spin speed for the swords
    public float fireRate;
    //Script that spins the sword emitter/bool that catches sword 
    public ConradDumbSpin swordCenter;
    public bool areSwords;
    //Bools that catch aura/thorns
    public bool isAura;
    public bool isThorn;

    private void Start()
    {
        if (areSwords)
        {
            swordCenter.spinSpeed = fireRate;
            spawnedBullet = Instantiate(bulletPrefab, transform.position, transform.localRotation);
            spawnedBullet.transform.SetParent(this.transform);
        }
        if (isThorn)
        {
            spawnedBullet = Instantiate(bulletPrefab, transform.position, transform.localRotation);
            spawnedBullet.transform.SetParent(this.transform);
        }
    }

    void Update()
    {
        if (!areSwords)
        {
            if (isLoaded)
            {
                StartCoroutine(SpinEmitter());
            }
        }
    }

    IEnumerator SpinEmitter()
    {
        if (isLoaded)
        {
            isLoaded = false;
            if (burstAmount > 1)
            {
                for (int i = 0; i < burstAmount; i++)
                {
                    spawnedBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                    if (isAura)
                    {
                        spawnedBullet.transform.SetParent(this.transform);
                    }
                    yield return new WaitForSeconds(burstWait);
                }
                yield return new WaitForSeconds(reloadTime);
                isLoaded = true;
            }
            else
            {
                spawnedBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                if (isAura)
                {
                    spawnedBullet.transform.SetParent(this.transform);
                }
                yield return new WaitForSeconds(reloadTime);
                isLoaded = true;
            }
        }
    }
}
