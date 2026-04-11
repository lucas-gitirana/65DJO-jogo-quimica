using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RN04 – puzzle da primeira sala: exige que os três tubos tenham contribuído e o béquer atinja um nível mínimo.
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

        IsSolved = true;
        onSolved?.Invoke();
    }
}
