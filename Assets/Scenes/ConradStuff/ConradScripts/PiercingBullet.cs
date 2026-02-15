using UnityEngine;
using System.Collections;

public class PiercingBullet : MonoBehaviour
{
    //Bullet speed
    public float bullSpeed;
    //Bullet particles
    public GameObject bulletSplash;
    //Bullet lifetime
    public float bulletLife;
    //Enemy tag script, checks for when determining piercing
    EnemyTag enemyTag;
    //Bullet jitter
    //public float bulletDeviation;
    void Start()
    {
        StartCoroutine(SelfDes());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * bullSpeed);
        //transform.position = new Vector3(transform.position.x + (Random.Range(-bulletDeviation, bulletDeviation)), transform.position.y + (Random.Range(-bulletDeviation, bulletDeviation)), transform.position.z + (Random.Range(-bulletDeviation, bulletDeviation)));
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null )
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        enemyTag = col.gameObject.GetComponent<EnemyTag>();
        if (enemyTag != null)
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        }
        else
        {
            Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    IEnumerator SelfDes()
    {
        yield return new WaitForSeconds(bulletLife);
        Instantiate(bulletSplash, this.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
