using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum TabState 
{
    init=0, setMenu=1, Menu1Ready=2, lancarDados=10, lancandoDados=11, verificaDados=12, movePlayer=20, waitMovePlayer=21, PlayerInPosition=30, waitPlayerAction = 32,
    movingToJail =40, waitForBill=44, waitToPass=46, inSorteReves=48, 
    PlayerBuyOption=60, PlayerUpgrade=62, pagarFianca=160, passarVez=180,
}

static public class TabuleiroExtensions
{
    
}

public class Tabuleiro : NetworkBehaviour
{
    public List<PlayerData> jogadores = new List<PlayerData>();
    public List<Titulo> titulos = new List<Titulo>();
    public NewNetworkManager nm;
    public Transform diceStart;
    public SorteReves sorteReves;

    static public float valorInicial    = 30000;
    static public float precoFianca     = 750;
    static public float anualMoney      = 2000;
    static public float bonusMoney      = 2000;
    static public float impostoMoney    = 2000;
    static public int minVoltasUpgrade  = 1;

    public int repeatTimes;


    ///////////////////////////////
    /// Sync vars
    /// 
    public SyncList<string> playersNames = new SyncList<string>();
    public SyncList<float> playersAccounts = new SyncList<float>();
    [SyncVar]
    public int vezJogador;      // what player is playing
    [SyncVar]
    public int donoTitulo;      // owner of current titulo +1
    [SyncVar]
    public float clock;
    [SyncVar]
    public bool emProgresso, getAnualMoney, PaymentRequest, getBonusMoney,    // jogo em progresso;
        upgradeAvailable, tituloInfoUpgrade, contratoPendente;
    [SyncVar]
    public int playersReady;    // each player set one bit when is ready.
    [SyncVar]
    public int state;           // game state
    [SyncVar]
    public int dice1Value, dice2Value;
    [SyncVar]
    public float auxiliarPrice1, upgradePrice;
    [SyncVar]
    public string contratoData;
    


    /////////////////////////////////
    /// Private variable
    private bool clientUpdateStarted;
    private bool serverUpdateStarted;
    private GameObject Dice1;
    private GameObject Dice2;
    private Quaternion diceRotation1, diceRotation2;
    private Vector3 diceForce1, diceTorque1, diceForce2, diceTorque2;
    private int timerShowState;
    private PlayerHUD playerHudLocal;

    // Start is called before the first frame update
    void Start()
    {
        Menus.addButtonStartHandler(cmdStartButton);

        // Load Positions of the board
        for( int i=0; i<4; i++)
            PlayerMove.startPosition(i, GameObject.Find("StartPoint ("+i+")"));

        sorteReves = GameObject.Find("Server").GetComponent<SorteReves>();
    }
    public override void OnStopClient()
    {
        StopAllCoroutines();
        clientUpdateStarted = false;
        Menus.enableFirstScreenPanel();
    }

    public void setPlayerHud(PlayerHUD playerHud)
    {
        playerHudLocal = playerHud;
        sorteReves.playerHud = playerHud;
    }

    // Update is called once per frame
    void Update()
    {
        //////////////////////////
        /// Running in the server
        if (isServer && !serverUpdateStarted)
        {
            StartCoroutine("serverUpdate");
            serverUpdateStarted = true;
        }


        ///////////////////////////
        /// Running in the client
        if (isClient )
        {

            if (!clientUpdateStarted)
            {
                StartCoroutine("clientUpdatePlayerName");
                clientUpdateStarted = true;
            }
        }


    }


    #region Parallel functions

