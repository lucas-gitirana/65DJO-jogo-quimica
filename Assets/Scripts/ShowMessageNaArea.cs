using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

/// <summary>
/// Mostra UI ao jogador quando um raio a partir da câmara atinge este objeto (ex.: pisar sobre Chao3).
/// Opcional: painel escuro (fundo) e botão Sair. Em VR, para clicar no botão com o raio, normalmente não deves
/// desativar os <see cref="XRRayInteractor"/> (usa <see cref="disableControllerRaysWhenPaused"/> = false).
/// </summary>
public class ShowMessageNaArea : MonoBehaviour
{
    [Tooltip("Use a transform da Main Camera sob o XR Origin (não o Camera Offset). Se vazio, tenta encontrar no XROrigin.")]
    [SerializeField]
    private Transform playerCamera;

    [Tooltip("Canvas raiz (recomendado) ou qualquer filho (ex. Text TMP). Se o Canvas estiver desativado como no tutorial, o script ativa o Canvas ancestral antes.")]
    [SerializeField]
    private GameObject messageObject;

    [Tooltip("Painel escuro (ex.: Image em stretch sobre o ecrã) por baixo do texto. Opcional; ativa o Canvas ancestral se necessário.")]
    [SerializeField]
    private GameObject backdropObject;

    [Tooltip("Botão Sair. Liga no OnClick em Awake. Em VR, mantém os raios ativos (desmarca a opção abaixo) ou usa Escape no teclado (PC).")]
    [SerializeField]
    private Button quitButton;

    [Header("Áudio de vitória")]
    [Tooltip("Fonte dedicada (não uses a do PourAudioCoordinator / som de derramar).")]
    [SerializeField]
    private AudioSource victoryAudioSource;

    [SerializeField]
    private AudioClip victoryClip;

    [SerializeField]
    private LocomotionMediator locomotionSystem;

    [SerializeField]
    private XRRayInteractor leftRay;

    [SerializeField]
    private XRRayInteractor rightRay;

    [Tooltip("Se falso, os raios dos controladores ficam ativos durante a pausa — necessário para apontar ao botão em VR.")]
    [SerializeField]
    private bool disableControllerRaysWhenPaused = true;

    [Tooltip("No PC, Escape chama Sair após a mensagem (útil se os raios estiverem desligados).")]
    [SerializeField]
    private bool quitOnEscapeAfterTrigger = true;

    [SerializeField]
    private float maxDistance = 5f;

    [SerializeField]
    private LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    private bool _triggered;

    private void Awake()
    {
        if (playerCamera == null)
        {
            var origin = FindFirstObjectByType<XROrigin>();
            if (origin != null && origin.Camera != null)
                playerCamera = origin.Camera.transform;
        }

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitApplication);
    }

    private void OnDestroy()
    {
        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitApplication);
    }

    private void Update()
    {
        if (_triggered)
        {
            if (quitOnEscapeAfterTrigger && Input.GetKeyDown(KeyCode.Escape))
                QuitApplication();
            return;
        }

        if (playerCamera == null)
            return;

        var ray = new Ray(playerCamera.position, Vector3.down);
        if (!Physics.Raycast(ray, out var hit, maxDistance, raycastLayers, QueryTriggerInteraction.Collide))
            return;

        if (!IsOwnZoneCollider(hit.collider, transform))
            return;

        ActivateVictoryUi();
        PlayVictoryMusic();

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

        _triggered = true;
    }

    /// <summary>Encerra a aplicação (ou sai do Play Mode no Editor).</summary>
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static bool IsOwnZoneCollider(Collider c, Transform zoneRoot)
    {
        return c != null && (c.transform == zoneRoot || c.transform.IsChildOf(zoneRoot));
    }

    private void ActivateVictoryUi()
    {
        var canvas = FindCanvas(messageObject) ?? FindCanvas(backdropObject);
        if (canvas != null)
            canvas.gameObject.SetActive(true);

        if (backdropObject != null)
            backdropObject.SetActive(true);
        if (messageObject != null)
            messageObject.SetActive(true);
    }

    private void PlayVictoryMusic()
    {
        if (victoryClip == null || victoryAudioSource == null)
            return;

        // Com Time.timeScale = 0, alguns setups pausam o listener; isto mantém a vitória audível.
        victoryAudioSource.ignoreListenerPause = true;
        victoryAudioSource.PlayOneShot(victoryClip);
    }

    private static Canvas FindCanvas(GameObject go)
    {
        return go == null ? null : go.GetComponentInParent<Canvas>(true);
    }
}
