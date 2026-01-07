using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase5D1_CreateEtiquetas_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Tabela 'etiquetas' criada corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateEtiquetasStrict);
    }

    private bool ValidateCreateEtiquetasStrict(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // 1) STRICT do comando (mesma linha de raciocínio dos anteriores)
        string cleaned = StripComments(sql);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = NormalizeSpaces(cleaned);

        // Regex geral do CREATE TABLE etiquetas (não analisamos colunas aqui — PRAGMA cuida disso)
        var rxHead = new Regex(@"^create\s+table\s+etiquetas\s*\(.*\)$",
                               RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (!rxHead.IsMatch(cleaned)) return false;

        // 2) PRAGMA — validar esquema exato
        var info = db.GetTableInfo("etiquetas");
        if (info == null || info.Count == 0) return false;

        // conjunto de colunas — exatamente estas 4
        var expectedNames = new HashSet<string>(new[] { "id", "nome", "cor", "created_at" }, StringComparer.OrdinalIgnoreCase);
        var actualNames = new HashSet<string>(info.Select(c => c.name), StringComparer.OrdinalIgnoreCase);
        if (!expectedNames.SetEquals(actualNames)) return false;

        var cols = info.ToDictionary(c => c.name.ToLowerInvariant(), c => c);

        bool TypeIs(string declared, string want) =>
            !string.IsNullOrWhiteSpace(declared) &&
            declared.Trim().Equals(want, StringComparison.OrdinalIgnoreCase);
        string Dflt(string s) => (s ?? string.Empty).Trim();

        // id: INTEGER PRIMARY KEY
        if (!TypeIs(cols["id"].type, "INTEGER")) return false;
        if (cols["id"].pk != 1) return false;

        // nome: TEXT NOT NULL UNIQUE
        if (!TypeIs(cols["nome"].type, "TEXT")) return false;
        if (cols["nome"].notnull != 1) return false;

        // cor: TEXT (opcional)
        if (!TypeIs(cols["cor"].type, "TEXT")) return false;
        if (cols["cor"].notnull != 0) return false;

        // created_at: TEXT DEFAULT CURRENT_DATE
        if (!TypeIs(cols["created_at"].type, "TEXT")) return false;
        if (!Dflt(cols["created_at"].dflt_value).Equals("CURRENT_DATE", StringComparison.OrdinalIgnoreCase)) return false;

        // UNIQUE(nome) — pode virar autoindex; validamos que exista um índice único só com 'nome'
        var uniques = db.GetUniqueIndexes("etiquetas"); // inclui auto-indexes
        bool hasUniqueOnNome = uniques.Any(u =>
            u.columns.Count == 1 &&
            u.columns[0].Equals("nome", StringComparison.OrdinalIgnoreCase)
        );
        if (!hasUniqueOnNome) return false;

        // Sem FKs nessa tabela
        if (db.GetForeignKeys("etiquetas").Count != 0) return false;

        return true;
    }

    private static string StripComments(string s)
    {
        s = Regex.Replace(s, @"--.*?$", "", RegexOptions.Multiline);
        s = Regex.Replace(s, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return s;
    }
    private static string NormalizeSpaces(string s) => Regex.Replace(s, @"\s+", " ").Trim();
}
