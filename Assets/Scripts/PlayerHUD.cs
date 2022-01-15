using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Drawing;
using System.Linq;
using UnityEngine.UI;
//using UnityEditor;
//using UnityEngine.PlayerLoop;
//using System.Threading;


public struct structContratoTitulos
{
    public int position, level;
    public float price;
}

public class PlayerHUD : NetworkBehaviour
{
    public Tabuleiro tabuleiro;
    [SyncVar]
    public int playerId, cadeia;

    private Transform cameraTransform;
    private Transform tabuleiroTransform;
    private float rotationSpeed;

    private bool coroutineSlowupdate;
    private bool isCameraInitialPosition;
    private bool rotateCamera;
    private Titulo tituloInfo;

    private List<structContratoTitulos> contratanteTitulos, contraparteTitulos;
    private List<GameObject> listTitulosPrice = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            cameraTransform = GameObject.FindWithTag("MainCamera").transform;
            tabuleiroTransform = GameObject.Find("Tabuleiro").transform;
            rotationSpeed = 20f;

            tabuleiro = GameObject.Find("Server").GetComponent<Tabuleiro>();
            tabuleiro.setPlayerHud(this);

            if (listTitulosPrice.Count == 0 && GameObject.Find("TextTituloPrice(" + 0 + ")") == null)
            {
                for (int i = 0; i < tabuleiro.titulos.Count; i++)
                {
                    Pose pos = PlayerMove.getPosition(0, tabuleiro.titulos[i].position);
                    listTitulosPrice.Add(Instantiate(tabuleiro.nm.spawnPrefabs[7], pos.position, pos.rotation));
                    listTitulosPrice[i].name = "TextTituloPrice(" + i + ")";
                    listTitulosPrice[i].transform.Translate(new Vector3(-1.16f, 0, 0));
                    updateTitulosPrice(i, tabuleiro.titulos[i].type, tabuleiro.titulos[i].Dono, tabuleiro.titulos[i].price);
                }
            }

            Menus.contratoEnviar.GetComponent<Button>().onClick.AddListener(handerContratoEnviar);
            Menus.contratoCancelar.GetComponent<Button>().onClick.AddListener(handerContratoCancelar);

