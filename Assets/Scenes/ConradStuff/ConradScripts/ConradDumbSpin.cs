using UnityEngine;

public class ConradDumbSpin : MonoBehaviour
{
    public float spinSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localEulerAngles = new Vector3(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y, this.transform.localEulerAngles.z + (spinSpeed * Time.deltaTime));
    }
}
