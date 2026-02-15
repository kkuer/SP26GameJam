using UnityEngine;
using System.Collections;

public class ExplosiveBullet : MonoBehaviour
{
    //Bullet speed
    public float bullSpeed;
    //Bullet particles
    public GameObject bulletSplash;
    //Bullet lifetime
    public float bulletLife;
    //Explosion death
    public GameObject bombBlast;
    void Start()
    {
        StartCoroutine(SelfDes());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * bullSpeed);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        Instantiate(bombBlast, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        Instantiate(bombBlast, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator SelfDes()
    {
        yield return new WaitForSeconds(bulletLife);
        Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
