using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Menus : MonoBehaviour
{
    static public GameObject PlayerMenu1;
    static public GameObject firstScreen;
    static public GameObject firstScreenPanel;
    static public GameObject firstScreenMenuPg1;
    static public GameObject firstScreenMenuPg2;
    static public GameObject firstScreenMenuPg3;
    static public GameObject DisplayPlayersAccounts;
    static public GameObject titulosInfoCanvas, titulosInfoUpgrade, titulosInfoDowngrade;
    static public GameObject contratoCanvas, contratoContraparte, contratoContratante, contratoContratanteMoney, contratoContraparteMoney, contratoEnviar, contratoCancelar;
    static public List<GameObject> contratoContratanteEnviar = new List<GameObject>(), contratoContraparteEnviar = new List<GameObject>();

    // Components
    static public Text[] textPlayerName = new Text[6];
    static public Text[] textPlayerAccount = new Text[6];
    static public GameObject[] Text_Pg3_Players = new GameObject[6];
    static public GameObject[] Text_Pg3_Ready = new GameObject[6];

    static public GameObject serverIp, connectingIp;
    static public GameObject Button_Pg3_Ready;
    static public GameObject Button_Pg3_Start;
    static public GameObject[] ButtonPlayerOption;
    static public GameObject[] ButtonSorteReves;
    static public GameObject ButtonSorteRevesNext;
    static public GameObject ButtonSorteRevesPrevious;
    static public GameObject textCountDown;
    static public GameObject contratoTitle;
    static public GameObject txtHistory;

    static private bool button_Pg3_Ready_listerner;
    static private bool isdisabledfirstScreenPanel;
    static private int buttonClean;
    static private int sorteRevesClean;

    static public void init()
    {
        PlayerMenu1 = GameObject.Find("PlayerMenu1");
        firstScreen = GameObject.Find("ScreenMenu");
        firstScreenMenuPg1 = GameObject.Find("FistScreenPg1");
        firstScreenMenuPg2 = GameObject.Find("FistScreenPg2");
        firstScreenMenuPg3 = GameObject.Find("FistScreenPg3");
        firstScreenPanel = GameObject.Find("FistScreenPanel");
        DisplayPlayersAccounts = GameObject.Find("DisplayPlayersAccounts");
        txtHistory = GameObject.Find("displayHistory");
        // Buttons
        Button_Pg3_Ready = GameObject.Find("Button_Pg3_Ready");
        Button_Pg3_Start = GameObject.Find("Button_Pg3_Start");
        textCountDown = GameObject.Find("TextCountDown");

        ButtonPlayerOption = new GameObject[6];
        for ( int i=0; i< ButtonPlayerOption.Length; i++)
            ButtonPlayerOption[i] = GameObject.Find("ButtonPlayerOption (" + i + ")");

        // Button SorteReves
        ButtonSorteReves = new GameObject[6];
        for (int i = 0; i < ButtonSorteReves.Length; i++)
            ButtonSorteReves[i] = GameObject.Find("ButtonSorteReves (" + i + ")");
        ButtonSorteRevesNext = GameObject.Find("ButtonSorteReves>>");
        ButtonSorteRevesPrevious = GameObject.Find("ButtonSorteReves<<");

        for (int i = 0; i < 6; i++)
        {
            textPlayerName[i] = GameObject.Find("namePlayer" + (i + 1)).GetComponent<Text>();
            textPlayerAccount[i] = GameObject.Find("accountPlayer" + (i + 1)).GetComponent<Text>();
            Text_Pg3_Players[i] = GameObject.Find("Text_Pg3_Player" + (i + 1));
            Text_Pg3_Ready[i] = GameObject.Find("Text_Pg3_Ready" + (i + 1));
        }

        titulosInfoCanvas = GameObject.Find("TituloInfoCanvas");
        titulosInfoUpgrade = GameObject.Find("TituloInfo_upgrade");
        titulosInfoDowngrade = GameObject.Find("TituloInfo_downgrade");

        contratoCanvas = GameObject.Find("ContratoCanvas");
        contratoTitle = GameObject.Find("Contrato_title");
        contratoContraparte = GameObject.Find("Contrato_contraparte");
        contratoContratante = GameObject.Find("Contrato_contratante");
        contratoContratanteMoney = GameObject.Find("Contrato_contratanteMoney");
        contratoContraparteMoney = GameObject.Find("Contrato_contraparteMoney");
        contratoEnviar = GameObject.Find("Contrato_enviar");
        contratoCancelar = GameObject.Find("Contrato_cancelar");
        for (int i=0; i<4; i++)
        {
            contratoContratanteEnviar.Add(GameObject.Find("Contrato_contratanteEnviar (" + i + ")"));
            contratoContraparteEnviar.Add(GameObject.Find("Contrato_contraparteEnviar (" + i + ")"));
        }

        serverIp = GameObject.Find("InputField_serverIp");
        connectingIp = GameObject.Find("Text _ConnetingIp");

        List<Dropdown.OptionData> listIdiomas = new List<Dropdown.OptionData>();
        foreach (string i in Language.getLanguages())
            listIdiomas.Add(new Dropdown.OptionData(i));
        GameObject.Find("Drop_Idioma").GetComponent<Dropdown>().options = listIdiomas;
        GameObject.Find("Drop_Idioma").GetComponent<Dropdown>().onValueChanged.AddListener(changeLanguage);

        checkFile();
        

        // Hide Elements
        hideElements();
    }

    static void checkFile()
    {
        string fileName = Application.persistentDataPath + "/conf1.cfg";
        if (!File.Exists(fileName))
        {
            StreamWriter wr = new StreamWriter(fileName);
            wr.WriteLine("0"); // language
            wr.WriteLine("");  // User
            wr.WriteLine("");  // Ip
            wr.Close();
        }
        else
        {
            StreamReader rd = new StreamReader(fileName);
            GameObject.Find("Drop_Idioma").GetComponent<Dropdown>().value = int.Parse(rd.ReadLine());   // language
            GameObject.Find("InputField_playerName").GetComponent<InputField>().text = rd.ReadLine();   // User
            GameObject.Find("InputField_serverIp").GetComponent<InputField>().text = rd.ReadLine();     // Ip
            rd.Close();
            changeLanguage(GameObject.Find("Drop_Idioma").GetComponent<Dropdown>().value);
        }
    }

    static public void updateFile()
    {
        string fileName = Application.persistentDataPath + "/conf1.cfg";
        if (File.Exists(fileName))
        {
            StreamWriter wr = new StreamWriter(fileName);
            wr.WriteLine(GameObject.Find("Drop_Idioma").GetComponent<Dropdown>().value);                // language
            wr.WriteLine(GameObject.Find("InputField_playerName").GetComponent<InputField>().text);     // User
            wr.WriteLine(GameObject.Find("InputField_serverIp").GetComponent<InputField>().text);       // Ip
            wr.Close();
        }
    }

    static private void changeLanguage(int value)
    {
        Language.languageSel = value;
        GameObject.Find("Button_connectar").GetComponentInChildren<Text>().text = Language.text(languageText.Conectar);
        GameObject.Find("Button_Pg1_Host").GetComponentInChildren<Text>().text = Language.text(languageText.Host);
        GameObject.Find("Button_Pg1_Server").GetComponentInChildren<Text>().text = Language.text(languageText.Server);
        GameObject.Find("Text_playerName").GetComponentInChildren<Text>().text = Language.text(languageText.NomeJogador);
        contratoTitle.GetComponent<Text>().text = Language.text(languageText.Contrato);

        Button_Pg3_Ready.GetComponentInChildren<Text>().text = Language.text(languageText.Pronto);
        Button_Pg3_Start.GetComponentInChildren<Text>().text = Language.text(languageText.Comecar);
        contratoCancelar.GetComponentInChildren<Text>().text = Language.text(languageText.canelarContrato);
    }

    static private void hideElements()
    {
        PlayerMenu1.SetActive(false);
        textCountDown.GetComponent<Text>().text = "";

        for (int i = 0; i < ButtonSorteReves.Length; i++)
            ButtonSorteReves[i].SetActive(false);

        ButtonSorteRevesNext.SetActive(false);
        ButtonSorteRevesPrevious.SetActive(false);

        titulosInfoCanvas.SetActive(false);
        titulosInfoUpgrade.SetActive(false);
        titulosInfoDowngrade.SetActive(false);

        contratoCanvas.SetActive(false);
    }

    static public void countDownVisibility(float clock)
    {
        if (clock > 0)
        {
            string txt = (clock - 1f).ToString();
            if( txt.Length>3) 
                txt = txt.Substring(0,3);
            textCountDown.GetComponent<Text>().text = txt;
        }
        else
            textCountDown.GetComponent<Text>().text = "";
    }

    static public void setFirstScreenMenuPg1()
    {
        firstScreenMenuPg1.SetActive(true);
        firstScreenMenuPg2.SetActive(false);
        firstScreenMenuPg3.SetActive(false);
    }
    static public void setFirstScreenMenuPg3()
    {
        firstScreenMenuPg1.SetActive(false);
        firstScreenMenuPg2.SetActive(false);
        firstScreenMenuPg3.SetActive(true);
    }

    static public void enableFirstScreenPanel()
    {
        if (isdisabledfirstScreenPanel)
        {
            firstScreenPanel.SetActive(true);
            firstScreen.GetComponent<Image>().enabled = true;
            isdisabledfirstScreenPanel = false;
        }
    }
    static public void disableFirstScreenPanel()
    {
        if (!isdisabledfirstScreenPanel)
        {
            firstScreenPanel.SetActive(false);
            firstScreen.GetComponent<Image>().enabled = false;
            isdisabledfirstScreenPanel = true;
        }
    }

    static public void handlerButton_Pg3_Ready(UnityAction func)
    {
        if (!button_Pg3_Ready_listerner)
        {
            Button_Pg3_Ready.GetComponent<Button>().onClick.AddListener(func);
            button_Pg3_Ready_listerner = true;
        }
    }
    static public void addButtonStartHandler(UnityAction func)
    {
        Button_Pg3_Start.GetComponent<Button>().onClick.AddListener(func);
    }
    static public void addButtonHandler(int buttonIndex, UnityAction func, string text)
    {
        addButtonHandler(buttonIndex, func, text, true);
    }
    static public void addButtonHandler(int buttonIndex, UnityAction func, string text, bool interactable)
    {
        ButtonPlayerOption[buttonIndex].SetActive(true);
        ButtonPlayerOption[buttonIndex].GetComponent<Button>().onClick.RemoveAllListeners();
        ButtonPlayerOption[buttonIndex].GetComponent<Button>().onClick.AddListener(func);
        ButtonPlayerOption[buttonIndex].GetComponent<Button>().interactable = interactable;
        ButtonPlayerOption[buttonIndex].GetComponentInChildren<Text>().text = text;
        buttonClean |= (1 << buttonIndex); // for reseting porpuse
    }
    static public void addSorteRevesHandler(int buttonIndex, UnityAction func, string text, bool interactable)
    {
        ButtonSorteReves[buttonIndex].SetActive(true);
        ButtonSorteReves[buttonIndex].GetComponent<Button>().onClick.RemoveAllListeners();
        ButtonSorteReves[buttonIndex].GetComponent<Button>().onClick.AddListener(func);
        ButtonSorteReves[buttonIndex].GetComponent<Button>().interactable = interactable;
        ButtonSorteReves[buttonIndex].GetComponentInChildren<Text>().text = text;
        sorteRevesClean |= (1 << buttonIndex); // for reseting porpuse
    }
    static public void addSorteRevesHandler(int buttonIndex, UnityAction func, string text)
    {
        addSorteRevesHandler(buttonIndex, func, text, true);
    }

    static public int handlerPlayerVezMenu1(UnityAction cmdLancarDados)
    {
        PlayerMenu1.SetActive(true);
        ButtonPlayerOption[0].SetActive(true);
        Button lancarDatos = ButtonPlayerOption[0].GetComponent<Button>();
        lancarDatos.onClick.RemoveAllListeners();
        lancarDatos.onClick.AddListener(cmdLancarDados);
        ButtonPlayerOption[0].GetComponentInChildren<Text>().text = Language.text(languageText.lancarDados);

        for (int i = 1; i < ButtonPlayerOption.Length; i++)                     // disable not used buttons
            ButtonPlayerOption[i].SetActive(false);

        return (int)TabState.Menu1Ready;
    }

    static public void CleanSorteRevesReset()
    {
        PlayerMenu1.SetActive(true);
        sorteRevesClean = 0;
    }
    static public void CleanPlayerMenuReset()
    {
        PlayerMenu1.SetActive(true);
        buttonClean = 0;
    }
    static public void CleanPlayerMenu(int index)
    {
        PlayerMenu1.SetActive(true);
        for (int i = index; i < ButtonPlayerOption.Length; i++)
            ButtonPlayerOption[i].SetActive(false);
    }
    static public void CleanPlayerMenu()
    {
        for (int i = 0; i < ButtonPlayerOption.Length; i++)
            if( (buttonClean & (1<<i)) == 0)
                ButtonPlayerOption[i].SetActive(false);
    }
    static public void CleanSorteReves()
    {
        for (int i = 0; i < ButtonSorteReves.Length; i++)
            if ((sorteRevesClean & (1 << i)) == 0)
                ButtonSorteReves[i].SetActive(false);
    }

    static public void CleanSorteRevesMenu()
    {
        for (int i = 0; i < ButtonSorteReves.Length; i++)
            ButtonSorteReves[i].SetActive(false);
        ButtonSorteRevesNext.SetActive(false);
        ButtonSorteRevesPrevious.SetActive(false);
    }

    static public void SetDisplayPlayersAccounts(bool visibility)
    {
        DisplayPlayersAccounts.SetActive(visibility);
    }

    #region Contrato
    static private void contratoTitulosReload(GameObject obj, List<string> titulosNames) 
    {
        obj.SetActive(true);
        List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
        list.Add(new Dropdown.OptionData(Language.text(languageText.selecionePropriedade)));
        if (titulosNames == null) return;

        for (int j = 0; j < titulosNames.Count; j++)
            list.Add(new Dropdown.OptionData(titulosNames[j]));
        obj.GetComponent<Dropdown>().options = list;
    }
    static public void contratoContratanteTitulosShow(List<string> titulosNames, bool reload)
    {
        for (int i=0; i<contratoContratanteEnviar.Count-1; i++)
            if (contratoContratanteEnviar[i].GetComponent<Dropdown>().value == 0 || !contratoContratanteEnviar[i].activeSelf)
            {
                contratoContratanteEnviar[i + 1].SetActive(false);
                contratoContratanteEnviar[i + 1].GetComponent<Dropdown>().value = 0;
                if (reload && contratoContratanteEnviar[i].activeSelf)
                    contratoTitulosReload(contratoContratanteEnviar[i], titulosNames);
            }
            else
                contratoTitulosReload(contratoContratanteEnviar[i + 1], titulosNames);
    }
    static public void contratoContraparteTitulosShow(List<string> titulosNames, bool reload, bool reset)
    {
        //if (reload && reset)
        //    contratoContraparte.GetComponent<Dropdown>().value = 0;

        if (contratoContraparte.GetComponent<Dropdown>().value > 0)
            contratoContraparteEnviar[0].SetActive(true);
        else
        {
            contratoContraparteEnviar[0].SetActive(false);
            contratoContraparteEnviar[0].GetComponent<Dropdown>().value = 0;
        }

        for (int i=0; i < contratoContraparteEnviar.Count - 1;  i++)
            if (contratoContraparteEnviar[i].GetComponent<Dropdown>().value == 0 || !contratoContraparteEnviar[i].activeSelf)
            {
                contratoContraparteEnviar[i + 1].SetActive(false);
                contratoContraparteEnviar[i + 1].GetComponent<Dropdown>().value = 0;
                if (reload && contratoContraparteEnviar[i].activeSelf)
                    contratoTitulosReload(contratoContraparteEnviar[i], titulosNames);
            }
            else
                contratoTitulosReload(contratoContraparteEnviar[i + 1], titulosNames);
    }

    static public void contratoContraparteShow(int playerId, Mirror.SyncList<string> playersNames, string[] contrato, List<Titulo> titulos)
    {
        if (int.Parse(contrato[0]) != playerId)     // se nao é o contratante
        {
            contratoCanvas.SetActive(true);
            contratoContratante.GetComponent<Text>().text = playersNames[int.Parse(contrato[0])];
            contratoContratante.GetComponent<Text>().color = Tabuleiro.getPlayerColor(int.Parse(contrato[0]));
            contratoContraparte.GetComponent<Dropdown>().options = new List<Dropdown.OptionData>() { new Dropdown.OptionData(playersNames[int.Parse(contrato[1])]) };

            for (int i = 0; i < 4; i++) // 2:5
            {
                if (int.Parse(contrato[i + 2]) == 0)
                    contratoContratanteEnviar[i].SetActive(false);
                else
                {
                    contratoContratanteEnviar[i].SetActive(true);
                    contratoContratanteEnviar[i].GetComponent<Dropdown>().options =
                        new List<Dropdown.OptionData>() { new Dropdown.OptionData(Titulo.getTituloName(titulos, int.Parse(contrato[i + 2]))) };
                }
            }
            for (int i = 0; i < 4; i++)
            {
                if (int.Parse(contrato[i + 6]) == 0)
                    contratoContraparteEnviar[i].SetActive(false);
                else
                {
                    contratoContraparteEnviar[i].SetActive(true);
                    contratoContraparteEnviar[i].GetComponent<Dropdown>().options =
                        new List<Dropdown.OptionData>() { new Dropdown.OptionData(Titulo.getTituloName(titulos, int.Parse(contrato[i + 6]))) };
                }
            }
            contratoContratanteMoney.GetComponent<InputField>().text = contrato[10];
            contratoContraparteMoney.GetComponent<InputField>().text = contrato[11];

            if (int.Parse(contrato[1]) == playerId) // se for a contraparte
            {
                contratoEnviar.SetActive(true);
                contratoEnviar.GetComponentInChildren<Text>().text = Language.text(languageText.aceitarContrato);
            }
            else
            {
                contratoCancelar.SetActive(false);
                contratoEnviar.SetActive(false);
            }
            ContratoInterable(false);
        }
    }

    static public void handlerContratoContratanteTitulosChange(UnityAction<int> func)
    {
        foreach (GameObject i in contratoContratanteEnviar)
        {
            i.GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
            i.GetComponent<Dropdown>().onValueChanged.AddListener(func);
        }
    }
    static public void handlerContratoContraparteTitulosChange(UnityAction<int> func)
    {
        foreach (GameObject i in contratoContraparteEnviar)
        {
            i.GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
            i.GetComponent<Dropdown>().onValueChanged.AddListener(func);
        }
    }
    static public void ContratoInterable( bool interable)
    {
        foreach (GameObject i in contratoContratanteEnviar)
            i.GetComponent<Dropdown>().interactable = interable;
        foreach (GameObject i in contratoContraparteEnviar)
            i.GetComponent<Dropdown>().interactable = interable;
        contratoContraparte.GetComponent<Dropdown>().interactable = interable;

        object[] list = contratoCanvas.GetComponentsInChildren<InputField>();
        foreach (InputField i in list)
            i.interactable = interable;
    }
    static public void ContratoCancelar(bool fullReset)
    {
        contratoContratanteTitulosShow(new List<string>() { Language.text(languageText.selecionePropriedade) }, fullReset);
        contratoContraparteTitulosShow(new List<string>() { Language.text(languageText.selecionePropriedade) }, fullReset, true);

        if (fullReset)
        {
            contratoContraparte.GetComponent<Dropdown>().value = 0;
            //contratoContraparteEnviar[0]
            object[] list = contratoCanvas.GetComponentsInChildren<InputField>();
            foreach (InputField i in list)
                i.text = "";
        }
        contratoEnviar.SetActive(true);

        ContratoInterable(true);
    }

    static public void contratoDropdownReloadEnviar()
    {
        contratoContratanteTitulosShow(null, false);
    }
    #endregion

    #region History
    static public void historyAdd(string txt)
    {
        string history = txtHistory.GetComponent<Text>().text;
        history = ":" + txt + "\n" + history;
        string[] list = history.Split('\n');
        history = "";
        for (int i = 0; i < list.Length && i < 4; i++)
            history += list[i] + "\n";
        txtHistory.GetComponent<Text>().text = history;
    }
    #endregion
}
