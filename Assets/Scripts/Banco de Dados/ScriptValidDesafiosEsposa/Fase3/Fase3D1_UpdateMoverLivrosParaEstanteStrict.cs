using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase3D1_UpdateMoverLivrosParaEstanteStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_CAMA = 301;
    private const int ID_ESTANTE = 304;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "UPDATE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateUpdateMoverLivros_AnyOrderWhere);
    }

    private bool ValidateUpdateMoverLivros_AnyOrderWhere(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

       
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

     
        var m = Regex.Match(
            cleaned,
            @"^\s*update\s+itens\s+set\s+idmovel\s*=\s*(?:304|'304'|""304"")\s+" +
            @"where\s+(.*?)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string whereBody = NormalizeSpaces(m.Groups[1].Value);

     
        var conds = Regex.Split(whereBody, @"\s+and\s+", RegexOptions.IgnoreCase)
                         .Select(NormalizeSpaces)
                         .Where(s => s.Length > 0)
                         .ToList();

    
        if (conds.Count != 2) return false;

        bool hasIdMovel = false;
        bool hasLivroLike = false;

        foreach (var c in conds)
        {
            if (Regex.IsMatch(c, @"^idmovel\s*=\s*(?:301|'301'|""301"")$", RegexOptions.IgnoreCase))
            {
                hasIdMovel = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^nomeitem\s+like\s*(?:'Livro%'|""Livro%"")$", RegexOptions.IgnoreCase))
            {
                hasLivroLike = true;
                continue;
            }

       
            return false;
        }

        return hasIdMovel && hasLivroLike;
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