    IEnumerator clientUpdatePlayerName()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
        }
    }
    IEnumerator serverUpdate()
    {
        while (true)
        {
            serverCheckPlayersReady();

            if (clock > 1f) clock -= 0.2f;

            if(emProgresso)
            {

                if (state == (int)TabState.init)
                    serverFuncInit();
                else if (state == (int)TabState.lancarDados || state == (int)TabState.lancandoDados) // lancar dados
                    serverFuncLancarDados();
                else if (state == (int)TabState.verificaDados)
                    serverFuncVerifiyDados();
                else if (state == (int)TabState.movePlayer)
                    serverFuncMovePlayer();
                else if (state == (int)TabState.waitMovePlayer)
                    serverFuncWaitMoveFinish();
                else if (state == (int)TabState.PlayerInPosition)
                    serverFuncPlayerInPosition();
                else if (state == (int)TabState.movingToJail)
                    serverFuncMovingToJail();
                else if (state == (int)TabState.passarVez)
                    serverFuncPassarVez();
                else if (state == (int)TabState.waitToPass)
                    serverFuncWaitToPlass();
                else if (state == (int)TabState.waitForBill)
                    serverFuncWaitForBill();
                else if (state == (int)TabState.waitForBill)
                    serverFuncWaitPlayerAction();
            }

            if( ++timerShowState > 25)
            {
                timerShowState = 0;
                Debug.Log(state);
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    #endregion

    #region Server Functions


    private void serverCheckPlayersReady()
    {
        int tempReady = 0;
        for (int i = 0; i < jogadores.Count; i++)
        {
            playersAccounts[i] = jogadores[i].account;
            playersNames[i] = jogadores[i].playerName;
            if (jogadores[i].ready)
                tempReady |= 1 << i;

            if (!jogadores[i].connected && emProgresso)
                playersNames[i] = "?" + playersNames[i];
            if (i == vezJogador && emProgresso)
                playersNames[i] = ">" + playersNames[i];
        }
        playersReady = tempReady;
    }
    public void serverFuncComprar(int idJogador, int posTitulo, float priceFactor=1f)
    {
        PlayerData jogador = jogadores[idJogador];
        Titulo t = titulos.getTitulo(posTitulo);
        jogador.account -= t.getPrice(jogadores, dice1Value, dice2Value, priceFactor);
        jogador.titulos.Add(t);
        t.Dono = idJogador;
        t.dia = jogador.dia;
        t.mes = jogador.mes;
        t.ano = jogador.ano;

        rpcHistoryAddComprar(jogadores[idJogador].playerName , t.tituloName);
        rpcUpdateTitulosPrice(titulos.getIndex(posTitulo), t.type, t.Dono, t.getPrice(jogadores, 1, 0));
    }

    public void serverUpdateTitulosPrice()
    {
        for (int i = 0; i < titulos.Count; i++)
            rpcUpdateTitulosPrice(i, titulos[i].type, titulos[i].Dono, titulos[i].getPrice(jogadores, 1, 0));
    }
    private void serverTitulosUpgradable()
    {
        // verifica ano
        int minAnos = jogadores[0].ano;
        for (int i = 1; i < jogadores.Count; i++) minAnos = System.Math.Min(minAnos, jogadores[i].ano);
        if( minAnos > minVoltasUpgrade) // minimo numero de voltas
        {
            for(int i=0; i < titulos.Count; i++)
            {
                if( titulos[i].type == "Area" && titulos[i].Dono>=0)
                    titulos[i].upgradable = titulos[i].canUpgrade(jogadores);
            }
        }
    }
    private void serverFuncInit()
    {
        tituloInfoUpgrade = false;
        serverTitulosUpgradable();

        if (jogadores[vezJogador].account <= 0) {
            if (++vezJogador == jogadores.Count)
                vezJogador = 0;
            serverFuncInit();
        }
        donoTitulo = -1;
        clock = 0f;
        state = (int)TabState.setMenu;
    }
    private void serverFuncLancarDados()
    {
        getAnualMoney = false;
        getBonusMoney = false;

        if (state == (int)TabState.lancarDados )
        {
            diceRotation1 = Random.rotation;
            diceRotation2 = Random.rotation;
            state = (int)TabState.lancandoDados;                            // next state
        }
        else if (state == (int)TabState.lancandoDados)
        {
            rpcFuncLancarDados(1, diceRotation1, diceForce1, diceTorque1);
            serverFuncLancarDados(1, diceRotation1, diceForce1, diceTorque1);
            rpcFuncLancarDados(2, diceRotation2, diceForce2, diceTorque2);
            serverFuncLancarDados(2, diceRotation2, diceForce2, diceTorque2);
            state = (int)TabState.verificaDados;                            // next state
        }
    }
    private void serverFuncVerifiyDados()
    {
        dice1Value = Dice1.GetComponent<DiceScript>().value;
        dice2Value = Dice2.GetComponent<DiceScript>().value;
        if (dice1Value == -1 || dice2Value == -1) state = (int)TabState.lancarDados;
        else if (dice1Value > 0 && dice2Value > 0)
        {
            if (jogadores[vezJogador].cadeia > 0)       // verifica se jogador está preso
            {
                if (dice1Value == dice2Value)           // se dados iguais jogador pode mover
                {
                    serverPrenderJogador(jogadores[vezJogador], 0);
                    serverUpdateTitulosPrice();
                    state = (int)TabState.movePlayer;   // next state
                }
                else
                {
                    if (jogadores[vezJogador].cadeia == 1)      // jogador precisa pagar fianca
                    { 
                        state = (int)TabState.pagarFianca;      // pagar fianca
                        rpcUpdateFianca(precoFianca);
                    }
                    else
                    {                                           // jogador passa a vez
                        jogadores[vezJogador].cadeia--;
                        state = (int)TabState.passarVez;        // passar vez
                    }
                }
            }
            else if (repeatTimes == 2 && dice1Value == dice2Value)      // repeat 3 times double dice go to jail
            {
                serverGoToJail(jogadores[vezJogador]);
                serverPrenderJogador(jogadores[vezJogador], 3);       // number of turns in jail
            }
            else
                state = (int)TabState.movePlayer; // next state
        }
    }
    private void serverFuncMovePlayer()
    {
        PlayerHUD playerhud = jogadores[vezJogador].playerObj.GetComponent<PlayerHUD>();
        for (int i = 0; i < dice1Value + dice2Value; i++)
        {
            jogadores[vezJogador].pathQueue.PathMove(PlayerMove.jumpForward(vezJogador, jogadores[vezJogador].position), 3f);
            jogadores[vezJogador].position++;
            if (jogadores[vezJogador].position == 40)
            {
                jogadores[vezJogador].ano++;
                jogadores[vezJogador].position = 0;
                getAnualMoney = true;
            }
            jogadores[vezJogador].playerPos = PlayerMove.getPosition(vezJogador, jogadores[vezJogador].position);
        }
        state = (int)TabState.waitMovePlayer;
    }
    private void serverFuncWaitMoveFinish()
    {
        if (!jogadores[vezJogador].pathQueue.Moving || !jogadores[vezJogador].connected)
        {
            rpcSetRepeatTimes(++repeatTimes);
            state = (int)TabState.PlayerInPosition;
        }
    }
    private void serverFuncPlayerInPosition()
    {
        // Go to jail
        if (jogadores[vezJogador].position == 30)
        {
            serverGoToJail(jogadores[vezJogador]);
        }
        else if (SorteReves.isSortReves(jogadores[vezJogador].position))
        {
            sorteReves.inSorteReves();
        }
        else if (jogadores[vezJogador].position == 18)  // bonus + salario extra
        {
            getBonusMoney = true;
            auxiliarPrice1 = bonusMoney;
            if (dice1Value != dice2Value)
            {
                clock = 10f + 1f;
                state = (int)TabState.waitToPass;
            }
            else
                state = (int)TabState.waitPlayerAction;
        }
        else if (jogadores[vezJogador].position == 24) //  Imposto
        {
            getBonusMoney = true;
            auxiliarPrice1 = -impostoMoney;
            if (dice1Value != dice2Value)
            {
                clock = 10f + 1f;
                state = (int)TabState.waitToPass;
            }
            else
                state = (int)TabState.waitPlayerAction;
        }
        else
        {
            Titulo titulo = Titulo.getTitulo(titulos, jogadores[vezJogador].position);
            if (titulo != null)
            {
                auxiliarPrice1 = titulo.getPrice(jogadores, dice1Value, dice2Value);
                if (titulo.Dono >= 0)
                {
                    if (titulo.Dono == vezJogador)
                    {
                        upgradePrice = titulo.getUpgradePrice();
                        if (upgradePrice > 0f)
                        {
                            upgradeAvailable = true;
                            auxiliarPrice1 = titulo.getUpgradeRent();
                        }
                        state = (int)TabState.PlayerUpgrade;
                    }
                    else
                    {
                        if (playersAccounts[titulo.Dono] > 0)
                        {
                            clock = 5f + 1f;
                            state = (int)TabState.waitForBill;
                        }
                        else
                        {
                            state = (int)TabState.waitPlayerAction;
                        }
                    }
                    donoTitulo = titulo.Dono;
                }
                else
                {
                    state = (int)TabState.PlayerBuyOption;
                }
            }
            else
            {
                if (dice1Value != dice2Value)
                {
                    clock = 5f + 1f;
                    state = (int)TabState.waitToPass;
                }
                else
                    state = (int)TabState.waitPlayerAction;
            }
        }
    }
    private void serverFuncMovingToJail()
    {
        if (!jogadores[vezJogador].pathQueue.Moving || !jogadores[vezJogador].connected)
        {
            serverUpdateTitulosPrice();
            state = (int)TabState.passarVez;
        }
    }
    private void serverFuncPassarVez()
    {
        jogadores[vezJogador].mes++;
        vezJogador++;
        if (vezJogador == playersNames.Count) vezJogador = 0;
        donoTitulo = -1;
        auxiliarPrice1 = 0;
        repeatTimes = 0;
        PaymentRequest = false;
        getAnualMoney = false;
        getBonusMoney = false;
        clock = 0f;
        state = (int)TabState.init;
    }
    private void serverFuncWaitToPlass()
    {
        if (clock > 0f && clock <= 1f)
        {
            clock = 0f;

            // pay imposto
            if (getBonusMoney && auxiliarPrice1 < 0)
                jogadores[vezJogador].account += auxiliarPrice1 * 1.20f;

            state = (int)TabState.passarVez;
        }
    }
    private void serverFuncWaitForBill()
    {
        if (clock > 0f && clock <= 1f)
            clock = 0f;
    }
    private void serverFuncWaitPlayerAction()
    {
        if(jogadores[vezJogador].account <= 0)
            state = (int)TabState.passarVez;
    }

    private void serverFuncLancarDados(int dice, Quaternion rotation, Vector3 force, Vector3 torque)
    {
        if (dice == 1)
        {
            if (Dice1 == null)
                Dice1 = Instantiate(nm.spawnPrefabs[6], diceStart.position, Random.rotation);

            Dice1.GetComponent<Rigidbody>().Sleep();
            Dice1.transform.position = diceStart.transform.position;
            Dice1.transform.rotation = rotation;
            Dice1.GetComponent<DiceScript>().value = 0;
        }
        else if (dice == 2)
        {
            if (Dice2 == null)
                Dice2 = Instantiate(nm.spawnPrefabs[6], diceStart.position, Random.rotation);

            Dice2.GetComponent<Rigidbody>().Sleep();
            Dice2.transform.position = diceStart.transform.position - new Vector3(3, 0, 0);
            Dice2.transform.rotation = rotation;
            Dice2.GetComponent<DiceScript>().value = 0;
        }
    }
    public void serverGoToJail(PlayerData jogador, int turns, bool movingToJail)
    {
        jogador.pathQueue.PathMove(PlayerMove.jumpToJail(vezJogador));
        jogador.position = 10;    // position of the jail
        jogador.cadeia = turns;
        jogador.playerObj.GetComponent<PlayerHUD>().cadeia = jogador.cadeia;

        if (movingToJail)
            state = (int)TabState.movingToJail;
        else
            serverUpdateTitulosPrice();
    }
    public void serverGoToJail(PlayerData jogador)
    {
        serverGoToJail(jogador, 3, true);
    }
    public void serverPrenderJogador(PlayerData jogador, int dias)
    {
        jogador.cadeia = dias;
        jogador.playerObj.GetComponent<PlayerHUD>().cadeia = jogador.cadeia;
    }

    #endregion

    public void clientRequestTitulo(int posicao)
    {
        cmdRequestTituloInfo(playerHudLocal.playerId, posicao);
    }
    

    // reset flag connected when player disconnect
    public int disconnectPlayer(string playerName)
    {
        int indexPlayer = findPlayer(playerName);
        if (indexPlayer >= 0)
        {
            jogadores[indexPlayer].connected = false;
            return indexPlayer;
        }
        return -1;
    }
    // find player by name
    public int findPlayer(string playerName)
    {
        for(int i=0; i<jogadores.Count; i++)
        {
            if (jogadores[i].playerName == playerName)
            {
                return i;
            }
        }
        return -1;  // jogador nao encotrado
    }

    #region Commands and 

    [Command(requiresAuthority = false)]
    void cmdRequestTituloInfo(int playerId, int posicao)
    {
        jogadores[playerId].playerObj.GetComponent<PlayerHUD>().targetResponseTituloInfo(Titulo.getTitulo(titulos, posicao), true);
    }
    [Command(requiresAuthority = false)]
    void cmdStartButton()
    {
        int seed = (System.DateTime.Now.Millisecond * System.DateTime.Now.Second);
        Random.InitState(seed);
        SorteReves.Init(seed, 60);

        vezJogador = (int)(Random.value / (1f / (float)jogadores.Count));
        for (int i = 0; i < jogadores.Count; i++)
            jogadores[i].account = valorInicial;
        emProgresso = true;
    }
    #endregion

    #region Rpcs

    [ClientRpc]
    public void rpcRegisterTabuleiro(uint objNetId)
    {
        GameObject[] objList = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject i in objList)
        {
            if (i.GetComponent<NetworkIdentity>().netId == objNetId)
                i.GetComponent<PlayerHUD>().tabuleiro = GameObject.Find("Server").GetComponent<Tabuleiro>();
        }
    }
    [ClientRpc]
    public void rpcFuncLancarDados(int dice, Quaternion rotation, Vector3 force, Vector3 torque)
    {
        if (dice == 1)
        {
            if (Dice1 == null)
                Dice1 = Instantiate(nm.spawnPrefabs[6], diceStart.position, Random.rotation);

            Dice1.GetComponent<Rigidbody>().Sleep();
            Dice1.transform.position = diceStart.transform.position;
            Dice1.transform.rotation = rotation;
            //Dice1.GetComponent<Rigidbody>().AddRelativeForce(force);
            //Dice1.GetComponent<Rigidbody>().AddRelativeTorque(torque, ForceMode.Impulse);
        }
        else if (dice == 2)
        {
            if (Dice2 == null)
                Dice2 = Instantiate(nm.spawnPrefabs[6], diceStart.position, Random.rotation);

            Dice2.GetComponent<Rigidbody>().Sleep();
            Dice2.transform.position = diceStart.transform.position - new Vector3(3,0,0);
            Dice2.transform.rotation = rotation;
            //Dice2.GetComponent<Rigidbody>().AddRelativeForce(force);
            //Dice2.GetComponent<Rigidbody>().AddRelativeTorque(torque, ForceMode.Impulse);
        }
    }
    [ClientRpc]
    public void rpcSetRepeatTimes(int repeatTimes)
    {
        this.repeatTimes = repeatTimes;
    }
    [ClientRpc]
    public void rpcUpdateTitulos(List<Titulo> titulos)
    {
        this.titulos = titulos;
    }
    [ClientRpc]
    void rpcUpdateTitulosPrice(int index, string type, int dono, float price)
    {
        GameObject objTituloPrice = GameObject.Find("TextTituloPrice(" + index + ")");
        if (type == "Area" || dono == -1)
            objTituloPrice.GetComponentInChildren<TextMesh>().text = "$" + price;
        else
            objTituloPrice.GetComponentInChildren<TextMesh>().text = "x$" + price;
        switch (dono)
        {
            case 0:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.blue; break;
            case 1:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.white; break;
            case 2:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = new UnityEngine.Color(108f / 255f, 53f / 255f, 11f / 255f); break;
            case 3:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.red; break;
            case 4:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.yellow; break;
            case 5:
                objTituloPrice.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.green; break;
        }
    }
    [ClientRpc]
    void rpcUpdateFianca(float precoFianca)
    {
        Tabuleiro.precoFianca = precoFianca;
    }

    [ClientRpc]
    public void rpcHistoryAddComprar(string playerName, string tituloName)
    {
        string txt = playerName + " " + Language.text(languageText.Comprou) + " " + tituloName;
        Menus.historyAdd(txt);
    }
    [ClientRpc]
    public void rpcHistoryAddPagou(string fromJogador, string toJogador, float value)
    {
        string txt = fromJogador + " " + Language.text(languageText.Pagou) + ": $" + value.ToString("0.");
        if (toJogador != null && toJogador.Length > 0)
            txt += " -> " + toJogador;
        Menus.historyAdd(txt);
    }
    [ClientRpc]
    public void rpcHistoryAddRecebeu(string jogador, string fromJogador, float value)
    {
        string txt = jogador + " " + Language.text(languageText.Recebeu) + ": $" + value.ToString("0.");
        if (fromJogador != null && fromJogador.Length > 0)
            txt += " -> " + fromJogador;
        Menus.historyAdd(txt);
    }
    [ClientRpc]
    public void rpcHistoryAddFezUpgrade(string jogadorNome, int idTitulo)
    {
        string txt = jogadorNome + " " + Language.text(languageText.fezUpgrade) + ": " + titulos.getTituloName(idTitulo);
        Menus.historyAdd(txt);
    }
    [ClientRpc]
    public void rpcHistoryAddPagouFianca(string jogadorNome, float value)
    {
        string txt = jogadorNome + " " + Language.text(languageText.Pagou) + " " + Language.text(languageText.Fianca) + ": $" + value.ToString("0.");
        Menus.historyAdd(txt);
    }
    [ClientRpc]
    public void rpcHistoryAddDowngrade(string jogadorNome, int idTitulo, int idDono)
    {
        string txt = jogadorNome + " ";
        if( idDono >= 0 )
            txt += Language.text(languageText.Downgrade) + ": " + titulos.getTitulo(idTitulo).tituloName;
        else
            txt += Language.text(languageText.Vendeu) + ": " + titulos.getTitulo(idTitulo).tituloName;
        Menus.historyAdd(txt);
    }
    #endregion

    //***********************************************
    // Static functions
    #region Static Functions
    static public void setTransformPosition(Pose source, Transform target)
    {
        target.SetPositionAndRotation(source.position, source.rotation);
    }
    static public void setStartPosition(int playerId, Transform startPoint, Transform target)
    {
        if (playerId < 3)
        {
            target.SetPositionAndRotation(startPoint.position + new Vector3(0f, 0f, 1.4f * ((float)playerId)), startPoint.rotation);
        }else if( playerId < 6)
        {
            target.SetPositionAndRotation(startPoint.position + new Vector3(1.4f, 0f, 1.4f * ((float)playerId-3)), startPoint.rotation);
        }
    }
    static public GameObject getGameObject(int id, GameObject[] objList)
    {
        foreach (GameObject obj in objList)
        {
            string objSearchName = "[connId=" + id.ToString() + "]";
            string objName = obj.name.Substring(obj.name.Length - objSearchName.Length);
            if (objName == objSearchName)
                return obj;
        }
        return null;
    }
    static public uint registerPlayerObj(int id, PlayerData targetPlayer, GameObject[] objList)
    {
        GameObject obj = getGameObject(id, objList);
        if (obj != null)
        {
            targetPlayer.playerObj = obj;
            targetPlayer.updatePositionFromObj();
            targetPlayer.pathQueue = targetPlayer.playerObj.GetComponent<PathQueue>();
            targetPlayer.connected = true;
            return targetPlayer.playerObj.GetComponent<NetworkIdentity>().netId;
        }
        return 0;
    }
    static public void removePlayer(Tabuleiro tab, int playerId)
    {
        if( !tab.emProgresso && playerId>=0 )
        {
            tab.playersNames.RemoveAt(playerId);
            tab.playersAccounts.RemoveAt(playerId);
            Destroy(tab.jogadores[playerId]);
            tab.jogadores.RemoveAt(playerId);
        }
    }
    static public UnityEngine.Color getPlayerColor(int jogador)
    {
        switch (jogador)
        {
            case 0:
                return UnityEngine.Color.blue;
            case 1:
                return UnityEngine.Color.white;
            case 2:
                return new UnityEngine.Color(108f / 255f, 53f / 255f, 11f / 255f);
            case 3:
                return UnityEngine.Color.red;
            case 4:
                return UnityEngine.Color.yellow;
            case 5:
                return UnityEngine.Color.green;
            default:
                return UnityEngine.Color.black;
        }
    }
    #endregion

}
