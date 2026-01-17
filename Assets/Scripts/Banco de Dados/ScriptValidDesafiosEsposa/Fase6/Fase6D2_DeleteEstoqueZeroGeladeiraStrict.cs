using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase6D2_DeleteEstoqueZeroGeladeiraStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_GELADEIRA = 101;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "DELETE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateDeleteEstoqueZeroGeladeira_AnyOrderWhere);
    }

    private bool ValidateDeleteEstoqueZeroGeladeira_AnyOrderWhere(string sql, int _, DatabaseManager __)
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

        
        if (conds.Count != 3) return false;

        bool hasIdMovel101 = false;
        bool hasTipoEstoque = false;
        bool hasQtdLeZero = false;

        foreach (var c in conds)
        {
            if (Regex.IsMatch(c, @"^idmovel\s*=\s*(?:101|'101'|""101"")$", RegexOptions.IgnoreCase))
            {
                hasIdMovel101 = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^tipo\s*=\s*(?:'ESTOQUE'|""ESTOQUE"")$", RegexOptions.IgnoreCase))
            {
                hasTipoEstoque = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^quantidade\s*<=\s*(?:0|'0'|""0"")$", RegexOptions.IgnoreCase))
            {
                hasQtdLeZero = true;
                continue;
            }

            return false;
        }

        return hasIdMovel101 && hasTipoEstoque && hasQtdLeZero;
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
