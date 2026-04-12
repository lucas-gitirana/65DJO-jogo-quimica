using System;
using UnityEngine;

/// <summary>
/// Estado central do béquer: nível de enchimento (cap 1), máscara de tubos que já contribuíram e cor resultante.
/// A soma do volume transferido dos tubos não ultrapassa 1 (unidades normalizadas).
/// </summary>
[DisallowMultipleComponent]
public class BeakerLiquidSystem : MonoBehaviour
{
    public static BeakerLiquidSystem Instance { get; private set; }

    [Header("Visual do líquido no béquer")]
    [SerializeField]
    private Transform beakerLiquidTransform;

    [Tooltip("Escala Y local quando o béquer está cheio (no prefab o líquido costuma começar em 0).")]
    [SerializeField]
    private float fullBeakerScaleY = 1f;

    [SerializeField]
    private Renderer beakerLiquidRenderer;

    [Tooltip("Nome da propriedade de cor no shader (Built-in/URP Lit: _BaseColor ou _Color).")]
    [SerializeField]
    private string colorPropertyName = "_BaseColor";

    [SerializeField]
    private bool alsoSetColor = true;

    [Header("Cores por índice de tubo (0, 1, 2)")]
    [SerializeField]
    private Color[] tubeColors =
    {
        new Color(1f, 0.2f, 0.2f),
        new Color(0.2f, 0.5f, 1f),
        new Color(0.2f, 0.85f, 0.2f)
    };

    [SerializeField, Range(0f, 1f)]
    private float initialBeakerFill;

    /// <summary>Volume já recebido no béquer [0,1].</summary>
    public float NormalizedFill => _fill;

    /// <summary>Bits 0..2 = tubos que já derramaram algum volume.</summary>
    public int TubeContributionMask => _tubeMask;

    public int DistinctTubeContributionCount
    {
        get
        {
            int c = 0;
            for (int i = 0; i < 3; i++)
            {
                if ((_tubeMask & (1 << i)) != 0)
                    c++;
            }

            return c;
        }
    }

    public event Action FillOrColorChanged;

    private float _fill;
    private int _tubeMask;
    private MaterialPropertyBlock _mpb;
    private bool _usePropertyBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Mais de um BeakerLiquidSystem na cena; mantendo o primeiro.");
            enabled = false;
            return;
        }

        Instance = this;
        _fill = Mathf.Clamp01(initialBeakerFill);
        _mpb = new MaterialPropertyBlock();
        if (beakerLiquidRenderer != null)
            _usePropertyBlock = true;
        UpdateMixedColor();
        ApplyBeakerVisual();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Transfere volume normalizado do tubo para o béquer. Retorna quanto foi efetivamente aceito.
    /// </summary>
    public float TryAddFromTube(int tubeIndex, float normalizedAmount)
    {
        tubeIndex = Mathf.Clamp(tubeIndex, 0, 2);
        normalizedAmount = Mathf.Max(0f, normalizedAmount);
        float room = 1f - _fill;
        float accepted = Mathf.Min(normalizedAmount, room);
        if (accepted <= 0f)
            return 0f;

        _fill += accepted;
        _tubeMask |= 1 << tubeIndex;
        UpdateMixedColor();
        ApplyBeakerVisual();
        FillOrColorChanged?.Invoke();
        return accepted;
    }

    private void UpdateMixedColor()
    {
        // Mistura simples: média das cores dos tubos que já contribuíram (RN02).
        Color sum = Color.black;
        int n = 0;
        for (int i = 0; i < 3; i++)
        {
            if ((_tubeMask & (1 << i)) == 0)
                continue;
            Color c = i < tubeColors.Length ? tubeColors[i] : Color.white;
            sum += c;
            n++;
        }

        _mixed = n > 0 ? sum / n : Color.white;
    }

    private Color _mixed = Color.clear;

    private void ApplyBeakerVisual()
    {
        if (beakerLiquidTransform != null)
        {
            var s = beakerLiquidTransform.localScale;
            s.y = Mathf.Lerp(0f, fullBeakerScaleY, _fill);
            beakerLiquidTransform.localScale = s;
        }

        if (beakerLiquidRenderer == null || !alsoSetColor)
            return;

        if (_usePropertyBlock)
        {
            beakerLiquidRenderer.GetPropertyBlock(_mpb);
            TrySetColorOnBlock(_mpb, _mixed);
            beakerLiquidRenderer.SetPropertyBlock(_mpb);
        }
        else if (beakerLiquidRenderer.material != null)
        {
            if (beakerLiquidRenderer.material.HasProperty(colorPropertyName))
                beakerLiquidRenderer.material.SetColor(colorPropertyName, _mixed);
        }
    }

    private static void TrySetColorOnBlock(MaterialPropertyBlock block, Color c)
    {
        if (block == null)
            return;
        // Tenta propriedades comuns sem depender do pipeline.
        block.SetColor(BaseColorId, c);
        block.SetColor(ColorId, c);
    }

    public Color GetTubeColor(int tubeIndex)
    {
        tubeIndex = Mathf.Clamp(tubeIndex, 0, 2);
        return tubeIndex < tubeColors.Length ? tubeColors[tubeIndex] : Color.white;
    }
}
