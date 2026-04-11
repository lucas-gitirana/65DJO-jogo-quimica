using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uma única fonte de áudio para o som de derramamento: evita sobreposição quando vários tubos derramam.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PourAudioCoordinator : MonoBehaviour
{
    public static PourAudioCoordinator Instance { get; private set; }

    private AudioSource _source;
    private readonly HashSet<int> _activePourers = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        Instance = this;
        _source = GetComponent<AudioSource>();
        _source.loop = true;
        _source.playOnAwake = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <param name="instanceId">ex.: GetInstanceID() do tubo que está derramando.</param>
    public void SetPouringActive(int instanceId, bool active)
    {
        if (instanceId == 0)
            return;

        if (active)
            _activePourers.Add(instanceId);
        else
            _activePourers.Remove(instanceId);

        if (_activePourers.Count > 0)
        {
            if (!_source.isPlaying)
                _source.Play();
        }
        else
            _source.Stop();
    }
}
