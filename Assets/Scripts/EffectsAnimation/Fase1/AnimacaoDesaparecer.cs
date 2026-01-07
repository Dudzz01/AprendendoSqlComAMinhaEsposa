using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimacaoDesaparecer : MonoBehaviour
{
    [Header("Alvo (se vazio, usa este GameObject)")]
    public GameObject target;

    [Header("Fade")]
    public float fadeDuration = 0.6f;

    [Header("Shrink de escala (opcional)")]
    public bool shrinkScale = true;
    [Range(1.0f, 1.2f)] public float startScale = 1.05f;   // começa levemente maior
    [Range(0.5f, 1.0f)] public float endScale = 0.9f;      // termina menor

    [Header("Comportamento final")]
    public bool disableAtEnd = true;                       // desativar GO ao final?

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
    public void Desaparecer()
    {
        StopAllCoroutines();
        StartCoroutine(DesaparecerRoutine());
    }

    private IEnumerator DesaparecerRoutine()
    {
        // garante que o GO esteja ativo antes de animar
        if (!target.activeSelf) target.SetActive(true);

        // garante alpha 1 no início
        foreach (var sr in _sprites)
        {
            var c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
        foreach (var im in _images)
        {
            var c = im.color;
            c.a = 1f;
            im.color = c;
        }

        // escala inicial opcional (começa maior pra depois encolher)
        if (shrinkScale)
            target.transform.localScale = _originalScale * startScale;
        else
            target.transform.localScale = _originalScale;

        // fade-out
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / fadeDuration);

            float alpha = Mathf.Lerp(1f, 0f, k);

            foreach (var sr in _sprites)
            {
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            foreach (var im in _images)
            {
                var c = im.color;
                c.a = alpha;
                im.color = c;
            }

            if (shrinkScale)
            {
                float scaleFactor = Mathf.Lerp(startScale, endScale, k);
                target.transform.localScale = _originalScale * scaleFactor;
            }

            yield return null;
        }

        // garante alpha 0 no final
        foreach (var sr in _sprites)
        {
            var c = sr.color;
            c.a = 0f;
            sr.color = c;
        }
        foreach (var im in _images)
        {
            var c = im.color;
            c.a = 0f;
            im.color = c;
        }

        if (shrinkScale)
            target.transform.localScale = _originalScale * endScale;

        if (disableAtEnd)
            target.SetActive(false);
    }
}