using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    public int musicIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.PlayMusic(musicIndex);
    }
}
