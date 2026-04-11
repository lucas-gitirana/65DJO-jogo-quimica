using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

/// <summary>
/// RN06: dispara o fim do jogo ao sair do laboratório (trigger) ou ao usar um TeleportationArea de saída.
/// </summary>
[DisallowMultipleComponent]
public class LabExitVictoryTrigger : MonoBehaviour
{
    [SerializeField]
    private bool useTriggerCollider = true;

    [SerializeField]
    private TeleportationArea exitTeleportArea;

    private bool _fired;

    private void OnEnable()
    {
        if (exitTeleportArea != null)
            exitTeleportArea.selectEntered.AddListener(OnTeleportExit);
    }

    private void OnDisable()
    {
        if (exitTeleportArea != null)
            exitTeleportArea.selectEntered.RemoveListener(OnTeleportExit);
    }

    private void OnTeleportExit(SelectEnterEventArgs _)
    {
        TriggerVictory();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerCollider)
            return;
        if (!IsPlayerCollider(other))
            return;
        TriggerVictory();
    }

    private static bool IsPlayerCollider(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;
        if (other.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>() != null)
            return true;
        return other.GetComponentInParent<CharacterController>() != null;
    }

    private void TriggerVictory()
    {
        if (_fired)
            return;
        _fired = true;
        GamePuzzleEvents.RaiseGameVictory();
    }
}
