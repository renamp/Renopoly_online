using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;
using System.Linq;

public class SorteReves : NetworkBehaviour
{
    static private List<int> listIndex = new List<int>();
    static private int RevesCount = 12, SorteCount = 10 , ActionCount = 2;

    public Tabuleiro tabuleiro;
    public PlayerHUD playerHud;

    // Sync Vars
    public SyncList<string> txtButtonOption = new SyncList<string>() {"","","","","","" };
    [SyncVar]    
    public string textSortReves = "";
    [SyncVar]
    public int page;

    UnityAction[] handlerListButtons;
    List<object> ListButtonOption = new List<object>();

    private void Start()
    {
        handlerListButtons = new UnityAction[]{ handlerButtonSorteReves0, handlerButtonSorteReves1, handlerButtonSorteReves2, handlerButtonSorteReves3, 
                                                handlerButtonSorteReves4, handlerButtonSorteReves5 };
    }

    private void Update()
    {
        if( isServer )
        {
            // Jogada está no sorte or Reves
            if (tabuleiro.state == (int)TabState.inSorteReves)
            {
                if (tabuleiro.clock < 1f)
                    serverButtonSorteReves(0);

                //if ( listIndex[0] < 600)
                //{
                    if (textSortReves.Length == 0)
                    {
                        serverPrepareSorteReves();
                        textSortReves = listIndex[0].ToString();
                    }
                //}
            }
        }


        if (playerHud != null && playerHud.isLocalPlayer && tabuleiro.emProgresso)
        {
            if (tabuleiro.state == (int)TabState.inSorteReves || tabuleiro.state == (int)TabState.passarVez) 
            {
                // Jogada está no sorte or Reves
                if (tabuleiro.vezJogador == playerHud.playerId)
                {
                    //int buttonIndex = 0;
                    Menus.CleanPlayerMenuReset();        // clean buttons

                    if (tabuleiro.getAnualMoney)            // receber money every year
                        Menus.addButtonHandler(3, playerHud.cmdReceberAnualMoney, "Receber 13 Salário");

                    if (textSortReves.Length > 0)
                        Menus.addButtonHandler(5, cmdButtonSorteReves, getText(textSortReves, tabuleiro));

                    Menus.CleanPlayerMenu();        // clean buttons


                    Menus.CleanSorteRevesReset();
                    int buttonIndex = 0;
                    for (int i = 0; i < txtButtonOption.Count; i++)
                        if( txtButtonOption[i].Length > 0)
                            Menus.addSorteRevesHandler(buttonIndex++, handlerListButtons[i], txtButtonOption[i]);
                    Menus.CleanSorteReves();
                }
                else  // only display sorte or reves to other players
                {
                    Menus.CleanPlayerMenuReset();        // clean buttons
                    if (textSortReves.Length > 0)
                        Menus.addButtonHandler(5, doNothing, getText(textSortReves, tabuleiro), false);
                    Menus.CleanPlayerMenu();        // clean buttons

                    Menus.CleanSorteRevesReset();
                    int buttonIndex = 0;
                    for (int i = 0; i < txtButtonOption.Count; i++)
                        if (txtButtonOption[i].Length > 0)
                            Menus.addSorteRevesHandler(buttonIndex++, doNothing, txtButtonOption[i], false);
                    Menus.CleanSorteReves();
                }
            }
        }
    }

    void doNothing() { }
    public void inSorteReves()
    {
        tabuleiro.clock = 50f + 1f;
        tabuleiro.state = (int)TabState.inSorteReves;
    }

    public void handlerButtonSorteReves0() => cmdHandlerButtonSorteReves(0);
    public void handlerButtonSorteReves1() => cmdHandlerButtonSorteReves(1);
    public void handlerButtonSorteReves2() => cmdHandlerButtonSorteReves(2);
    public void handlerButtonSorteReves3() => cmdHandlerButtonSorteReves(3);
    public void handlerButtonSorteReves4() => cmdHandlerButtonSorteReves(4);
    public void handlerButtonSorteReves5() => cmdHandlerButtonSorteReves(5);

