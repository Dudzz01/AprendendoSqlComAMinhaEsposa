using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase4D1_DeleteItensLixoStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "DELETE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateDeleteLixo_AnyOrderWhere);
    }

    private bool ValidateDeleteLixo_AnyOrderWhere(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

     
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

     
        var m = Regex.Match(
            cleaned,
            @"^\s*delete\s+from\s+itens\s+where\s+(.*?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string whereBody = NormalizeSpaces(m.Groups[1].Value);

     
        var conds = Regex.Split(whereBody, @"\s+and\s+", RegexOptions.IgnoreCase)
                         .Select(NormalizeSpaces)
                         .Where(s => s.Length > 0)
                         .ToList();

     
        if (conds.Count != 1) return false;

      
        return Regex.IsMatch(
            conds[0],
            @"^nomeitem\s*=\s*(?:'Lixo'|""Lixo"")$",
            RegexOptions.IgnoreCase
        );
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