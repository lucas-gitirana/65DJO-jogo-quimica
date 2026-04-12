using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RN05 – o jogador deve acionar o botão do meio (numBotao == 2) três vezes.
/// </summary>
public class Room2MiddleButtonPuzzle : MonoBehaviour
{
    [SerializeField]
    private int requiredMiddlePresses = 3;

    [SerializeField]
    private UnityEvent onSolved;

    [Header("Som de vitória (opcional)")]
    [SerializeField]
    private AudioSource celebrationAudio;

    [SerializeField]
    private AudioClip celebrationClip;

    public bool IsSolved { get; private set; }

    private int _count;

    private void Awake()
    {
        if (celebrationAudio == null)
            TryGetComponent(out celebrationAudio);
    }

    private void OnEnable()
    {
        GamePuzzleEvents.LabButtonPressed += OnButton;
    }

    private void OnDisable()
    {
        GamePuzzleEvents.LabButtonPressed -= OnButton;
    }

    private void OnButton(int numBotao)
    {
        if (IsSolved || numBotao != 2)
            return;

        _count++;
        if (_count < requiredMiddlePresses)
            return;

        IsSolved = true;

        if (celebrationClip != null && celebrationAudio != null)
            celebrationAudio.PlayOneShot(celebrationClip);

        onSolved?.Invoke();
    }
}
