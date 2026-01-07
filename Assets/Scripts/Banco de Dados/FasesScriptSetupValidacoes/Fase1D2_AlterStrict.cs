using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase1D2_AlterStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "ALTER";
        furniture.successMessage = "Parabéns, você concluiu o desafio!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = false;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateAlterExact);
    }

    private bool ValidateAlterExact(string sql, int _, DatabaseManager __)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        
        string cleaned = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"/\*.*?\*/", "", RegexOptions.Singleline);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        
        var rx = new Regex(@"^alter table brinquedos add column cor_favorita text$",
                           RegexOptions.IgnoreCase);
        return rx.IsMatch(cleaned);
    }
}
