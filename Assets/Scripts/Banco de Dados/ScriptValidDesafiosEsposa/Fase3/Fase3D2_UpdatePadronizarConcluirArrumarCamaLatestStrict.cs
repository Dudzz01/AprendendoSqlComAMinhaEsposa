using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase3D2_UpdatePadronizarConcluirArrumarCamaLatestStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_CAMA = 301;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "UPDATE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateUpdateArrumarCamaLatest_AnyOrderSetAndWhere);
    }

    private bool ValidateUpdateArrumarCamaLatest_AnyOrderSetAndWhere(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var m = Regex.Match(
            cleaned,
            @"^\s*update\s+itens\s+set\s+(.*?)\s+" +
            @"where\s+iditem\s*=\s*\(\s*" +
                @"select\s+max\s*\(\s*iditem\s*\)\s+" +
                @"from\s+itens\s+" +
                @"where\s+(.*?)\s*" +
            @"\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string setBody = NormalizeSpaces(m.Groups[1].Value);
        string whereBody = NormalizeSpaces(m.Groups[2].Value);

        
        var setParts = setBody.Split(',')
                              .Select(NormalizeSpaces)
                              .Where(s => s.Length > 0)
                              .ToList();

        if (setParts.Count != 3) return false;

        bool setNomeOk = false;
        bool setStatusOk = false;
        bool setPrioridadeOk = false;

        foreach (var p in setParts)
        {
            if (Regex.IsMatch(p, @"^nomeitem\s*=\s*(?:'Arrumar a cama'|""Arrumar a cama"")$", RegexOptions.IgnoreCase))
            {
                setNomeOk = true;
                continue;
            }

            if (Regex.IsMatch(p, @"^status\s*=\s*(?:'CONCLUIDA'|""CONCLUIDA"")$", RegexOptions.IgnoreCase))
            {
                setStatusOk = true;
                continue;
            }

            if (Regex.IsMatch(p, @"^prioridade\s*=\s*(?:1|'1'|""1"")$", RegexOptions.IgnoreCase))
            {
                setPrioridadeOk = true;
                continue;
            }

         
            return false;
        }

        if (!(setNomeOk && setStatusOk && setPrioridadeOk)) return false;

 
        var conds = Regex.Split(whereBody, @"\s+and\s+", RegexOptions.IgnoreCase)
                         .Select(NormalizeSpaces)
                         .Where(s => s.Length > 0)
                         .ToList();

        if (conds.Count != 4) return false;

        bool hasIdMovel = false;
        bool hasTipo = false;
        bool hasArrumar = false;
        bool hasCama = false;

        foreach (var c in conds)
        {
            if (Regex.IsMatch(c, @"^idmovel\s*=\s*(?:301|'301'|""301"")$", RegexOptions.IgnoreCase))
            {
                hasIdMovel = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^tipo\s*=\s*(?:'TAREFA'|""TAREFA"")$", RegexOptions.IgnoreCase))
            {
                hasTipo = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^lower\s*\(\s*nomeitem\s*\)\s+like\s*(?:'%arrumar%'|""%arrumar%"")$", RegexOptions.IgnoreCase))
            {
                hasArrumar = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^lower\s*\(\s*nomeitem\s*\)\s+like\s*(?:'%cama%'|""%cama%"")$", RegexOptions.IgnoreCase))
            {
                hasCama = true;
                continue;
            }

           
            return false;
        }

        return hasIdMovel && hasTipo && hasArrumar && hasCama;
    }

 
    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }

    private static string NormalizeSpaces(string s)
    {
        s = Regex.Replace(s, @"\s+", " ");
        return s.Trim();
    }
}
