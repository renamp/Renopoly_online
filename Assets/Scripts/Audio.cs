using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Audio : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> audioList;

    private int count, id, max;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
        //id = 1;
        max = audioList.Count;

        Random.InitState(System.DateTime.Now.Millisecond);
        id = Random.Range(0, max);

        audioSource.clip = audioList[id];
        audioSource.Play();
        count = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        if( !audioSource.isPlaying && count < 2)
        {
            audioSource.clip = audioList[id];
            audioSource.Play();
            count++;
        }
        else if(!audioSource.isPlaying && count == 2)
        {
            //if (++id >= max) id = 0;
            id = Random.Range(0, max);
            audioSource.clip = audioList[id];
            audioSource.Play();
            count = 0;
        }
        
    }
}
