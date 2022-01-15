using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum languageText
{
    Language, Conectar, Host, Server, Pronto, Comecar, Aguardando, NomeJogador,
    lancarDados, Negociar, passarVez, Comprar, receber13Salario, receberBonus, pagarFianca,
    pagarImposto, Pagar, receberRestituicao, solicitarPagamento, pagamentoSolicitado, Upgrade, Downgrade, vender,
    enviarContrato, aceitarContrato, canelarContrato, Contrato, selecionePropriedade, Contraparte,
    Comprou, Pagou, fezUpgrade, Recebeu, Fianca, Vendeu,
    Reves, RevesPague, pagueCadaJogador,
    reves0, reves1, reves2, reves3, reves4, reves5, reves6, reves7, reves8, reves9, reves10,
    reves11, //reves12,
    Sorte, SorteReceba, recebaDeCadaJogador,
    sorte0, sorte1, sorte2, sorte3, sorte4, sorte5, sorte6, sorte7, sorte8, sorte9, //sorte10,
    Acao,
    acao0, acao1,
    SorteRevesEnd,
}

public class Language : MonoBehaviour
{
    static string[,] languages = {
        { "Portugues", "Conectar", "Host","Server", "Pronto", "Começar", "Aguardando..", "Nome:",
            "Lancar Dados", "Negociar", "Passar a Vez", "Comprar", "Receber 13 Salário", "Receber Bonus", "Pagar Fiança",
            "Pagar Imposto", "Pagar", "Receber Restituição", "Enviar Pix","Pix Enviado!", "Upgrade", "Downgrade", "Vender",
            "Enviar\nContrato", "Aceitar\nContrato", "Cancelar\nContrato", "Contrato", "Selecione Propriedade", "Contraparte",
            "comprou", "pagou", "fez upgrade", "recebeu", "fiança", "vendeu",
            "Revés", "Pague", "Pague a cada jogador",
            "Você estacionou em lugar proibido","Você ultrapassou sinal vermelho", "Pague a cada jogador valor dados x $90",
            "", "", "",
            "", "", "", 
            "", 
            "Vá para a cadeia por 3 rodadas!" , "Você fez um péssimo investimento na bolsa e perdeu 20% do valor em sua conta",
            "Sorte", "Receba", "Receba de cada jogador",
            "Receba de cada jogador valor dos dados x $120", "", "",
            "", "", "",
            "", "", "", 
            "", 
            "Ação",
            "Você é um piloto de aviâo e um passageiro está causando confusão.\nMande um jogador para a prisão por 3 rodadas.", "Troque de lugar com um jogador",
            "",
        },
        { "English", "Connect", "Host","Server", "Ready", "Start", "Waiting..", "Name:",
            "Roll Dices", "Deal", "Next Turn", "Buy", "Get Salary", "Get Bonus", "Pay Bail",
            "Pay Taxes", "Pay", "Receive Refunds", "Request Payment","Payment Requested!", "Upgrade", "Downgrade", "Sell",
            "Send\nContract", "Accept\nContract", "Cancel\nContract", "Contract", "Select Property", "Counterparty",
            "bought", "paid", "upgraded", "received", "bail", "sold",
            "Setback", "Pay", "pay each player",
            "You parked in a prohibited place", "You ran the red light", "Pay each player dice value x $80",
            "", "", "",
            "", "", "", 
            "", 
            "Go to jail for 3 turns", "You made a bad investment in the stock market and lost 20% of the amount in your account",
            "Luck", "Receive", "get from each player",
            "Receive dice value x $120 from each player", "", "",
            "", "", "",
            "", "", "", 
            "", 
            "Action",
            "You are an airplane pilot and a passenger is causing trouble.\nSend a player to prison for 3 rounds.", "switch places with a player", 
            "",
        },
    };

    static public int languageSel; // language selected

    static public string text(languageText lang)
    {
        return languages[languageSel,(int)lang];
    }
    static public string text(int index)
    {
        return languages[languageSel, index];
    }
    static public string textReves(int index)
    {
        return languages[languageSel, ((int)languageText.reves0) + index];
    }
    static public string textSorte(int index)
    {
        return languages[languageSel, ((int)languageText.sorte0) + index];
    }
    static public string textSorteReves(int index)
    {
        try
        {
            if (index < 200 && index < RevesTexCount())
                return languages[languageSel, ((int)languageText.reves0) + index];
            else if( index < 400 && (index - 200) < SorteTexCount())
                return languages[languageSel, ((int)languageText.sorte0) + index - 200];
            else if(index < 600 && (index - 400) < ActionTexCount())
                return languages[languageSel, ((int)languageText.acao0) + index - 400];
        }
        catch{}
        return "Error:"+index;
    }
    
    static public string[] getLanguages()
    {
        List<string> lang = new List<string>();
        for(int i=0; i<languages.GetLength(0); i++)
            lang.Add(languages[i,0]);
        return lang.ToArray();
    }
    static public int RevesTexCount()
    {
        return (int)languageText.Sorte - (int)languageText.reves0;
    }
    static public int SorteTexCount()
    {
        return (int)languageText.Acao - (int)languageText.sorte0;
    }
    static public int ActionTexCount()
    {
        return (int)languageText.SorteRevesEnd - (int)languageText.acao0;
    }
}
