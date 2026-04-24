using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RN04 – puzzle da primeira sala: exige ordem correta de cores no béquer (Azul → Vermelho → Verde, ver BeakerLiquidSystem),
/// os três tubos tenham contribuído e o béquer atinja um nível mínimo; ao resolver dispara o evento (ex.: destrancar porta + som).
/// </summary>
public class ChemistryBeakerPuzzle : MonoBehaviour
{
    [SerializeField]
    private BeakerLiquidSystem beaker;

    [SerializeField, Range(0f, 1f)]
    private float minFillToSolve = 0.55f;

    [SerializeField]
    private UnityEvent onSolved;

    public bool IsSolved { get; private set; }

    private void Awake()
    {
        if (beaker == null)
            beaker = BeakerLiquidSystem.Instance;
    }

    private void Start()
    {
        TrySolveImmediate();
    }

    private void OnEnable()
    {
        if (beaker != null)
            beaker.FillOrColorChanged += OnFillChanged;
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
