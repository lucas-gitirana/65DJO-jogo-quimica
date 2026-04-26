using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

/// <summary>
/// Exibe UI de Game Over quando a ordem de derramamento no béquer está incorreta
/// (evento <see cref="GamePuzzleEvents.BeakerPourSequenceFailed"/>).
/// </summary>
public class BeakerSequenceGameOverUI : MonoBehaviour
{
    [Tooltip("Texto/painel principal de Game Over (ou qualquer filho do Canvas de Game Over).")]
    [SerializeField]
    private GameObject messageObject;

    [Tooltip("Fundo escuro para bloquear a visão. Opcional.")]
    [SerializeField]
    private GameObject backdropObject;

    [Tooltip("Botão para reiniciar a cena atual.")]
    [SerializeField]
    private Button restartButton;

    [SerializeField]
    private LocomotionMediator locomotionSystem;

    [SerializeField]
    private XRRayInteractor leftRay;

    [SerializeField]
    private XRRayInteractor rightRay;

    [Tooltip("Se falso, mantém raios ativos para clicar no botão em VR.")]
    [SerializeField]
    private bool disableControllerRaysWhenPaused = false;

    [Header("Áudio de Game Over (opcional)")]
    [SerializeField]
    private AudioSource gameOverAudioSource;

    [SerializeField]
    private AudioClip gameOverClip;

    private bool _shown;

    private void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartCurrentScene);
    }

    private void OnEnable()
    {
        GamePuzzleEvents.BeakerPourSequenceFailed += ShowGameOver;
    }

    private void OnDisable()
    {
        GamePuzzleEvents.BeakerPourSequenceFailed -= ShowGameOver;
    }

    private void OnDestroy()
    {
        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartCurrentScene);
    }

    private void ShowGameOver()
    {
        if (_shown)
            return;
        _shown = true;

        var pourAudio = PourAudioCoordinator.Instance;
        if (pourAudio != null)
            pourAudio.ForceStopAllPouring();

        var canvas = FindCanvas(messageObject) ?? FindCanvas(backdropObject);
        if (canvas != null)
            canvas.gameObject.SetActive(true);
        if (backdropObject != null)
            backdropObject.SetActive(true);
        if (messageObject != null)
            messageObject.SetActive(true);

        if (gameOverAudioSource != null && gameOverClip != null)
        {
            gameOverAudioSource.ignoreListenerPause = true;
            gameOverAudioSource.PlayOneShot(gameOverClip);
        }

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

    public void RestartCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static Canvas FindCanvas(GameObject go)
    {
        return go == null ? null : go.GetComponentInParent<Canvas>(true);
    }
}
