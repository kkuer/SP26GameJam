using UnityEngine;
using System.Collections;

public class NormalEmitter : MonoBehaviour
{
    //Rotate emitter speed
    //public float spinSpeed;
    //Delay between shots
    public float reloadTime;
    public bool isLoaded;
    //Normal bullet prefab
    public GameObject normalBullet;

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
