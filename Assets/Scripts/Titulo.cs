using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using TMPro;
using System.Runtime.CompilerServices;

public static class TituloExtensions
{
    static public string getTituloName(this IEnumerable<Titulo> titulos, int position)
    {
        
        foreach (var titulo in titulos)
            if (titulo.position == position) 
                return titulo.tituloName;
        return "";
    }
    static public int getIndex(this IEnumerable<Titulo> titulos, int position)
    {
        for (int i = 0; i < titulos.Count(); i++)
            if (titulos.ElementAt(i).position == position)
                return i;
        return 0;
    }
    static public Titulo getTitulo(this IEnumerable<Titulo> titulos, int position)
    {
        foreach (Titulo i in titulos)
            if (i.position == position)
                return i;

        return null;
    }
    static public void removeFromList(this ICollection<Titulo> titulos, int position)
    {
        titulos.Remove(getTitulo(titulos, position));
    }

    static public void changeDono(this IEnumerable<Titulo> titulos, int position, List<PlayerData> jogadores, int NovoDono)
    {
        Titulo t = getTitulo(titulos, position);
        jogadores[t.Dono].titulos.removeFromList(position);
        jogadores[NovoDono].titulos.Add(t);
        t.Dono = NovoDono;
        // extra rules
        if (t.level > 1) t.level--;             // reduce level 
        t.ano = jogadores[NovoDono].ano + 1;    // set for two turns to upgrade
    }
}

public class Titulo //: NetworkBehaviour
{
    //public PlayerData owner;
    public string tituloName;
    public int position;
    public float price;
    public string type;
    public int level;
    public List<float> custRent;
    public List<float> custUpgrade;

    public int dia, mes, ano;
    public float rent;
    public int Dono;
    public bool upgradable;

    public Titulo(int position, string tituloName, float price, List<float> custRent, List<float> custUpgrade)
    {
        this.position = position;
        this.tituloName = tituloName;
        this.price = price;
        this.custRent = custRent;
        this.custUpgrade = custUpgrade;
        this.level = 0;
        this.Dono = -1;
        //owner = null;
    }
    public Titulo() { 
        this.Dono = -1; 
    }

    public bool canUpgrade()
    {
        if (level < 5) return true;
        return false;
    }

    public bool canUpgrade(List<PlayerData> jogadores)
    {
        if (canUpgrade() && Dono >= 0 && jogadores[Dono].ano > ano && jogadores[Dono].mes > mes)
            return true;
        return false;
    }
    

    public float getPrice(List<PlayerData> jogadores, int dice1, int dice2, float priceFactor)
    {
        if (Dono < 0) rent = price*priceFactor;
        else
        {
            if (jogadores[Dono].cadeia > 0)
                rent = (custRent[level] / 2f) * priceFactor;
            else
                rent = custRent[level] * priceFactor;
            if (type == "company")
                rent *= (dice1 + dice2);
        }
        return rent;
    }
    public float getPrice(List<PlayerData> jogadores, int dice1, int dice2)
    {
        rent = getPrice(jogadores, dice1, dice2, 1f);
        return rent;
    }
    public float getPrice(int dice1, int dice2)
    {
        if (Dono < 0) rent = price ;
        else
        {
            rent = custRent[level];
            if (type == "company")
                rent *= (dice1 + dice2);
        }
        return rent;
    }

    public float getUpgradePrice()
    {
        if (level < 5)
            return custUpgrade[level];
        return 0;
    }

    public float getUpgradeRent()
    {
        if (Dono >= 0)
            return custRent[level + 1];
        return 0;
    }
    public float getDowngradePrice()
    {
        if (level > 0)
            return custUpgrade[level - 1]/2;
        return price/2;
    }
    public void doUpgrade(List<PlayerData> jogadores, bool updateAno)
    {
        jogadores[Dono].account -= getUpgradePrice();
        if (level < 5)
            level++;
        if (updateAno)
        {
            this.ano = jogadores[Dono].ano;
            this.mes = jogadores[Dono].mes;
        }
    }
    public void doDowngrade(List<PlayerData> jogadores)
    {
        jogadores[Dono].account += getDowngradePrice();
        if (level > 0)
            level--;
        else
        {
            int index = getIndex(jogadores[Dono].titulos, position);
            jogadores[Dono].titulos.RemoveAt(index);
            Dono = -1;
        }
    }
    public string getOwnerName(List<string> jogadores)
    {
        if (Dono >= 0)
            return jogadores[Dono];
        return "";
    }

    static public bool hasOwner(List<Titulo> titulos, int position)
    {
        if (titulos == null) return false;

        foreach( Titulo i in titulos)
        {
            if (i.position == position && i.Dono>=0)
                return true;
        }

        return false;
    }

    static public Titulo getTitulo(List<Titulo> titulos, int position)
    {
        foreach (Titulo i in titulos)
            if (i.position == position)
                return i;

        return null;
    }
    static public int getIndex(List<Titulo> titulos, int position)
    {
        for (int i=0; i < titulos.Count; i++)
            if (titulos[i].position == position)
                return i;
        return 0;
    }
    static public string getTituloName(List<Titulo> titulos, int position)
    {
        string result = "";
        Titulo t = getTitulo(titulos, position);
        if (t != null)
            result = t.tituloName;
        return result;
    }
    static public Titulo createTitulo(GameObject obj, string type, int position, string tituloName, float price, float rent1, float rent2, float rent3, float rent4, float rent5, float rent6, float casa, float hotel, float hipoteca)
    {
        //Titulo titulo = obj.AddComponent<Titulo>();
        Titulo titulo = new Titulo();
        titulo.type = type;
        titulo.position = position;
        titulo.tituloName = tituloName;
        titulo.price = price;
        List<float> rent = new List<float>() {rent1, rent2, rent3, rent4, rent5, rent6};
        List<float> lvl = new List<float>() {casa, casa, casa, casa, hotel};
        titulo.custRent = rent;
        titulo.custUpgrade = lvl;
        titulo.Dono = -1;
        return titulo;
    }

