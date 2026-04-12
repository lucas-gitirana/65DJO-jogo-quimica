using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// RN04/RN05: bloqueia a porta até o puzzle resolver; ao resolver ativa outline e som de vitória;
/// ao abrir a porta remove o outline (giratória: HingeJoint; de correr: ConfigurableJoint, como EventosPortaCorrer).
/// Porta de correr: desativa o grab e trava o eixo linear do joint até o puzzle ser resolvido.
/// </summary>
[DisallowMultipleComponent]
public class DoorPuzzleController : MonoBehaviour
{
    private enum SlideAxis
    {
        X,
        Y,
        Z
    }

    [SerializeField]
    private HingeJoint hinge;

    [Tooltip("Porta de correr: deixe HingeJoint vazio e arraste o ConfigurableJoint da folha móvel.")]
    [SerializeField]
    private ConfigurableJoint slidingJoint;

    [Tooltip("Qual eixo linear do ConfigurableJoint está livre para o deslize (o que estava Limited ou Free).")]
    [SerializeField]
    private SlideAxis slidingFreeAxis = SlideAxis.X;

    [Tooltip("Abertura linear (|deslocamento no eixo X do joint|) para considerar aberta — alinhar com EventosPortaCorrer.")]
    [SerializeField]
    private float slidingOpenThreshold = 0.6f;

    [Tooltip("Grab da folha móvel. Se vazio, procura no mesmo objeto do Sliding Joint / neste objeto.")]
    [SerializeField]
    private XRGrabInteractable slidingDoorGrab;

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
    private bool _hingeUnlockApplied;
    private bool _slidingUnlockApplied;
    private bool _wasOpen;

    private ConfigurableJointMotion _savedSlideAxisMotion;
    private bool _hadSlidingJointCache;
    private bool _grabCached;
    private bool _grabWasEnabledBeforeLock;

    private void Awake()
    {
        if (hinge == null)
            hinge = GetComponent<HingeJoint>();
        if (slidingJoint == null)
            slidingJoint = GetComponent<ConfigurableJoint>();

        if (slidingDoorGrab == null && slidingJoint != null)
        {
            slidingDoorGrab = slidingJoint.GetComponent<XRGrabInteractable>();
            if (slidingDoorGrab == null)
                slidingDoorGrab = slidingJoint.GetComponentInChildren<XRGrabInteractable>();
        }

        if (slidingDoorGrab == null)
            TryGetComponent(out slidingDoorGrab);
    }

    private void Start()
    {
        if (hinge != null)
        {
            _savedLimits = hinge.limits;
            _limitsCached = true;
            LockDoorAtCurrentAngle();
        }

        if (slidingJoint != null && hinge == null)
            LockSlidingDoor();

        if (doorOutline != null)
            doorOutline.OutlineWidth = 0f;
    }

    private void Update()
    {
        if (!_puzzleSolved)
            return;

        bool open = IsDoorPhysicallyOpen();
        if (open && !_wasOpen && doorOutline != null)
            doorOutline.OutlineWidth = 0f;
        _wasOpen = open;
    }

    private bool IsDoorPhysicallyOpen()
    {
        if (hinge != null)
            return hinge.angle <= openAngleThreshold;

        if (slidingJoint != null)
            return Mathf.Abs(GetJointLinearX(slidingJoint)) >= slidingOpenThreshold;

        return false;
    }

    private static float GetJointLinearX(ConfigurableJoint joint)
    {
        if (joint == null)
            return 0f;

        Vector3 worldAnchor = joint.transform.TransformPoint(joint.anchor);
        Vector3 connectedAnchor = joint.connectedAnchor;
        Vector3 delta = worldAnchor - connectedAnchor;
        Vector3 axisX = joint.transform.TransformDirection(Vector3.right);
        return Vector3.Dot(delta, axisX);
    }

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

    private void LockSlidingDoor()
    {
        _hadSlidingJointCache = true;
        _savedSlideAxisMotion = GetSlideAxisMotion(slidingJoint, slidingFreeAxis);
        SetSlideAxisMotion(slidingJoint, slidingFreeAxis, ConfigurableJointMotion.Locked);

        if (slidingDoorGrab != null)
        {
            _grabCached = true;
            _grabWasEnabledBeforeLock = slidingDoorGrab.enabled;
            slidingDoorGrab.enabled = false;
        }
    }

    private void UnlockDoorPhysics()
    {
        if (hinge != null && _limitsCached && !_hingeUnlockApplied)
        {
            hinge.limits = _savedLimits;
            hinge.useLimits = true;
            _hingeUnlockApplied = true;
        }

        if (slidingJoint != null && _hadSlidingJointCache && !_slidingUnlockApplied)
        {
            SetSlideAxisMotion(slidingJoint, slidingFreeAxis, _savedSlideAxisMotion);
            _slidingUnlockApplied = true;
        }

        if (_grabCached && slidingDoorGrab != null)
            slidingDoorGrab.enabled = _grabWasEnabledBeforeLock;
    }

    private static ConfigurableJointMotion GetSlideAxisMotion(ConfigurableJoint joint, SlideAxis axis)
    {
        return axis switch
        {
            SlideAxis.X => joint.xMotion,
            SlideAxis.Y => joint.yMotion,
            SlideAxis.Z => joint.zMotion,
            _ => joint.xMotion
        };
    }

    private static void SetSlideAxisMotion(ConfigurableJoint joint, SlideAxis axis, ConfigurableJointMotion motion)
    {
        switch (axis)
        {
            case SlideAxis.X:
                joint.xMotion = motion;
                break;
            case SlideAxis.Y:
                joint.yMotion = motion;
                break;
            case SlideAxis.Z:
                joint.zMotion = motion;
                break;
        }
    }
}
