using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase6D2_Alter_AddFKEtiqueta_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "ALTER";
        furniture.successMessage = "Coluna e FK adicionadas corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateAlterAddFk);
    }

    private bool ValidateAlterAddFk(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // limpa comentários, ; final e normaliza espaços
        string cleaned = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"/\*.*?\*/", "", RegexOptions.Singleline);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        // deve ser exatamente (com espaços livres):
        // ALTER TABLE brinquedos ADD COLUMN etiqueta_id INTEGER REFERENCES etiquetas(id) ON DELETE SET NULL
        var rx = new Regex(
            @"^alter\s+table\s+brinquedos\s+add\s+column\s+etiqueta_id\s+integer\s+references\s+etiquetas\s*\(\s*id\s*\)\s+on\s+delete\s+set\s+null$",
            RegexOptions.IgnoreCase
        );
        if (!rx.IsMatch(cleaned)) return false;

        // PRAGMA table_info: coluna existe, tipo INTEGER, sem NOT NULL e sem DEFAULT
        var cols = db.GetTableInfo("brinquedos");
        if (cols == null || cols.Count == 0) return false;

        var col = cols.FirstOrDefault(c => c.name.Equals("etiqueta_id", StringComparison.OrdinalIgnoreCase));
        if (col == null) return false;
        if (!(col.type ?? "").Trim().Equals("INTEGER", StringComparison.OrdinalIgnoreCase)) return false;
        if (col.notnull != 0) return false; // tem que permitir NULL (por causa do SET NULL)
        if (!string.IsNullOrEmpty((col.dflt_value ?? "").Trim())) return false; // sem default

        // PRAGMA foreign_key_list: FK da coluna etiqueta_id -> etiquetas(id) com ON DELETE SET NULL
        var fks = db.GetForeignKeys("brinquedos");
        var fk = fks.FirstOrDefault(f =>
            f.from.Equals("etiqueta_id", StringComparison.OrdinalIgnoreCase) &&
            f.table.Equals("etiquetas", StringComparison.OrdinalIgnoreCase) &&
            f.to.Equals("id", StringComparison.OrdinalIgnoreCase)
        );
        if (fk == null) return false;
        if (!"SET NULL".Equals((fk.on_delete ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }
}
