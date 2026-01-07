using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class Fase6D1_Index_EtiquetasNome_Strict : MonoBehaviour
{
    [Header("Alvo da interação")]
    public FurnitureInteractable furniture;

    void Awake()
    {
        if (!furniture) furniture = FindObjectOfType<FurnitureInteractable>();

        furniture.allowedOp = "CREATE";
        furniture.successMessage = "Índice criado corretamente!";
        furniture.closeDelaySeconds = 1.0f;
        furniture.autoCloseOnSuccess = true;

        furniture.validator = new Func<string, int, DatabaseManager, bool>(ValidateCreateIndexNome);
    }

    private class IndexListRow { public int seq { get; set; } public string name { get; set; } public int unique { get; set; } public string origin { get; set; } public int partial { get; set; } }
    private class IndexInfoRow { public int seqno { get; set; } public int cid { get; set; } public string name { get; set; } }

    private bool ValidateCreateIndexNome(string sql, int _, DatabaseManager db)
    {
        if (string.IsNullOrWhiteSpace(sql)) return false;

        // limpa comentários, ; final e normaliza espaços
        string cleaned = Regex.Replace(sql, @"--.*?$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"/\*.*?\*/", "", RegexOptions.Singleline);
        cleaned = Regex.Replace(cleaned, @";\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        // deve ser exatamente: CREATE INDEX idx_etiquetas_nome ON etiquetas (nome)
        var rx = new Regex(
            @"^create\s+index\s+idx_etiquetas_nome\s+on\s+etiquetas\s*\(\s*nome\s*\)$",
            RegexOptions.IgnoreCase
        );
        if (!rx.IsMatch(cleaned)) return false;

        // confere via PRAGMA que o índice existe, é não-único e tem só a coluna 'nome'
        var list = db.ExecuteQuery<IndexListRow>("PRAGMA index_list('etiquetas')");
        var idx = list.FirstOrDefault(i => i.name.Equals("idx_etiquetas_nome", StringComparison.OrdinalIgnoreCase));
        if (idx == null) return false;
        if (idx.unique != 0) return false; // tem que ser não-único

        var cols = db.ExecuteQuery<IndexInfoRow>("PRAGMA index_info('idx_etiquetas_nome')");
        if (cols == null || cols.Count != 1) return false;
        if (!cols[0].name.Equals("nome", StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }
}