    public void serverPrepareSorteReves()
    {
        if (listIndex[0] >= 400 && listIndex[0] < 600)  // action cards
        {
            switch (listIndex[0])
            {
                case 400:
                case 401:       // show list of players
                    for (int j = 0, i = 0; i < tabuleiro.playersNames.Count; i++)
                        if (i != tabuleiro.vezJogador)
                        {
                            txtButtonOption[j++] = tabuleiro.playersNames[i];
                            ListButtonOption.Add(i);
                        }
                    break;
            }
        }
    }
    public void serverButtonSorteReves(int button)
    {
        switch (listIndex[0])
        {
            // Reves special cases
            case 2:
                tabuleiro.jogadores.transferToAll(tabuleiro.vezJogador,  getValue(tabuleiro) );
                break;
            case 10:    // go to jail
                tabuleiro.serverGoToJail(tabuleiro.jogadores[tabuleiro.vezJogador], 3, false);
                break;

            // Sorte special cases
            case 200:
                tabuleiro.jogadores.transferFromAll(tabuleiro.vezJogador, getValue(tabuleiro));
                break;

            // Action cases
            case 400: // send player to jail
                tabuleiro.serverGoToJail(tabuleiro.jogadores[(int)ListButtonOption[button]], 3, false);

                break;
            case 401: // change place with player
                int posYou = tabuleiro.jogadores[tabuleiro.vezJogador].position;
                int posOther = tabuleiro.jogadores[(int)ListButtonOption[button]].position;
                tabuleiro.jogadores[tabuleiro.vezJogador].pathQueue.PathMove(PlayerMove.jumpToPosition(tabuleiro.vezJogador, posOther));
                tabuleiro.jogadores[(int)ListButtonOption[button]].pathQueue.PathMove(PlayerMove.jumpToPosition((int)ListButtonOption[button], posYou));
                tabuleiro.jogadores[tabuleiro.vezJogador].position = posOther;
                tabuleiro.jogadores[(int)ListButtonOption[button]].position = posYou;
                break;

            default:
                if (listIndex[0] < 200)  // reves
                {
                    tabuleiro.jogadores.transfer(tabuleiro.vezJogador, -1, getValue(tabuleiro)); //tabuleiro.jogadores[tabuleiro.vezJogador].account += getValue(tabuleiro);
                    tabuleiro.rpcHistoryAddPagou(tabuleiro.jogadores[tabuleiro.vezJogador].playerName, "", getValue(tabuleiro));
                }
                else if (listIndex[0] < 400) // sorte
                {
                    tabuleiro.jogadores.transfer(-1, tabuleiro.vezJogador, getValue(tabuleiro));
                    tabuleiro.rpcHistoryAddRecebeu(tabuleiro.jogadores[tabuleiro.vezJogador].playerName, "", getValue(tabuleiro));
                }
                break;
        }
        next();
        textSortReves = "";
        ListButtonOption.Clear();
        for (int i = 0; i < txtButtonOption.Count(); i++)
            txtButtonOption[i] = "";

        if (tabuleiro.dice1Value == tabuleiro.dice2Value)
            tabuleiro.state = (int)TabState.waitPlayerAction;
        else
            tabuleiro.state = (int)TabState.passarVez;
    }
    [Command(requiresAuthority =false)]
    public void cmdButtonSorteReves()
    {
        if (listIndex[0] < 400)
        {
            tabuleiro.clock = 0f;
            serverButtonSorteReves(0);
        }
    }
    [Command(requiresAuthority = false)]
    public void cmdHandlerButtonSorteReves(int button)
    {
        tabuleiro.clock = 0f;
        serverButtonSorteReves(button);
    }

