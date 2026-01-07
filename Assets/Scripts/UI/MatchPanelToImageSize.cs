using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class MatchPanelToImageSize : MonoBehaviour
{
    
    public RectTransform sourceImageRect;

    RectTransform _panelRect;

    void Awake()
    {
        _panelRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (sourceImageRect == null) return;

        
        Vector2 imageSize = sourceImageRect.sizeDelta;

        
        _panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal, imageSize.x);
        _panelRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical, imageSize.y);
    }
}
