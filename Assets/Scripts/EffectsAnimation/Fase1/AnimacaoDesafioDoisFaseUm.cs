using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimacaoDesafioDoisFaseUm : MonoBehaviour
{
    [Header("Alvo (se vazio, usa este GameObject)")]
    public GameObject target;

    [Header("Cor a aplicar")]
    public Color targetColor = Color.blue;

    void Awake()
    {
        if (!target) target = gameObject;
    }

    /// <summary>
    /// Chame este método no sucesso do desafio (UnityEvent).
    /// </summary>
    public void AplicarAzul()
    {
        // 2D: Sprites
        foreach (var sr in target.GetComponentsInChildren<SpriteRenderer>(true))
        {
            var c = sr.color;
            c.r = targetColor.r; c.g = targetColor.g; c.b = targetColor.b; // mantém alpha original
            sr.color = c;
        }

        // 3D: Materiais de MeshRenderer (instancia material pra não afetar shared material)
        foreach (var mr in target.GetComponentsInChildren<MeshRenderer>(true))
        {
            var mats = mr.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                var c = mats[i].color;
                c.r = targetColor.r; c.g = targetColor.g; c.b = targetColor.b; // mantém alpha
                mats[i].color = c;
            }
        }

        // UI: Imagens
        foreach (var img in target.GetComponentsInChildren<Image>(true))
        {
            var c = img.color;
            c.r = targetColor.r; c.g = targetColor.g; c.b = targetColor.b; // mantém alpha
            img.color = c;
        }
    }

    // Se quiser aplicar automaticamente ao iniciar, descomente:
    // void Start() => AplicarAzul();

    // Update vazio, pode remover se quiser
    void Update() { }
}