    static private int shaffleAux(int value)
    {
        int randValue = UnityEngine.Random.Range(0, listIndex.Count);
        if (randValue == value) 
            randValue = shaffleAux(value);
        return randValue;
    }
    static public void Init(int seed, int times)
    {
        int a, b, c;

        // Add Reves
        for (int i = 0; i < RevesCount; i++)
            listIndex.Add(i);
        // Add Sorte
        for (int i = 0; i < SorteCount; i++)
            listIndex.Add(i + 200);
        // Add Action
        for (int i = 0; i < ActionCount; i++)
            listIndex.Add(i + 400);

        if (listIndex.Count == 0) return;

        UnityEngine.Random.InitState(seed);
        for(int j=0; j < 100; j++) 
            for (int i=0; i<times; i++)
            {
                a = shaffleAux(-1);
                b = shaffleAux(a);
                c = listIndex[a];
                listIndex[a] = listIndex[b];
                listIndex[b] = c;
            }

        // Debug for reves
#if DEBUG
        listIndex.Insert(0, 10);
        listIndex.Insert(0, 10);
#endif
    }
    static public string getText(int indexList, Tabuleiro tabuleiro)
    {
        float value = getValue(indexList, tabuleiro);
        string txt="";

        if (indexList < 200) txt = Language.text(languageText.Reves) + "\n\n";
        else if (indexList < 400) txt = Language.text(languageText.Sorte) + "\n\n";
        else if (indexList < 600) txt = Language.text(languageText.Acao) + "\n\n";

        switch (indexList)
        {
            // Reves special cases
            case 2:
                return txt += Language.textSorteReves(indexList) + "\n" + Language.text(languageText.pagueCadaJogador) + ": $" + value;
            case 10:    // go to jail
                return txt += Language.textSorteReves(indexList) ;

            // Sorte special cases
            case 200:
                return txt += Language.textSorteReves(indexList) + "\n" + Language.text(languageText.recebaDeCadaJogador) + ": $" + value;

            default:
                if(indexList < Language.RevesTexCount())
                    return txt += Language.textSorteReves(indexList) + "\n" + Language.text(languageText.RevesPague) + ": $" + value;
                else if(indexList - 200 < Language.SorteTexCount())
                    return txt += Language.textSorteReves(indexList) + "\n" + Language.text(languageText.SorteReceba) + ": $" + value;
                break;
        }

        if (indexList < 200) txt += Language.text(languageText.RevesPague) + ": $" + value;             // reves
        else if (indexList < 400) txt += Language.text(languageText.SorteReceba) + ": $" + value;       // sorte
        else if( indexList < 600) txt += Language.textSorteReves( indexList ) ;                         // action

        return txt;
    }
    static public string getText(string indexList, Tabuleiro tabuleiro)
    {
        return getText(int.Parse(indexList), tabuleiro);
    }
    static public string getText(Tabuleiro tabuleiro)
    {
        return getText(listIndex[0], tabuleiro);
    }

    static public float getValue(int indexList, Tabuleiro tabuleiro)
    {
        switch (indexList)
        {
            // Reves
            case 0:  
                return 900f;
            case 1: 
                return 600f;
            case 2:
                return 90f * (tabuleiro.dice1Value + tabuleiro.dice2Value);
            case 3:
                return 1200f;
            case 4:
                return 1500f;
            case 5:
                return 2000f;
            case 6:
                return 200f;
            case 7:
                return 250f;
            case 8:
                return 850f;
            case 9:
                return 650f;
            case 10:            // go to jail
                return 0f;
            case 11:            // pay 20% of money in account
                return tabuleiro.jogadores[tabuleiro.vezJogador].account * 0.20f;

            // Sorte
            case 200:
                return 120f * (tabuleiro.dice1Value + tabuleiro.dice2Value);
            case 201:
                return 500f;
            case 202:
                return 750f;
            case 203:
                return 1100f;
            case 204:
                return 1600f;
            case 205:
                return 2000f;
            case 206:
                return 300f;
            case 207:
                return 150f;
            case 208:
                return 800f;
            case 209:
                return 650f;

        }

        return 0;
    }
    static public float getValue(string indexList, Tabuleiro tabuleiro)
    {
        return getValue(int.Parse(indexList), tabuleiro);
    }
    static public float getValue(Tabuleiro tabuleiro)
    {
        return getValue(listIndex[0], tabuleiro);
    }
    
    static public void next()
    {
        int aux;
        aux = listIndex[0];
        listIndex.RemoveAt(0);
        listIndex.Insert(listIndex.Count, aux);
    }
    static public bool isSortReves(int position)
    {
        switch (position)
        {
            case 2:
            case 12:
            case 16:
            case 22:
            case 27:
            case 37:
                return true;
        }
        return false;
    }
}
