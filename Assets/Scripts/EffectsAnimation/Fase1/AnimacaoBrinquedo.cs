using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimacaoBrinquedo : MonoBehaviour
{
    [Header("Alvo (se vazio, usa este GameObject)")]
    public GameObject target;

    [Header("Fade")]
    public float fadeDuration = 0.6f;

    [Header("Pop de escala (opcional)")]
    public bool popScale = true;
    [Range(0.5f, 1.0f)] public float startScale = 0.9f;     // escala inicial (90%)
    [Range(1.0f, 1.2f)] public float overshoot = 1.05f;     // leve overshoot (105%)

    // cache
    private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();
    private List<Image> _images = new List<Image>();
    private Vector3 _originalScale;

    void Awake()
    {
        if (!target) target = gameObject; // se não setar, usa o próprio
        // pega renderers mesmo se o alvo estiver inativo
        _sprites.AddRange(target.GetComponentsInChildren<SpriteRenderer>(true));
        _images.AddRange(target.GetComponentsInChildren<Image>(true));
        _originalScale = target.transform.localScale;
    }

    /// <summary>
    /// Chame este método no OnSuccess (UnityEvent) do seu desafio.
    /// </summary>
    public void Revelar()
    {
        StopAllCoroutines();
        StartCoroutine(RevelarRoutine());
    }

    private IEnumerator RevelarRoutine()
    {
        // ativa o GO antes de animar
        if (!target.activeSelf) target.SetActive(true);

        // zera alpha
        foreach (var sr in _sprites) { var c = sr.color; c.a = 0f; sr.color = c; }
        foreach (var im in _images) { var c = im.color; c.a = 0f; im.color = c; }

        // escala inicial opcional
        if (popScale)
            target.transform.localScale = _originalScale * startScale;

        // fade-in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / fadeDuration);

            foreach (var sr in _sprites) { var c = sr.color; c.a = k; sr.color = c; }
            foreach (var im in _images) { var c = im.color; c.a = k; im.color = c; }

            if (popScale)
                target.transform.localScale = Vector3.Lerp(_originalScale * startScale, _originalScale * overshoot, k);

            yield return null;
        }

        // assenta overshoot rapidamente
        if (popScale)
        {
            float settle = 0.12f;
            float u = 0f;
            var over = _originalScale * overshoot;
            while (u < settle)
            {
                u += Time.deltaTime;
                target.transform.localScale = Vector3.Lerp(over, _originalScale, u / settle);
                yield return null;
            }
            target.transform.localScale = _originalScale;
        }
    }
}