            cmdUpdateTitulosPrice();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if( isLocalPlayer )
        {
            if( !coroutineSlowupdate)
                StartCoroutine("slowUpdate");

            Menus.handlerButton_Pg3_Ready(cmdToogleReady);  // handler for the button ready

            if (tabuleiro.emProgresso)
            {
                Menus.disableFirstScreenPanel();                // disable connection screen
                Menus.countDownVisibility(tabuleiro.clock);

                if (!isCameraInitialPosition)
                {
                    isCameraInitialPosition = true;
                    StartCoroutine("cameraInitialPosition");
                }

                // Menu for Titulo
                if (Menus.titulosInfoCanvas.activeSelf)
                {
                    // Upgrade button
                    if (!tabuleiro.tituloInfoUpgrade && tituloInfo.upgradable && tituloInfo.Dono == playerId && tabuleiro.vezJogador == playerId && cadeia==0)
                    {
                        Menus.titulosInfoUpgrade.SetActive(true);
                        Menus.titulosInfoUpgrade.GetComponentInChildren<Text>().text = "$" + tituloInfo.getUpgradePrice() + "\n" + Language.text(languageText.Upgrade);
                        Menus.titulosInfoUpgrade.GetComponent<Button>().onClick.RemoveAllListeners();
                        Menus.titulosInfoUpgrade.GetComponent<Button>().onClick.AddListener(clienteTituloInfoUpgrade);
                    }
                    else
                        Menus.titulosInfoUpgrade.SetActive(false);

                    // Downgrade button
                    if(tituloInfo.Dono == playerId)
                    {
                        Menus.titulosInfoDowngrade.SetActive(true);
                        Menus.titulosInfoDowngrade.GetComponent<Button>().onClick.RemoveAllListeners();
                        Menus.titulosInfoDowngrade.GetComponent<Button>().onClick.AddListener(clientTituloInfoDowngrade);
                        if ( tituloInfo.level == 0 )
                            Menus.titulosInfoDowngrade.GetComponentInChildren<Text>().text = "$" + tituloInfo.getDowngradePrice() + "\n" + Language.text(languageText.vender);
                        else
                            Menus.titulosInfoDowngrade.GetComponentInChildren<Text>().text = "$" + tituloInfo.getDowngradePrice() + "\n" + Language.text(languageText.Downgrade);
                    }
                    else
                        Menus.titulosInfoDowngrade.SetActive(false);
                }

                // Menu Contrato disable
                if (!tabuleiro.contratoPendente  && Menus.contratoCanvas.activeSelf) // && !tabuleiro.PaymentRequest
                {
                    if ( (tabuleiro.vezJogador==playerId && tabuleiro.state!=(int)TabState.Menu1Ready) || 
                        (tabuleiro.vezJogador != playerId && playerId != tabuleiro.donoTitulo))
                        Menus.contratoCanvas.SetActive(false);
                }
                if (tabuleiro.contratoPendente)
                {
                    string[] contrato = tabuleiro.contratoData.Split(';');
                    Menus.contratoContraparteShow(playerId, tabuleiro.playersNames, contrato, tabuleiro.titulos);
                }

                if (tabuleiro.vezJogador == playerId)
                {
                    
                    // Jogada inicial do jogador
                    if (tabuleiro.state == (int)TabState.setMenu)               // first player option menu
                    {
                        cmdSetState((int)TabState.Menu1Ready);
                    }
                    else if (tabuleiro.state == (int)TabState.Menu1Ready)
                    {
                        //Menus.handlerPlayerVezMenu1(cmdLancarDados);
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset

                        Menus.addButtonHandler(buttonIndex++, cmdLancarDados, Language.text(languageText.lancarDados));

                        if (!tabuleiro.contratoPendente)
                            Menus.addButtonHandler(buttonIndex++, contratoFuncShow, Language.text(languageText.Negociar));

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.lancarDados || tabuleiro.state == (int)TabState.verificaDados)
                    {
                        Menus.CleanPlayerMenu(0);        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.PlayerBuyOption)
                    {
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset

                        // if double dices
                        if (tabuleiro.dice1Value == tabuleiro.dice2Value)
                            Menus.addButtonHandler(buttonIndex++, cmdLancarDados, Language.text(languageText.lancarDados));
                        else
                            Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));

                        string text = "$" + tabuleiro.auxiliarPrice1 + "\n" + Language.text(languageText.Comprar);
                        Menus.addButtonHandler(buttonIndex++, cmdComprarTitulo, text);

                        if (tabuleiro.getAnualMoney)            // receber money every year
                            Menus.addButtonHandler(buttonIndex++, cmdReceberAnualMoney, Language.text(languageText.receber13Salario));

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.waitToPass)
                    {
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset

                        if (tabuleiro.getBonusMoney)
                        {            // receber money every year
                            if (tabuleiro.auxiliarPrice1 > 0)
                            {
                                Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));
                                Menus.addButtonHandler(3, cmdReceberBonusMoney, Language.text(languageText.receberBonus));
                            }
                            else
                            {
                                string txt = Language.text(languageText.pagarImposto) + "\n$" + tabuleiro.auxiliarPrice1;
                                Menus.addButtonHandler(3, cmdReceberBonusMoney, txt);
                            }
                        }
                        else
                            Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));

                        if (tabuleiro.getAnualMoney)            // receber money every year
                            Menus.addButtonHandler(3, cmdReceberAnualMoney, Language.text(languageText.receber13Salario));

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.waitForBill)
                    {
                        Menus.CleanPlayerMenuReset();        // clean buttons reset
                        int buttonIndex = 0;
                        if (tabuleiro.clock == 0f || tabuleiro.PaymentRequest)
                        {
                            if (!tabuleiro.PaymentRequest)
                            {
                                // if double dices
                                if (tabuleiro.dice1Value == tabuleiro.dice2Value)
                                    Menus.addButtonHandler(buttonIndex++, cmdLancarDados, Language.text(languageText.lancarDados));
                                else
                                    Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));
                            }
                            else
                            {
                                string text = "$" + tabuleiro.auxiliarPrice1 + "\n" + Language.text(languageText.Pagar);
                                Menus.addButtonHandler(buttonIndex++, cmdPagarRent, text);
                            }
                        }
                        if (tabuleiro.getAnualMoney)            // receber money every year
                            Menus.addButtonHandler(3, cmdReceberAnualMoney, Language.text(languageText.receber13Salario));

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if ( tabuleiro.state == (int)TabState.waitPlayerAction)  // tabuleiro.state == (int)TabState.PlayerInPosition ||
                    {
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset

                        if ((!tabuleiro.getBonusMoney || tabuleiro.auxiliarPrice1 > 0))
                        {
                            // if double dices
                            if (tabuleiro.dice1Value == tabuleiro.dice2Value)
                                Menus.addButtonHandler(buttonIndex++, cmdLancarDados, Language.text(languageText.lancarDados));
                            else
                                Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));
                        }

                        if (tabuleiro.getAnualMoney)            // receber money every year
                            Menus.addButtonHandler(3, cmdReceberAnualMoney, Language.text(languageText.receber13Salario));

                        if (tabuleiro.getBonusMoney)
                        {
                            string txt;
                            if (tabuleiro.auxiliarPrice1 > 0)
                                txt = Language.text(languageText.receberRestituicao) + "\n$" + tabuleiro.auxiliarPrice1;
                            else
                                txt = Language.text(languageText.pagarImposto) + "\n$" + tabuleiro.auxiliarPrice1;
                                
                            Menus.addButtonHandler(3, cmdReceberBonusMoney, txt);
                        }

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.PlayerUpgrade)
                    {
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset
                        // if double dices
                        if (tabuleiro.dice1Value == tabuleiro.dice2Value )
                            Menus.addButtonHandler(buttonIndex++, cmdLancarDados, Language.text(languageText.lancarDados));
                        else
                            Menus.addButtonHandler(buttonIndex++, cmdPassarVez, Language.text(languageText.passarVez));

                        if (tabuleiro.upgradeAvailable)
                        {
                            string txt = "$" + tabuleiro.upgradePrice + "\n"+ Language.text(languageText.Upgrade) + "\n($" + tabuleiro.auxiliarPrice1 + ")";
                            Menus.addButtonHandler(2, cmdDoUpgrade, txt);
                        }

                        if (tabuleiro.getAnualMoney)            // receber money every year
                            Menus.addButtonHandler(3, cmdReceberAnualMoney, Language.text(languageText.receber13Salario));

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                    else if (tabuleiro.state == (int)TabState.pagarFianca) // se jogador precisa pagar fianca
                    {
                        int buttonIndex = 0;
                        Menus.CleanPlayerMenuReset();        // clean buttons reset
                        string text = Language.text(languageText.pagarFianca) + "\n$" + Tabuleiro.precoFianca;
                        Menus.addButtonHandler(buttonIndex++, cmdPagarFianca, text);

                        Menus.CleanPlayerMenu();        // clean buttons
                    }
                }
                // Not player turn
                else if(playerId>=0 && playerId == tabuleiro.donoTitulo && tabuleiro.state == (int)TabState.waitForBill)
                {
                    int buttonIndex = 0;
                    Menus.CleanPlayerMenuReset();        // clean buttons reset
                    string txt = "";
                    if (!tabuleiro.PaymentRequest)
                        txt = "Enviar Chave Pix\n$" + tabuleiro.auxiliarPrice1;
                    else
                        txt = "Pix Enviado!\n$" + tabuleiro.auxiliarPrice1;

                    Menus.addButtonHandler(buttonIndex++, cmdEnviarPix, txt);

                    if (!tabuleiro.contratoPendente && tabuleiro.PaymentRequest)
                        Menus.addButtonHandler(buttonIndex++, contratoFuncShow, "Negociar");

                    Menus.CleanPlayerMenu();        // clean buttons
                }
                else if(tabuleiro.state != (int)TabState.inSorteReves)
                {
                    Menus.PlayerMenu1.SetActive(false);
                }
            }
            else
            {
                cameraTransform.RotateAround(tabuleiroTransform.position, tabuleiroTransform.up, 5f * Time.deltaTime);
                if(playerId == -1)
                {
                    Menus.Button_Pg3_Ready.SetActive(false);
                }
            }
        }

    }

    public UnityEngine.Color getPlayerColor(int jogador)
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

    void updateTitulosPrice(int index, string type, int dono, float price)
    {
        GameObject objTituloPrice = GameObject.Find("TextTituloPrice(" + index + ")");
        if(type == "Area" || dono == -1)
            objTituloPrice.GetComponentInChildren<TextMesh>().text = "$" + price.ToString("0.");
        else
            objTituloPrice.GetComponentInChildren<TextMesh>().text = "x$" + price.ToString("0.");
        objTituloPrice.GetComponentInChildren<TextMesh>().color = getPlayerColor(dono);
    }

    void updateComponets()
    {
        Menus.SetDisplayPlayersAccounts(tabuleiro.emProgresso);

        int readyCount = 0;
        for (int i = 0; i < 6; i++)
        {
            if (i < tabuleiro.playersNames.Count)
            {
                if ((tabuleiro.playersReady & (1 << i)) > 0)
                {
                    Menus.Text_Pg3_Ready[i].GetComponent<Text>().text = Language.text(languageText.Pronto);
                    readyCount++;
                }
                else
                    Menus.Text_Pg3_Ready[i].GetComponent<Text>().text = Language.text(languageText.Aguardando);

                Menus.textPlayerAccount[i].text = "$" + tabuleiro.playersAccounts[i].ToString("0.");
                Menus.textPlayerName[i].text = tabuleiro.playersNames[i];
                Menus.Text_Pg3_Players[i].SetActive(true);
                Menus.Text_Pg3_Players[i].GetComponent<Text>().text = tabuleiro.playersNames[i];
            }
            else
            {
                Menus.Text_Pg3_Players[i].SetActive(false);
                Menus.textPlayerName[i].text = "";
                Menus.textPlayerAccount[i].text = "";
            }
        }
        if (readyCount == tabuleiro.playersNames.Count && playerId >= 0 )
            Menus.Button_Pg3_Start.SetActive(true);
        else
            Menus.Button_Pg3_Start.SetActive(false);
    }

    void clientUpdateTituloInfo(bool setVisible)
    {
        if( setVisible)
            Menus.titulosInfoCanvas.SetActive(true);
        if (Menus.titulosInfoCanvas.activeSelf)
        {
            GameObject.Find("TituloInfo_titulo").GetComponent<Text>().text = tituloInfo.tituloName;
            GameObject.Find("TituloInfo_id").GetComponent<Text>().text = tituloInfo.position.ToString();
            GameObject.Find("TituloInfo_dono").GetComponent<Text>().text = tituloInfo.getOwnerName(tabuleiro.playersNames.ToList());
            GameObject.Find("TituloInfo_dono").GetComponent<Text>().color = getPlayerColor(tituloInfo.Dono);
            for (int i = 0; i < tituloInfo.custRent.Count; i++)
            {
                string txt = "";
                if (i == tituloInfo.level && tituloInfo.Dono >= 0) txt = "->";
                GameObject.Find("TituloInfo_lvl (" + i + ")").GetComponent<Text>().text = txt + "Level " + i + "  - - - - - - - $" + tituloInfo.custRent[i];
            }
        }
    }


    void contratoFuncReloadContraparte()
    {
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        list.Add(new Dropdown.OptionData(Language.text(languageText.Contraparte)));
        for (int i = 0; i < tabuleiro.playersNames.Count; i++)
            if (playerId != i) list.Add(new Dropdown.OptionData(i+":"+tabuleiro.playersNames[i]));
        Menus.contratoContraparte.GetComponent<Dropdown>().options = list;
        Menus.contratoContraparte.GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
        Menus.contratoContraparte.GetComponent<Dropdown>().onValueChanged.AddListener(handerContraparteChange);
    }
    void contratoFuncShow()
    {
        Menus.contratoCanvas.SetActive(!Menus.contratoCanvas.activeSelf);
        Menus.contratoEnviar.GetComponentInChildren<Text>().text = Language.text(languageText.enviarContrato);
        Menus.contratoEnviar.SetActive(true);
        Menus.contratoCancelar.SetActive(true);
        Menus.handlerContratoContratanteTitulosChange(handlerContratoContratanteTitulosChange);
        Menus.handlerContratoContraparteTitulosChange(handlerContratoContraparteTitulosChange);

        Menus.contratoContratante.GetComponent<Text>().text = tabuleiro.playersNames[playerId];
        Menus.contratoContratante.GetComponent<Text>().color = getPlayerColor(playerId);
        Menus.contratoContratanteEnviar[0].SetActive(true);
        Menus.contratoContratanteEnviar[0].GetComponent<Dropdown>().value = 0;
        Menus.contratoContraparte.GetComponent<Dropdown>().value = 0;
        Menus.ContratoInterable(true);
        cmdRequestPlayerTitulos(playerId);

        int contraparteId = getContraparteId();
        if ( Menus.contratoContraparte.GetComponent<Dropdown>().value == 0)
        {
            contratoFuncReloadContraparte();
            contratoFuncTitulosUpdate(-1, new List<structContratoTitulos>());
        }
        else
        {
            contratoFuncTitulosUpdate(contraparteId, contraparteTitulos);
        }
    }
    void contratoFuncTitulosUpdate(int jogadorid, List<structContratoTitulos> titulos)
    {
        if (jogadorid == playerId)
        {
            contratanteTitulos = titulos;
            List<string> listTitulo = new List<string>();
            foreach (structContratoTitulos i in titulos)
                listTitulo.Add(i.position + ":" + tabuleiro.titulos.getTituloName(i.position));
            Menus.contratoContratanteTitulosShow(listTitulo, true);
        }
        else
        {
            contraparteTitulos = titulos;
            List<string> listTitulo = new List<string>();
            foreach (structContratoTitulos i in titulos)
                listTitulo.Add(i.position + ":" + tabuleiro.titulos.getTituloName(i.position));
            Menus.contratoContraparteTitulosShow(listTitulo, true, false);
        }
    }
    
    void handerContratoCancelar()
    {
        if (tabuleiro.contratoData.Length > 0) { 
            int contratanteId = int.Parse(tabuleiro.contratoData.Substring(0,1));
            Menus.ContratoCancelar((contratanteId != playerId) ? true : false);
            if( contratanteId != playerId)
                Menus.contratoCanvas.SetActive(false);
            cmdContratoCancelar();
        }
        else if (Menus.contratoCanvas.activeSelf)
            Menus.contratoCanvas.SetActive(false);
    }
    void handerContratoEnviar()
    {
        if( !tabuleiro.contratoPendente && getContraparteId()>=0 )
        {
            Menus.contratoEnviar.SetActive(false);
            Menus.ContratoInterable(false);
            cmdContratoEnviar( packContratoData() );
        }
        else if(tabuleiro.contratoPendente)
        {
            Menus.ContratoInterable(true);
            cmdContratoAceitar();
            Menus.contratoCanvas.SetActive(false);
        }
    }
    [Command]
    void cmdContratoAceitar()
    {
        string[] contrato = tabuleiro.contratoData.Split(';');
        PlayerData contratanteData = tabuleiro.jogadores[int.Parse(contrato[0])];
        PlayerData contraparteData = tabuleiro.jogadores[int.Parse(contrato[1])];

        for (int i = 0; i < 4; i++) // 2:5  Titulos From Contratante
        {
            int tituloPosition = int.Parse(contrato[i + 2]);
            if (tituloPosition > 0)
            {
                tabuleiro.titulos.changeDono(tituloPosition, tabuleiro.jogadores, contraparteData.id);
                Titulo t = tabuleiro.titulos.getTitulo(tituloPosition);
                rpcUpdateTitulosPrice(tabuleiro.titulos.getIndex(t.position), t.type, t.Dono, t.getPrice(tabuleiro.jogadores, 1, 0));
            }
        }
        for (int i = 0; i < 4; i++)  // titulos from Contraparte
        {
            int tituloPosition = int.Parse(contrato[i + 6]);
            if (tituloPosition > 0)
            {
                tabuleiro.titulos.changeDono(tituloPosition, tabuleiro.jogadores, contratanteData.id);
                Titulo t = tabuleiro.titulos.getTitulo(tituloPosition);
                rpcUpdateTitulosPrice(tabuleiro.titulos.getIndex(t.position), t.type, t.Dono, t.getPrice(tabuleiro.jogadores, 1, 0));
            }
        }
        contratanteData.account += float.Parse(contrato[11]);
        contraparteData.account -= float.Parse(contrato[11]);

        contraparteData.account += float.Parse(contrato[10]);
        contratanteData.account -= float.Parse(contrato[10]);

        tabuleiro.contratoPendente = false;
        tabuleiro.contratoData = "";

        rpcContratoCancelar(-1);
        if (tabuleiro.PaymentRequest)
        {
            tabuleiro.auxiliarPrice1 = 0;
            if (tabuleiro.dice1Value == tabuleiro.dice2Value)
                tabuleiro.state = (int)TabState.waitPlayerAction;
            else
                tabuleiro.state = (int)TabState.passarVez;
        }

    }
    [Command]
    void cmdContratoEnviar(string contratoData)
    {
        tabuleiro.contratoData = contratoData;
        tabuleiro.contratoPendente = true;
    }
    [Command]
    void cmdContratoCancelar()
    {
        int jogadorId = int.Parse(tabuleiro.contratoData.Substring(0,1));
        tabuleiro.contratoPendente = false;
        tabuleiro.contratoData = "";

        rpcContratoCancelar(jogadorId);
    }

    [ClientRpc]
    void rpcContratoCancelar(int contratanteId)
    {
        //if (contratanteId >= 0)
        Menus.ContratoCancelar((contratanteId != playerId) ? true : false);
        if( contratanteId == -1)
            Menus.contratoCanvas.SetActive(false);
    }

    void handerContraparteChange(int value)
    {
        int contraparteId = getContraparteId();
        if( contraparteId < 0)
            contratoFuncTitulosUpdate(-1, new List<structContratoTitulos>());
        else
            cmdRequestPlayerTitulos(contraparteId);
    }
    void handlerContratoContratanteTitulosChange(int value)
    {
        contratoFuncTitulosUpdate(playerId, contratanteTitulos);
    }
    void handlerContratoContraparteTitulosChange(int value)
    {
        int contraparteId = getContraparteId();
        contratoFuncTitulosUpdate(contraparteId, contraparteTitulos);
    }
    
    int getContraparteId()
    {
        Dropdown drop = Menus.contratoContraparte.GetComponent<Dropdown>();
        try
        {
            return int.Parse(drop.options[drop.value].text.Substring(0, 1));
        }
        catch { return -1; }
    }
    int getTituloPosition(Dropdown drop)
    {
        try
        {
            return int.Parse(drop.options[drop.value].text.Split(':')[0]);
        }
        catch { return 0; }
    }
    int[] getContratanteTitulos()
    {
        int[] list = new int[Menus.contratoContratanteEnviar.Count];
        for (int i = 0; i < Menus.contratoContratanteEnviar.Count; i++)
            list[i] = getTituloPosition(Menus.contratoContratanteEnviar[i].GetComponent<Dropdown>());
        return list;
    }
    int[] getContraparteTitulos()
    {
        int[] list = new int[Menus.contratoContraparteEnviar.Count];
        for (int i = 0; i < Menus.contratoContraparteEnviar.Count; i++)
            list[i] = getTituloPosition(Menus.contratoContraparteEnviar[i].GetComponent<Dropdown>());
        return list;
    }

    /// <summary>
    /// Pack Contrato Data
    /// </summary>
    /// <returns>Contratane, Contraparte, Contratante_titulos ... , Contraparte_titulos ... </returns>
    string packContratoData()
    {
        string txt = playerId + ";" + getContraparteId();
        int[] list = getContratanteTitulos();
        for (int i = 0; i < list.Count(); i++)
            txt += ";" + list[i];
        list = getContraparteTitulos();
        for (int i = 0; i < list.Count(); i++)
            txt += ";" + list[i];
        string tmp = Menus.contratoContratanteMoney.GetComponent<InputField>().text;
        if (tmp == null || tmp == "") tmp = "0";
        txt += ";" + tmp;
        tmp = Menus.contratoContraparteMoney.GetComponent<InputField>().text;
        if (tmp == null || tmp == "") tmp = "0";
        txt += ";" + tmp;

        return txt;
    }

    void clienteTituloInfoUpgrade()
    {
        cmdTituloInfoUpgrade(tituloInfo.position);
    }
    void clientTituloInfoDowngrade()
    {
        cmdDowngradeTitulo(tituloInfo.position);
    }
    
    public IEnumerator cameraInitialPosition()
    {
        Vector3 oldPos = new Vector3( cameraTransform.position.x, cameraTransform.position.y, cameraTransform.position.z);
        Quaternion oldRotation = new Quaternion(cameraTransform.rotation.x, cameraTransform.rotation.y, cameraTransform.rotation.z, cameraTransform.rotation.w);
        Vector3 targetPos = new Vector3(-22.12f, 33.85f, 0f);
        Quaternion targetRotation = Quaternion.Euler(65f, 90f, 0f);

        for (float t = 0f; t <= 1f; t += 0.01f)
        {
            cameraTransform.SetPositionAndRotation( Vector3.Lerp(oldPos, targetPos, t), Quaternion.Lerp(oldRotation, targetRotation, t));
            yield return new WaitForEndOfFrame();
        }
        cameraTransform.SetPositionAndRotation(targetPos, targetRotation);
    }

    public IEnumerator slowUpdate()
    {
        coroutineSlowupdate = true;
        while (coroutineSlowupdate)
        {
            updateComponets();
            yield return new WaitForSeconds(0.2f);
        }
    }

    #region Commands

    [Command]
    void cmdRequestPlayerTitulos(int jogadorId)
    {
        List<structContratoTitulos> titulos = new List<structContratoTitulos>();
        foreach(Titulo i in tabuleiro.jogadores[jogadorId].titulos)
            titulos.Add( new structContratoTitulos { position = i.position, level=i.level, price=i.getPrice(3,3) });
        targetResponsePlayerTitulos(jogadorId, titulos);
    }

    [Command]
    public void cmdTituloInfoUpgrade(int posicao)
    {
        Titulo t = Titulo.getTitulo(tabuleiro.titulos, posicao);
        t.doUpgrade(tabuleiro.jogadores, true);
        tabuleiro.tituloInfoUpgrade = true;
        rpcUpdateTitulosPrice(Titulo.getIndex(tabuleiro.titulos, t.position), t.type, t.Dono, t.getPrice(tabuleiro.jogadores, 1, 0));
        targetResponseTituloInfo(t, true);

        tabuleiro.rpcHistoryAddFezUpgrade(tabuleiro.jogadores[t.Dono].playerName, t.position);
    }
    [Command]
    public void cmdDowngradeTitulo(int posicao)
    {
        Titulo t = Titulo.getTitulo(tabuleiro.titulos, posicao);
        int index = Titulo.getIndex(tabuleiro.titulos, posicao);
        string nomeJogador = tabuleiro.jogadores[t.Dono].playerName;
        t.doDowngrade(tabuleiro.jogadores);
        rpcUpdateTitulosPrice(index, t.type, t.Dono, t.getPrice(tabuleiro.jogadores, 1, 0));
        targetResponseTituloInfo(t, false);

        tabuleiro.rpcHistoryAddDowngrade(nomeJogador, posicao, t.Dono);
    }

    [Command]
    public void cmdPagarFianca()
    {
        tabuleiro.jogadores[tabuleiro.vezJogador].account -= Tabuleiro.precoFianca;
        tabuleiro.serverPrenderJogador(tabuleiro.jogadores[tabuleiro.vezJogador], 0);
        tabuleiro.serverUpdateTitulosPrice();

        tabuleiro.rpcHistoryAddPagouFianca(tabuleiro.jogadores[tabuleiro.vezJogador].playerName, Tabuleiro.precoFianca);
        Tabuleiro.precoFianca *= 1.15f;         // rise 15% each time a player pay for bail
        tabuleiro.state = (int)TabState.passarVez;
    }
    [Command]
    public void cmdToogleReady()
    {
        tabuleiro.jogadores[playerId].toogleReady();
    }
    [Command]
    public void cmdLancarDados()
    {
        tabuleiro.upgradeAvailable = false;
        tabuleiro.upgradePrice = 0f;
        tabuleiro.auxiliarPrice1 = 0f;
        tabuleiro.jogadores[tabuleiro.vezJogador].dia++;
        tabuleiro.state = (int)TabState.lancarDados;
    }
    [Command]
    public void cmdComprarTitulo()
    {
        tabuleiro.serverFuncComprar(tabuleiro.vezJogador, tabuleiro.jogadores[tabuleiro.vezJogador].position);

        if (tabuleiro.dice1Value == tabuleiro.dice2Value)
            tabuleiro.state = (int)TabState.waitPlayerAction;
        else
            tabuleiro.state = (int)TabState.passarVez;

        targetResponseTituloInfo(tabuleiro.titulos.getTitulo(tabuleiro.jogadores[tabuleiro.vezJogador].position), false);
    }
    [Command]
    public void cmdPagarRent()
    {
        tabuleiro.jogadores.transfer(tabuleiro.vezJogador, tabuleiro.donoTitulo, tabuleiro.auxiliarPrice1);
        tabuleiro.rpcHistoryAddPagou(tabuleiro.jogadores[tabuleiro.vezJogador].playerName, tabuleiro.jogadores[tabuleiro.donoTitulo].playerName, tabuleiro.auxiliarPrice1);

        if (tabuleiro.dice1Value == tabuleiro.dice2Value)
            tabuleiro.state = (int)TabState.waitPlayerAction;
        else
            tabuleiro.state = (int)TabState.passarVez;
    }
    [Command]
    public void cmdEnviarPix()
    {
        tabuleiro.PaymentRequest = true;
        tabuleiro.clock = 0f;
    }
    [Command]
    public void cmdReceberAnualMoney()
    {
        float factor = 1f;
        if (tabuleiro.jogadores[tabuleiro.vezJogador].position == 0)
            factor = 1.5f;

        tabuleiro.jogadores[tabuleiro.vezJogador].account += Tabuleiro.anualMoney * factor;
        tabuleiro.getAnualMoney = false;
    }
    [Command]
    public void cmdReceberBonusMoney()
    {
        tabuleiro.jogadores[tabuleiro.vezJogador].account += tabuleiro.auxiliarPrice1;
        tabuleiro.getBonusMoney = false;
    }
    [Command]
    public void cmdDoUpgrade()
    {
        Titulo t = Titulo.getTitulo(tabuleiro.titulos, tabuleiro.jogadores[tabuleiro.vezJogador].position);
        t.doUpgrade(tabuleiro.jogadores, false);
        tabuleiro.upgradeAvailable = false;
        tabuleiro.upgradePrice = 0f;
        tabuleiro.auxiliarPrice1 = 0f;

        rpcUpdateTitulosPrice(Titulo.getIndex(tabuleiro.titulos, t.position), t.type, t.Dono, t.getPrice(tabuleiro.jogadores, 1, 0));
        tabuleiro.rpcHistoryAddFezUpgrade(tabuleiro.jogadores[t.Dono].playerName, t.position);
    }
    [Command]
    public void cmdPassarVez() => tabuleiro.state = (int)TabState.passarVez;
    [Command]
    public void cmdSetRepeatTimes(int repeatTimes) => tabuleiro.repeatTimes = repeatTimes;
    [Command]
    public void cmdSetState(int state) => tabuleiro.state = state;
    [Command]
    public void cmdUpdateTitulosPrice()
    {
        for (int i = 0; i < tabuleiro.titulos.Count; i++)
        {
            targetUpdateTitulosPrice(i, tabuleiro.titulos[i].type, tabuleiro.titulos[i].Dono, tabuleiro.titulos[i].getPrice(tabuleiro.jogadores, 1, 0));
        }
    }
    #endregion

    #region  Rpcs and Targets
    [ClientRpc]
    public void rpcSetRepeatTimes(int repeatTimes)
    {
        tabuleiro.repeatTimes = repeatTimes;
    }
    [ClientRpc]
    public void rpcUpdateTitulosPrice(int index, string type, int dono, float price)
    {
        updateTitulosPrice(index, type, dono, price);
    }
    [TargetRpc]
    public void targetUpdateTitulosPrice(int index, string type, int dono, float price)
    {
        updateTitulosPrice(index, type, dono, price);
    }
    [TargetRpc]
    public void targetResponseTituloInfo(Titulo titulo, bool show)
    {
        if (((!show) && tituloInfo!=null && tituloInfo.position == titulo.position) || show )
            tituloInfo = titulo;
        clientUpdateTituloInfo(show);
    }
    [TargetRpc]
    public void targetResponsePlayerTitulos(int jogadorid, List<structContratoTitulos> titulos)
    {
        contratoFuncTitulosUpdate(jogadorid, titulos);
    }

    #endregion

    private IEnumerator cameraAjust(int direction)
    {
        rotateCamera = true;
        float rotationAmount = 0;
        if (direction == 1)
        {
            rotationAmount = (cameraTransform.rotation.eulerAngles.y + 180f) / 90f;
            if (rotationAmount % 1 > 0.99f)
                rotationAmount = (1-(rotationAmount % 1)) + 90f;
            else if(rotationAmount % 1 < 0.01f)
                rotationAmount = ( (rotationAmount % 1)) + 90f;
            else
                rotationAmount = (1 - (rotationAmount % 1)) * 90f;
            rotationAmount /= 10;
        }
        else if( direction == 2)
        {
            rotationAmount = (cameraTransform.rotation.eulerAngles.y + 180f) / 90f;
            if (rotationAmount % 1 > 0.99f)
                rotationAmount = (-1 + (rotationAmount % 1)) - 90f;
            else if (rotationAmount % 1 < 0.01f)
                rotationAmount = -((rotationAmount % 1)) - 90f;
            else
                rotationAmount = (- (rotationAmount % 1)) * 90f;
            rotationAmount /= 10;
        }

        for (int i = 0; i < 10; i++)
        {
            cameraTransform.RotateAround(tabuleiroTransform.position, tabuleiroTransform.up, rotationAmount);
            yield return new WaitForSeconds(0.05f);
        }
        rotateCamera = false;
    }

    void OnGUI()
    {

        if (isLocalPlayer)
        {

            Size screenSize = new Size(Screen.currentResolution.width, Screen.currentResolution.height);        // get display size

            /////////////////////
            /// Buttons size
            Size buttonRotate = new Size(5 * screenSize.Width / 100, 5 * screenSize.Width / 100);               // set size of button rotate camera

            /////////////////////////////////
            /// Camera rotation
            GUIStyle guiStyle1 = new GUIStyle(GUI.skin.button);
            guiStyle1.fontSize = 50;
            // Rotate camera forward
            if (GUI.Button(new Rect(10, screenSize.Height - buttonRotate.Height - 30 - buttonRotate.Height, buttonRotate.Width, buttonRotate.Height), "<<", guiStyle1) && !rotateCamera)
                StartCoroutine("cameraAjust", 1);
            // Rotate camera forward
            if (GUI.RepeatButton(new Rect(10, screenSize.Height - buttonRotate.Height - 10, buttonRotate.Width, buttonRotate.Height), "<", guiStyle1))
                cameraTransform.RotateAround(tabuleiroTransform.position, tabuleiroTransform.up, rotationSpeed * Time.deltaTime);

            // Rotate camera Reverse
            if (GUI.RepeatButton(new Rect(screenSize.Width - buttonRotate.Width - 10, screenSize.Height - buttonRotate.Height - 10, buttonRotate.Width, buttonRotate.Height), ">", guiStyle1))
                cameraTransform.RotateAround(tabuleiroTransform.position, tabuleiroTransform.up, (-rotationSpeed) * Time.deltaTime);
            // Rotate camera Reverse
            if (GUI.RepeatButton(new Rect(screenSize.Width - buttonRotate.Width - 10, screenSize.Height - buttonRotate.Height - 30 - buttonRotate.Height, buttonRotate.Width, buttonRotate.Height), ">>", guiStyle1) && !rotateCamera)
                StartCoroutine("cameraAjust", 2);

#if DEBUG
            //if (GUI.Button(new Rect(270, 200, 70, 50), "Button 1"))
            //{
            //    cmdTest1();
            //}
            //if (GUI.Button(new Rect(470, 200, 50, 50), "Button 2"))
            //{
            //    cmdTest2();
            //}
#endif
        }
    }

    [Command]
    void cmdTest1()
    {
        //tabuleiro.dice1Value = 1;
        //tabuleiro.dice2Value = 6;
        //tabuleiro.state = (int)TabState.movePlayer;
        tabuleiro.state = (int)TabState.init;
    }
    [Command]
    void cmdTest2()
    {
        tabuleiro.dice1Value = 3;
        tabuleiro.dice2Value = 2;
        tabuleiro.state = (int)TabState.movePlayer;
        //tabuleiro.vezJogador++;
        //if (tabuleiro.vezJogador == tabuleiro.playersNames.Count) tabuleiro.vezJogador = 0;
        //Pose pos = PlayerMove.getPosition(0, ++testobj);
        //objTest.transform.SetPositionAndRotation(pos.position, pos.rotation);
        //objTest.transform.Translate(new Vector3(-1.16f, 0, 0));
        //objTest.GetComponentInChildren<TextMesh>().color = UnityEngine.Color.red;
    }

    
}
