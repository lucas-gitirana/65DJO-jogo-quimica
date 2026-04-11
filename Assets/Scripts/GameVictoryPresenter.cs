using UnityEngine;

/// <summary>
/// Feedback de vitória final: áudio, animação (Animator), opcionalmente desativa interação.
/// </summary>
public class GameVictoryPresenter : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private string victoryTriggerParameter = "Victory";

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip victoryClip;

    [SerializeField]
    private bool playOnlyOnce = true;

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

        if (animator != null && !string.IsNullOrEmpty(victoryTriggerParameter))
            animator.SetTrigger(victoryTriggerParameter);

        if (audioSource != null && victoryClip != null)
            audioSource.PlayOneShot(victoryClip);
    }
}
