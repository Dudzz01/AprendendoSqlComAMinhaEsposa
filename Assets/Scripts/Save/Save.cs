using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Save
{
    public bool[] arrayFasesDesbloqueadas = new bool[20];

    public bool gameZerado;

    public int chavesColetadas;

    public int desafiosConcluidos;

    public bool[] quantidadesDesafiosConcluidos = new bool[70];
}
