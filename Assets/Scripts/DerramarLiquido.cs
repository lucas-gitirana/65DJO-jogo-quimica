using UnityEngine;

/// <summary>
/// Derrama líquido do tubo para o BeakerLiquidSystem enquanto o tubo está na zona e inclinado.
/// Reduz a escala Y do líquido do tubo em proporção ao volume restante.
/// </summary>
public class DerramarLiquido : MonoBehaviour
{
    [SerializeField]
    private float anguloMin = 100f;

    [Tooltip("Índice 0–2: deve bater com as cores em BeakerLiquidSystem.")]
    [SerializeField, Range(0, 2)]
    private int tubeIndex;

    [Tooltip("Volume normalizado inicial deste tubo (1 = cheio). Três tubos com 1 cada somam mais que o béquer; o excesso é bloqueado pelo sistema.")]
    [SerializeField, Range(0.01f, 1f)]
    private float initialTubeVolume = 1f;

    [Tooltip("Unidades normalizadas transferidas por segundo enquanto derrama.")]
    [SerializeField]
    private float pourRateNormalized = 0.35f;

    [SerializeField]
    private Transform tubeLiquidTransform;

    [SerializeField]
    private float fullLiquidScaleY = 1f;

    private bool _inPourZone;
    private bool _wasPouring;
    private float _remainingNormalized = 1f;
    private BeakerLiquidSystem _beaker;
    private Outline _outline;
    private int _audioKey;

    private void Awake()
    {
        _audioKey = GetInstanceID();
        _remainingNormalized = initialTubeVolume;
        if (tubeLiquidTransform == null && transform.childCount > 0)
            tubeLiquidTransform = transform.GetChild(0);
        if (tubeLiquidTransform != null)
            fullLiquidScaleY = tubeLiquidTransform.localScale.y;
        TryGetComponent(out _outline);
    }

    private void Start()
    {
        _beaker = BeakerLiquidSystem.Instance;
        if (_beaker == null)
            _beaker = FindFirstObjectByType<BeakerLiquidSystem>();
        ApplyTubeLiquidVisual();
    }

    private void Update()
    {
        if (_beaker == null)
            return;

        bool tilted = Vector3.Angle(transform.up, Vector3.up) > anguloMin;
        bool canPour = _inPourZone && tilted && _remainingNormalized > 0.001f && _beaker.NormalizedFill < 0.999f;
        bool isPouring = canPour;

        if (isPouring)
        {
            float delta = pourRateNormalized * Time.deltaTime;
            delta = Mathf.Min(delta, _remainingNormalized);
            float accepted = _beaker.TryAddFromTube(tubeIndex, delta);
            _remainingNormalized -= accepted;
            ApplyTubeLiquidVisual();
        }

        if (isPouring != _wasPouring)
        {
            var coord = PourAudioCoordinator.Instance;
            if (coord != null)
                coord.SetPouringActive(_audioKey, isPouring);
            _wasPouring = isPouring;
        }
    }

    private void ApplyTubeLiquidVisual()
    {
        if (tubeLiquidTransform == null)
            return;
        var s = tubeLiquidTransform.localScale;
        float t = initialTubeVolume > 0.001f ? _remainingNormalized / initialTubeVolume : 0f;
        t = Mathf.Clamp01(t);
        s.y = fullLiquidScaleY * t;
        tubeLiquidTransform.localScale = s;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("ZonaDerramar"))
            return;
        if (_outline != null)
            _outline.OutlineWidth = 5f;
        _inPourZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("ZonaDerramar"))
            return;
        if (_outline != null)
            _outline.OutlineWidth = 0f;
        _inPourZone = false;
    }

    private void OnDisable()
    {
        var coord = PourAudioCoordinator.Instance;
        if (coord != null)
            coord.SetPouringActive(_audioKey, false);
        _wasPouring = false;
    }
}
