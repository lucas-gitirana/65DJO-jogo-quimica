using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// Feedback de vitória final: Animator (vários), Timeline opcional, UnityEvent, overlay em CanvasGroup.
/// Opcionalmente encerra o jogo ou carrega outra cena após um tempo (tempo real), para a mensagem ser vista primeiro.
/// </summary>
public class GameVictoryPresenter : MonoBehaviour
{
    private enum VictoryFinishAction
    {
        None = 0,
        QuitApplication = 1,
        LoadScene = 2
    }

    [Header("Animator")]
    [SerializeField]
    private Animator animator;

    [Tooltip("Se o painel de vitória começa desativado, arrasta-o aqui: fica ativo antes do trigger (Animator precisa de estar enabled).")]
    [SerializeField]
    private GameObject victoryPanelRoot;

    [Tooltip("Outros animators que recebem o mesmo trigger (UI mundo, objeto decorativo, etc.).")]
    [SerializeField]
    private Animator[] additionalAnimators;

    [SerializeField]
    private string victoryTriggerParameter = "Victory";

    [Header("Timeline / eventos")]
    [SerializeField]
    private PlayableDirector victoryTimeline;

    [Tooltip("Chamado uma vez na vitória: partículas, áudio extra, luzes, etc.")]
    [SerializeField]
    private UnityEvent onVictory;

    [Header("Overlay (opcional — sem Animator Controller)")]
    [SerializeField]
    private CanvasGroup victoryOverlay;

    [SerializeField]
    private float overlayFadeDuration = 1.25f;

    [SerializeField]
    private bool useUnscaledTimeForOverlay = true;

    [Header("Áudio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip victoryClip;

    [SerializeField]
    private bool playOnlyOnce = true;

    [Header("Fim do jogo (após a mensagem)")]
    [Tooltip("Nada = só mostra vitória. Quit = fecha o exe. Load Scene = carrega outro nível (ex.: menu).")]
    [SerializeField]
    private VictoryFinishAction finishAfterVictory = VictoryFinishAction.None;

    [Tooltip("Segundos em tempo real a esperar depois de disparar a vitória (animação + leitura).")]
    [SerializeField]
    private float secondsBeforeFinish = 5f;

    [Tooltip("Nome da cena no Build Settings (só se Finish = Load Scene).")]
    [SerializeField]
    private string sceneToLoadAfterVictory = "MainMenu";

    private bool _played;

    private void OnEnable()
    {
        GamePuzzleEvents.GameVictoryRequested += PlayVictory;
    }

    private void OnDisable()
    {
        GamePuzzleEvents.GameVictoryRequested -= PlayVictory;
    }

    public void PlayVictory()
    {
        if (playOnlyOnce && _played)
            return;
        _played = true;

        if (victoryPanelRoot != null)
            victoryPanelRoot.SetActive(true);

        FireAnimators();

        if (victoryTimeline != null)
            victoryTimeline.Play();

        onVictory?.Invoke();

        if (victoryOverlay != null)
        {
            victoryOverlay.gameObject.SetActive(true);
            victoryOverlay.alpha = 0f;
            StartCoroutine(FadeOverlayIn());
        }

        if (audioSource != null && victoryClip != null)
            audioSource.PlayOneShot(victoryClip);

        if (finishAfterVictory != VictoryFinishAction.None && secondsBeforeFinish >= 0f)
            VictoryDelayedFinisher.Run(secondsBeforeFinish, ExecuteFinishAction);
    }

    private void ExecuteFinishAction()
    {
        Debug.Log("FINALIZANDO JOGO");
        switch (finishAfterVictory)
        {
            case VictoryFinishAction.QuitApplication:
                QuitApplication();
                break;
            case VictoryFinishAction.LoadScene:
                if (!string.IsNullOrEmpty(sceneToLoadAfterVictory))
                    SceneManager.LoadScene(sceneToLoadAfterVictory);
                break;
        }
    }

private static void QuitApplication()
{
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
}

    private void FireAnimators()
    {
        if (!string.IsNullOrEmpty(victoryTriggerParameter))
        {
            if (animator != null)
                animator.SetTrigger(victoryTriggerParameter);

            if (additionalAnimators != null)
            {
                foreach (var a in additionalAnimators)
                {
                    if (a != null)
                        a.SetTrigger(victoryTriggerParameter);
                }
            }
        }
    }

    private IEnumerator FadeOverlayIn()
    {
        float d = Mathf.Max(0.01f, overlayFadeDuration);
        float t = 0f;
        while (t < d)
        {
            t += useUnscaledTimeForOverlay ? Time.unscaledDeltaTime : Time.deltaTime;
            victoryOverlay.alpha = Mathf.Clamp01(t / d);
            yield return null;
        }

        victoryOverlay.alpha = 1f;
    }
}

/// <summary>
/// Objeto temporário com DontDestroyOnLoad para o atraso sobreviver a qualquer descarga de cena.
/// </summary>
internal sealed class VictoryDelayedFinisher : MonoBehaviour
{
    public static void Run(float delaySeconds, Action onComplete)
    {
        var go = new GameObject("~VictoryDelayedFinisher");
        DontDestroyOnLoad(go);
        var runner = go.AddComponent<VictoryDelayedFinisher>();
        runner.StartCoroutine(runner.WaitAndRun(delaySeconds, onComplete));
    }

    private IEnumerator WaitAndRun(float delaySeconds, Action onComplete)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delaySeconds));
        try
        {
            onComplete?.Invoke();
        }
        finally
        {
            Destroy(gameObject);
        }
    }
}
