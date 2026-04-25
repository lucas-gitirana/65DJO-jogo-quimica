using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Após os três tubos terem contribuído e o béquer atingir o enchimento mínimo, se a ordem dos primeiros derrames
/// não coincidir com <see cref="BeakerLiquidSystem"/>, mostra o Canvas de game over e pausa (alinhado ao <see cref="ChemistryBeakerPuzzle"/>).
/// </summary>
[DisallowMultipleComponent]
public class ShowMessageGameOver : MonoBehaviour
{
    [SerializeField]
    private BeakerLiquidSystem beaker;

    [Tooltip("Deve coincidir com o minFillToSolve do ChemistryBeakerPuzzle na mesma sala.")]
    [SerializeField, Range(0f, 1f)]
    private float minFillToFail = 0.97f;

    [Tooltip("Se atribuído, não mostra game over se o puzzle do béquer já estiver resolvido.")]
    [SerializeField]
    private ChemistryBeakerPuzzle beakerPuzzle;

    [Tooltip("Canvas raiz ou painel com a mensagem. O script ativa o Canvas ancestral se estiver desligado.")]
    [SerializeField]
    private GameObject messageObject;

    [SerializeField]
    private GameObject backdropObject;

    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private LocomotionMediator locomotionSystem;

    [SerializeField]
    private XRRayInteractor leftRay;

    [SerializeField]
    private XRRayInteractor rightRay;

    [Tooltip("Se falso, os raios ficam ativos na pausa (útil para clicar no botão em VR).")]
    [SerializeField]
    private bool disableControllerRaysWhenPaused = false;

    [SerializeField]
    private bool quitOnEscapeAfterTrigger = true;

    private bool _shown;

    private void Awake()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitApplication);
    }

    private void OnDestroy()
    {
        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitApplication);
    }

    private void OnEnable()
    {
        EnsureBeakerReference();
        if (beaker == null)
            return;
        beaker.FillOrColorChanged -= OnFillChanged;
        beaker.FillOrColorChanged += OnFillChanged;
    }

    private void OnDisable()
    {
        if (beaker != null)
            beaker.FillOrColorChanged -= OnFillChanged;
    }

    private void Update()
    {
        if (!_shown || !quitOnEscapeAfterTrigger)
            return;
        if (IsEscapePressed())
            QuitApplication();
    }

    private void EnsureBeakerReference()
    {
        if (beaker != null)
            return;
        beaker = BeakerLiquidSystem.Instance;
        if (beaker == null)
            beaker = FindFirstObjectByType<BeakerLiquidSystem>();
    }

    private void OnFillChanged()
    {
        if (_shown)
            return;
        EnsureBeakerReference();
        if (beaker == null)
            return;
        if (beakerPuzzle != null && beakerPuzzle.IsSolved)
            return;
        if (beaker.DistinctTubeContributionCount < 3)
            return;
        if (beaker.NormalizedFill < minFillToFail)
            return;
        if (beaker.IsFirstPourOrderCorrectForPuzzle())
            return;

        ShowFailureUi();
        _shown = true;

        Time.timeScale = 0f;
        if (locomotionSystem != null)
            locomotionSystem.enabled = false;
        if (disableControllerRaysWhenPaused)
        {
            if (leftRay != null)
                leftRay.enabled = false;
            if (rightRay != null)
                rightRay.enabled = false;
        }
    }

    private void ShowFailureUi()
    {
        var canvas = FindCanvas(messageObject) ?? FindCanvas(backdropObject);
        if (canvas != null)
            canvas.gameObject.SetActive(true);
        if (backdropObject != null)
            backdropObject.SetActive(true);
        if (messageObject != null)
            messageObject.SetActive(true);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static Canvas FindCanvas(GameObject go)
    {
        return go == null ? null : go.GetComponentInParent<Canvas>(true);
    }

    private static bool IsEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
