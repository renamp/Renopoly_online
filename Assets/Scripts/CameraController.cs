using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public Transform objCamera;

    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;

        GameObject temp = GameObject.FindWithTag("MainCamera");
        Debug.Log(temp.tag);
        objCamera = temp.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        if (!isLocalPlayer) return;

        if ( GUI.RepeatButton(new Rect(100,300, 120,80), "Move Camera"))
        {
            objCamera.RotateAround(transform.position, transform.up, 20 * Time.deltaTime);
            objCamera.LookAt(transform.position);

        }
    }
}
