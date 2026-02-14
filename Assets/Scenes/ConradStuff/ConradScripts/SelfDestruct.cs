using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour
{
    public float selfTime;
    void Start()
    {
        StartCoroutine(SelfDes());
    }

    IEnumerator SelfDes()
    {
        yield return new WaitForSeconds(selfTime);
        Destroy(gameObject);
    }
}
