using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase5D2_DeleteDuplicatasCanceladasArmarioSalaStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    private const int ID_ARMARIO_SALA = 204;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "DELETE";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateDeleteDuplicatasCanceladas_AnyOrder);
    }

    private bool ValidateDeleteDuplicatasCanceladas_AnyOrder(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

       
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var m = Regex.Match(
            cleaned,
            @"^\s*delete\s+from\s+itens\s+where\s+(.*?)\s+and\s+iditem\s+not\s+in\s*\(\s*(.*?)\s*\)\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m.Success) return false;

        string outerWhereBody = NormalizeSpaces(m.Groups[1].Value);
        string innerSelectBody = NormalizeSpaces(m.Groups[2].Value);

        
        var outerConds = Regex.Split(outerWhereBody, @"\s+and\s+", RegexOptions.IgnoreCase)
                              .Select(NormalizeSpaces)
                              .Where(s => s.Length > 0)
                              .ToList();

        if (outerConds.Count != 3) return false;

        bool hasIdMovel204 = false;
        bool hasTipoTarefa = false;
        bool hasStatusCancelada = false;

        foreach (var c in outerConds)
        {
            if (Regex.IsMatch(c, @"^idmovel\s*=\s*(?:204|'204'|""204"")$", RegexOptions.IgnoreCase))
            {
                hasIdMovel204 = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^tipo\s*=\s*(?:'TAREFA'|""TAREFA"")$", RegexOptions.IgnoreCase))
            {
                hasTipoTarefa = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^status\s*=\s*(?:'CANCELADA'|""CANCELADA"")$", RegexOptions.IgnoreCase))
            {
                hasStatusCancelada = true;
                continue;
            }

            return false;
        }

        if (!(hasIdMovel204 && hasTipoTarefa && hasStatusCancelada)) return false;

      
        var m2 = Regex.Match(
            innerSelectBody,
            @"^\s*select\s+min\s*\(\s*iditem\s*\)\s+" +
            @"from\s+itens\s+" +
            @"where\s+(.*?)\s+" +
            @"group\s+by\s+nomeitem\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        );
        if (!m2.Success) return false;

        string innerWhereBody = NormalizeSpaces(m2.Groups[1].Value);

        var innerConds = Regex.Split(innerWhereBody, @"\s+and\s+", RegexOptions.IgnoreCase)
                              .Select(NormalizeSpaces)
                              .Where(s => s.Length > 0)
                              .ToList();

        if (innerConds.Count != 3) return false;

        bool inHasIdMovel204 = false;
        bool inHasTipoTarefa = false;
        bool inHasStatusCancelada = false;

        foreach (var c in innerConds)
        {
            if (Regex.IsMatch(c, @"^idmovel\s*=\s*(?:204|'204'|""204"")$", RegexOptions.IgnoreCase))
            {
                inHasIdMovel204 = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^tipo\s*=\s*(?:'TAREFA'|""TAREFA"")$", RegexOptions.IgnoreCase))
            {
                inHasTipoTarefa = true;
                continue;
            }

            if (Regex.IsMatch(c, @"^status\s*=\s*(?:'CANCELADA'|""CANCELADA"")$", RegexOptions.IgnoreCase))
            {
                inHasStatusCancelada = true;
                continue;
            }

            return false;
        }

        return inHasIdMovel204 && inHasTipoTarefa && inHasStatusCancelada;
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
