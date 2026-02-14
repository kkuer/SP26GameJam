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
    public GameObject normalBullet;
    //Bullet list
    public List<GameObject> bullets = new List<GameObject>();
    //Enum for bullet emission selection
    public enum GunType
    {
        Standard, 
        Burst,
        Pierce,
        Bomb
    }
    public GunType currentGun;
    //Ints and floats for the attack bursts (think battle rifle three-tap rat-tat-tat)
    public int burstAmount;
    public float burstWait;

    void Update()
    {
        //transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
        if (isLoaded)
        {
            StartCoroutine(SpinEmitter());
        }
    }

    IEnumerator SpinEmitter()
    {
        if (isLoaded)
        {
            isLoaded = false;
            Instantiate(normalBullet, transform.position, transform.rotation);
            yield return new WaitForSeconds(reloadTime);
            isLoaded = true;
        }
    }
}
