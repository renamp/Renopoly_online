using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class testScript2 : NetworkBehaviour
{
    GameObject obj;

    // private
    private string auxTxt;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnGUI()
    {
        if (isLocalPlayer || isClient)
        {
            if (GUI.Button(new Rect(360, 200, 70, 50), "Button 2"))
            {
                cmdInstanteate();
            }
            if (GUI.Button(new Rect(270, 200, 70, 50), "Button 1"))
            {
                //testButton();
                //Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                //txt= Application.persistentDataPath;
                cmdSetTxt(auxTxt);
            }
            auxTxt = GUI.TextField(new Rect(10, 450, 200, 40), auxTxt);

            if( obj!=null)
                GUI.Label(new Rect(10, 500, 500, 120), obj.GetComponent<testScript>().vartxt);
        }
    }

    [Command(requiresAuthority = false)]
    void cmdSetTxt(string txt1)
    {
        obj.GetComponent<testScript>().vartxt = txt1;
    }

    [Command(requiresAuthority = false)]
    void cmdInstanteate()
    {
        if (obj == null)
        {
            NewNetworkManager netmanager = GameObject.Find("NetworkManager").GetComponent<NewNetworkManager>();
            obj = Instantiate(netmanager.spawnPrefabs[8], new Vector3(-15f, 1f, -15f), Quaternion.Euler(0, 0, 0));
            NetworkServer.Spawn(obj);
            rpcSendId(obj.GetComponent<NetworkIdentity>().assetId);
        }
    }
    [ClientRpc(includeOwner = true)]
    void rpcSendId(Guid assetId)
    {
        GameObject []list = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach( GameObject i in list)
        {
            try
            {
                if (i.GetComponent<NetworkIdentity>().assetId == assetId)
                {
                    obj = i;
                    return;
                }
            }
            catch { }
        }
    }
}