    static public List<Titulo> genTitulos(GameObject obj)
    {
        List<Titulo> titulos = new List<Titulo>();
        titulos.Add(createTitulo(obj, "Area", 1, "Leblon", 1000, 60, 300, 900, 2700, 4000, 5000, 500, 500, 500));
        titulos.Add(createTitulo(obj, "Area", 3, "Av Presidente Vargas", 600, 40, 200, 600, 1800, 3200, 4500, 500, 500, 400));
        titulos.Add(createTitulo(obj, "Area", 4, " Av.Nossa S.De Copacabana ", 600, 20, 100, 300, 900, 1600, 2500, 500, 500, 400));
        titulos.Add(createTitulo(obj, "Area", 6, "Av.Brigadeiro Faria Lima", 2400, 200, 1000, 3000, 7500, 9250, 11000, 1500, 1500, 1200));
        titulos.Add(createTitulo(obj, "Area", 8, "Av.Reboucas", 2200, 180, 900, 2500, 7000, 8750, 10500, 1500, 1500, 1100));
        titulos.Add(createTitulo(obj, "Area", 9, "Av. 9 De Julho", 2200, 180, 900, 2500, 7000, 8750, 10500, 1500, 1500, 1100));
        titulos.Add(createTitulo(obj, "Area", 11, "Av.Europa", 2000, 160, 800, 2200, 6000, 8000, 10000, 1000, 1000, 1000));
        titulos.Add(createTitulo(obj, "Area", 13, "Rua Augusta", 1800, 140, 700, 2000, 5500, 7500, 9500, 1000, 1000, 900));
        titulos.Add(createTitulo(obj, "Area", 14, "Av.Pacaembu", 1800, 140, 700, 2000, 5500, 7500, 9500, 1000, 1000, 900));
        titulos.Add(createTitulo(obj, "Area", 17, "Interlagos", 3500, 350, 1750, 5000, 11000, 13000, 15000, 2000, 2000, 1750));
        titulos.Add(createTitulo(obj, "Area", 19, "Morumbi", 4000, 500, 2000, 6000, 14000, 17000, 20000, 2000, 2000, 2000));
        titulos.Add(createTitulo(obj, "Area", 21, "Flamengo", 1200, 80, 400, 1000, 3000, 4500, 6000, 500, 500, 600));
        titulos.Add(createTitulo(obj, "Area", 23, "Botafogo", 1000, 60, 300, 900, 2700, 4000, 5000, 500, 500, 500));
        titulos.Add(createTitulo(obj, "Area", 26, "Av.Brasil", 1600, 120, 600, 1800, 5000, 7000, 9000, 1000, 1000, 800));
        titulos.Add(createTitulo(obj, "Area", 28, "Av.Paulista", 1400, 100, 500, 1500, 4500, 6250, 7500, 1000, 1000, 700));
        titulos.Add(createTitulo(obj, "Area", 29, "Jardim Europa", 1400, 100, 500, 1500, 4500, 6250, 7500, 1000, 1000, 700));
        titulos.Add(createTitulo(obj, "Area", 31, "Copacabana", 2600, 220, 1100, 3300, 8000, 9750, 11500, 1500, 1500, 1300));
        titulos.Add(createTitulo(obj, "Area", 33, "Av.Vieira Souto", 3200, 280, 1500, 4500, 10000, 12000, 14000, 2000, 2000, 1600));
        titulos.Add(createTitulo(obj, "Area", 34, "Av.Atlantica", 3000, 260, 1300, 3900, 9000, 11000, 12750, 2000, 2000, 1500));
        titulos.Add(createTitulo(obj, "Area", 36, "Ipanema", 3000, 260, 1300, 3900, 9000, 11000, 12750, 2000, 2000, 1500));
        titulos.Add(createTitulo(obj, "Area", 38, "Jardim Paulista", 2800, 240, 1200, 3600, 8500, 10350, 12000, 1500, 1500, 1400));
        titulos.Add(createTitulo(obj, "Area", 39, "Brooklin", 2600, 220, 1100, 3300, 8000, 9750, 11500, 1500, 1500, 1300));
        titulos.Add(createTitulo(obj, "company", 5, "Companhia Ferroviaria", 2000, 400, 500, 600, 700, 800, 1150, 1500, 4000, 1000));
        titulos.Add(createTitulo(obj, "company", 7, "Companhia de Viaçao", 2000, 400, 500, 600, 700, 800, 1050, 1500, 4000, 1000));
        titulos.Add(createTitulo(obj, "company", 15, "Companhia de Taxi", 1500, 300, 400, 450, 500, 600, 750, 900, 3000, 750));
        titulos.Add(createTitulo(obj, "company", 25, "Companhia de Navegacao", 1500, 300, 350, 450, 500, 600, 750, 1000, 3000, 750));
        titulos.Add(createTitulo(obj, "company", 32, "Companhia de Aviacao", 2500, 500, 650, 750, 950, 1200, 1500, 2500, 5000, 1000));
        titulos.Add(createTitulo(obj, "company", 35, "Companhia de Taxi aerio", 2000, 450, 600, 700, 800, 1050, 1200, 2000, 3000, 1000));
        return titulos;
    }
}
