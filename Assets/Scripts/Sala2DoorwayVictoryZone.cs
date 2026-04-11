using UnityEngine;

/// <summary>
/// Coloque na soleira (ou logo após) a porta de correr da sala 2: ao jogador atravessar, dispara a vitória
/// (<see cref="GamePuzzleEvents.RaiseGameVictory"/>), que o <see cref="GameVictoryPresenter"/> transforma em animação/som/UI.
/// Requer um Collider com <b>Is Trigger</b> no mesmo objeto ou num filho.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Sala2DoorwayVictoryZone : MonoBehaviour
{
    [SerializeField]
    private bool onlyOnce = true;

    private bool _fired;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c != null)
            c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && _fired)
            return;
        if (!IsPlayerCollider(other))
            return;

        _fired = true;
        GamePuzzleEvents.RaiseGameVictory();
    }

    private static bool IsPlayerCollider(Collider other)
    {
        if (other.CompareTag("Player"))
            return true;
        if (other.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>() != null)
            return true;
        return other.GetComponentInParent<CharacterController>() != null;
    }
}
