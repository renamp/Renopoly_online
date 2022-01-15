using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

static public class PlayerDataExtensions
{
    static public void transfer(this ICollection<PlayerData> jogadores, int idSource, int idTarget, float value)
    {
        if (idSource >= 0)
            jogadores.ElementAt(idSource).account -= value;
        if(idTarget >=0)
            jogadores.ElementAt(idTarget).account += value;
    }

    static public void transferToAll(this ICollection<PlayerData> jogadores, int idSource, float value)
    {
        for(int i=0; i<jogadores.Count(); i++)
            if (i != idSource)
            {
                jogadores.ElementAt(idSource).account -= value;
                jogadores.ElementAt(i).account += value;
            }
    }
    static public void transferFromAll(this ICollection<PlayerData> jogadores, int idTarget, float value)
    {
        for (int i = 0; i < jogadores.Count(); i++)
            if (i != idTarget)
            {
                jogadores.ElementAt(i).account -= value;
                jogadores.ElementAt(idTarget).account += value;
            }
    }
}


public class PlayerName : NetworkBehaviour
{
    public string playerName;
    public float account;
}

public class PlayerData : PlayerName
{
    public int id;
    
    public List<Titulo> titulos = new List<Titulo>();
    public GameObject playerObj;
    public PathQueue pathQueue;
    
    public Pose playerPos;
    public int position;

    public int cadeia;
    public bool connected;
    public bool ready;
    public int dia;
    public int mes;
    public int ano;

    public PlayerData(int id, string playerName, float valorInicial)
    {
        createPlayer(id, playerName, valorInicial);
    }
    public void createPlayer(int id, string playerName, float valorInicial)
    {
        this.id = id;
        this.position = 0;
        this.playerName = playerName;
        this.account = valorInicial;
        titulos = new List<Titulo>();
    }
    public void updatePositionFromObj()
    {
        if( playerObj != null)
            updatePositionFromObj(playerObj.transform.position, playerObj.transform.rotation);
    }
    public void updatePositionFromObj(Vector3 objPosition, Quaternion objRotation)
    {
        playerPos = new Pose(objPosition, objRotation);
    }
    public void copyPlayer(PlayerData cpy)
    {
        id          = cpy.id;
        playerName  = cpy.playerName;
        account     = cpy.account;
        titulos     = cpy.titulos;
        playerObj   = cpy.playerObj;
        playerPos   = cpy.playerPos;
        position    = cpy.position;
        connected   = cpy.connected;
        ready       = cpy.ready;
        dia         = cpy.dia;
        mes         = cpy.mes;
        ano         = cpy.ano;
        cadeia      = cpy.cadeia;
}

    public void toogleReady()
    {
        ready = !ready;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPos == null && playerObj != null)
            updatePositionFromObj();
    }
    
}
