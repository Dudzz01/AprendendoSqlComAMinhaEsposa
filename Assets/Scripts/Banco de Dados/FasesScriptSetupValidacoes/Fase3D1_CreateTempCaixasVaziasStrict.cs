// Fase 3 — Desafio 1
// Gabarito (ordem das colunas LIVRE):
// CREATE TEMP TABLE CAIXASVAZIAS(
//   id       INTEGER PRIMARY KEY,
//   nome     TEXT    NOT NULL,
//   tamanho  INTEGER NOT NULL
// );

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase3D1_CreateTempCaixasVaziasStrict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Tabela temporária criada corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateTemp);
    }

    private bool ValidateCreateTemp(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // 1) normalização superficial do texto (sem prender em espaços/maiusc.)
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
        string lower = cleaned.ToLowerInvariant();

        // rejeita constraints/keywords fora do gabarito
        if (Regex.IsMatch(lower, @"\b(autoincrement|foreign\s+key|constraint|unique|check|primary\s+key\s*\()", RegexOptions.IgnoreCase))
            return false;

        // 2) tem que ser CREATE TEMP(ORARY) TABLE CAIXASVAZIAS(...)
        var m = Regex.Match(lower, @"^\s*create\s+temp(orary)?\s+table\s+caixasvazias\s*\((.*)\)\s*$",
                            RegexOptions.Singleline);
        if (!m.Success) return false;

        

        // 4) valida esquema via PRAGMA (ordem livre, tipos/NOT NULL/PK exatos)
        var info = db.GetTableInfo("caixasvazias");
        if (info == null || info.Count == 0) return false;

        // nomes exatamente estes, sem extras (ordem livre)
        var expectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", "nome", "tamanho" };
        var actualNames = new HashSet<string>(info.Select(c => c.name), StringComparer.OrdinalIgnoreCase);
        if (!expectedNames.SetEquals(actualNames)) return false;

        var cols = info.ToDictionary(c => c.name.ToLowerInvariant(), c => c);

        bool TypeIs(string declared, string want) =>
            !string.IsNullOrWhiteSpace(declared) &&
            declared.Trim().Equals(want, StringComparison.OrdinalIgnoreCase);

        // id INTEGER PRIMARY KEY
        if (!cols.ContainsKey("id")) return false;
        if (!TypeIs(cols["id"].type, "INTEGER")) return false;
        if (cols["id"].pk != 1) return false;

        // nome TEXT NOT NULL
        if (!cols.ContainsKey("nome")) return false;
        if (!TypeIs(cols["nome"].type, "TEXT")) return false;
        if (cols["nome"].notnull != 1) return false;

        // tamanho INTEGER NOT NULL
        if (!cols.ContainsKey("tamanho")) return false;
        if (!TypeIs(cols["tamanho"].type, "INTEGER")) return false;
        if (cols["tamanho"].notnull != 1) return false;

        return true;
    }

    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }
}
