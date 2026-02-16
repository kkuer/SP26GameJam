using UnityEngine;

public class PlayNusic : MonoBehaviour
{
    public int musicTrack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayMusic(musicTrack);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
