using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RN04 – puzzle da primeira sala: exige ordem correta de cores no béquer (Azul → Vermelho → Verde, ver BeakerLiquidSystem),
/// os três tubos tenham contribuído e o béquer atinja um nível mínimo de enchimento; ao resolver dispara o evento (ex.: destrancar porta + som).
/// </summary>
public class ChemistryBeakerPuzzle : MonoBehaviour
{
    [SerializeField]
    private BeakerLiquidSystem beaker;

    [Tooltip(
        "Enchimento normalizado mínimo do béquer (0–1) para marcar como resolvido. Valores baixos (ex.: 0,55) disparam a vitória " +
        "logo que o 3.º tubo começa a derramar — o som sai antes do último tubo esvaziar. Usa ~0,95–0,99 para só resolver com o béquer quase cheio.")]
    [SerializeField, Range(0f, 1f)]
    private float minFillToSolve = 0.97f;

    [SerializeField]
    private UnityEvent onSolved;

    public bool IsSolved { get; private set; }

    private void Start()
    {
        EnsureBeakerReference();
        TrySolveImmediate();
    }

    private void OnEnable()
    {
        EnsureBeakerReference();
        if (beaker == null)
            return;
        beaker.FillOrColorChanged -= OnFillChanged;
        beaker.FillOrColorChanged += OnFillChanged;
    }

    /// <summary>
    /// Garante referência após todos os Awake (OnEnable/Start), com fallback como em <see cref="DerramarLiquido"/>.
    /// </summary>
    private void EnsureBeakerReference()
    {
        if (beaker != null)
            return;
        beaker = BeakerLiquidSystem.Instance;
        if (beaker == null)
            beaker = FindFirstObjectByType<BeakerLiquidSystem>();
    }

    private void OnDisable()
    {
        if (beaker != null)
            beaker.FillOrColorChanged -= OnFillChanged;
    }

    private void OnFillChanged()
    {
        TrySolveImmediate();
    }

    private void TrySolveImmediate()
    {
        if (IsSolved || beaker == null)
            return;
        if (beaker.DistinctTubeContributionCount < 3)
            return;
        if (beaker.NormalizedFill < minFillToSolve)
            return;
        if (!beaker.IsFirstPourOrderCorrectForPuzzle())
            return;

        IsSolved = true;
        onSolved?.Invoke();
    }
}
