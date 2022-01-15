using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class testScript : NetworkBehaviour
{
    [SyncVar]
    public string vartxt = "Renan";


    private void OnGUI()
    {
        
    }


    

    //private void Update()
    //{
    //    //if (isLocalPlayer)
    //    {
    //       // GameObject.Find("__CanvasTest").GetComponentInChildren<Text>().text = txt;
    //    }
    //    //if(Input.GetMouseButton(0) && false)
    //    //{
    //    //    //drawn = true;
    //    //    //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            
    //    //}

        
    //}

    //public void testButton(int value)
    //{
    //    Debug.Log("Botao clickado: " + value);
    //}
}
