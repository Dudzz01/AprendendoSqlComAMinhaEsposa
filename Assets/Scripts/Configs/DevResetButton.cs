using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class DevResetButton : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button resetAllButton;

    [Header("Opções")]
    [SerializeField] private bool reloadScene = true;
    [SerializeField] private string sceneToReload = ""; // vazio = cena atual

    private void Awake()
    {
        if (resetAllButton) resetAllButton.onClick.AddListener(ResetAll);
    }

    [ContextMenu("Reset ALL (DB + Save)")]
    public void ResetAll()
    {
        // 1) Reset do SAVE (memória + arquivo)
        GameController.s = new Save();                              // memória
        GameController.s.arrayFasesDesbloqueadas[0] = true;
        GameController.s.desafiosConcluidos = 0;
        var savePath = Path.Combine(Application.persistentDataPath, "saveSceneGame.save");
        if (File.Exists(savePath)) File.Delete(savePath);           // arquivo

        // 2) Reset do DB para o template
        var db = FindObjectOfType<DatabaseManager>();
        if (db != null) db.ResetToTemplate();
        else
        {
            // fallback raro: se não achar o manager na cena
            var dst = Path.Combine(Application.persistentDataPath, "gameData.db");
            if (File.Exists(dst)) File.Delete(dst);
            DatabaseUtils.GetDatabasePath("gameData.db");
        }

        Debug.Log("[DevReset] Save e DB resetados (template reaplicado).");

        // 3) Recarrega a cena, se marcado (garante que tudo reabra limpo)
        if (reloadScene)
        {
            var name = string.IsNullOrWhiteSpace(sceneToReload)
                ? SceneManager.GetActiveScene().name
                : sceneToReload;
            SceneManager.LoadScene(name);
        }
    }

    [ContextMenu("Reset SAVE only")]
    public void ResetSaveOnly()
    {
        GameController.s = new Save();
        GameController.s.arrayFasesDesbloqueadas[0] = true;
        var savePath = Path.Combine(Application.persistentDataPath, "saveSceneGame.save");
        if (File.Exists(savePath)) File.Delete(savePath);
        Debug.Log("[DevReset] SAVE resetado.");
        if (reloadScene) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Reset DB only")]
    public void ResetDbOnly()
    {
        var db = FindObjectOfType<DatabaseManager>();
        if (db != null) db.ResetToTemplate();
        else
        {
            var dst = Path.Combine(Application.persistentDataPath, "gameData.db");
            if (File.Exists(dst)) File.Delete(dst);
            DatabaseUtils.GetDatabasePath("gameData.db");
        }
        Debug.Log("[DevReset] DB resetado para template.");
        if (reloadScene) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
