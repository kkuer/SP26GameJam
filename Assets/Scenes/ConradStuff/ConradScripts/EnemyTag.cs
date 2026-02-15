using UnityEngine;

public class EnemyTag : MonoBehaviour
{
    public float HP_Max;
    public float HP_Current;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // probably have some enemy data imported here

        HP_Current = HP_Max;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
