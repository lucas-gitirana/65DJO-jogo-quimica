using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// RN04/RN05: bloqueia a porta até o puzzle resolver; ao resolver ativa outline e som de vitória;
/// ao abrir a porta (ângulo do hinge), remove o outline.
/// </summary>
[DisallowMultipleComponent]
public class DoorPuzzleController : MonoBehaviour
{
    [SerializeField]
    private HingeJoint hinge;

    [SerializeField]
    private Outline doorOutline;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip puzzleSolvedClip;

    [Tooltip("Mesmo critério usado em EventosPorta: porta considerada aberta.")]
    [SerializeField]
    private float openAngleThreshold = -40f;

    [SerializeField]
    private float outlineWidthWhenHint = 3f;

    [SerializeField]
    private UnityEvent onDoorUnlocked;

    private JointLimits _savedLimits;
    private bool _limitsCached;
    private bool _puzzleSolved;
    private bool _physicalUnlockApplied;
    private bool _wasOpen;

    private void Awake()
    {
        if (hinge == null)
            hinge = GetComponent<HingeJoint>();
    }

    private void Start()
    {
        if (hinge != null)
        {
            _savedLimits = hinge.limits;
            _limitsCached = true;
            LockDoorAtCurrentAngle();
        }

        if (doorOutline != null)
            doorOutline.OutlineWidth = 0f;
    }

    private void Update()
    {
        if (hinge == null || !_puzzleSolved)
            return;

        bool open = hinge.angle <= openAngleThreshold;
        if (open && !_wasOpen && doorOutline != null)
            doorOutline.OutlineWidth = 0f;
        _wasOpen = open;
    }

    /// <summary>Chame de um UnityEvent ligado ao puzzle (ChemistryBeakerPuzzle / Room2MiddleButtonPuzzle).</summary>
    public void NotifyPuzzleSolved()
    {
        if (_puzzleSolved)
            return;
        _puzzleSolved = true;

        UnlockDoorPhysics();
        onDoorUnlocked?.Invoke();

        if (doorOutline != null)
            doorOutline.OutlineWidth = outlineWidthWhenHint;

        if (audioSource != null && puzzleSolvedClip != null)
            audioSource.PlayOneShot(puzzleSolvedClip);
    }

    private void LockDoorAtCurrentAngle()
    {
        if (hinge == null || !_limitsCached)
            return;

        float a = hinge.angle;
        var l = hinge.limits;
        int rounded = Mathf.RoundToInt(a);
        l.min = rounded;
        l.max = rounded;
        hinge.limits = l;
        hinge.useLimits = true;
    }

    private void UnlockDoorPhysics()
    {
        if (_physicalUnlockApplied || hinge == null || !_limitsCached)
            return;
        hinge.limits = _savedLimits;
        hinge.useLimits = true;
        _physicalUnlockApplied = true;
    }
}
