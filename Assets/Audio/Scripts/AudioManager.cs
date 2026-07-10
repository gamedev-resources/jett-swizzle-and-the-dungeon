using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public enum SoundId
    {
        KeyPickup,
        DoorOpen,
        DoorClose,
        Footstep
    }

    [Header("Clips")]
    [SerializeField] private AudioClip _keyPickupClip;
    [SerializeField] private AudioClip _doorOpenClip;
    [SerializeField] private AudioClip _doorCloseClip;
    [Header("Player Footsteps")]
    [SerializeField] private AudioClip[] _footstepClips;
    [SerializeField, Range(0f, 1f)] private float _footstepVolume = 0.4f;

    [Header("Spacial Awareness")]
    [Tooltip("0 = full 2D (same volume everywhere), 1 = full 3D (position and distance affect volume).")]
    [SerializeField, Range(0f, 1f)] private float _spatialBlend = 0.8f;
    [Tooltip("Distance at which the sound is at full volume. Closer than this does not get louder.")]
    [SerializeField] private float _minDistance = 1f;
    [Tooltip("Distance at which the sound becomes nearly inaudible with Logarithmic rolloff.")]
    [SerializeField] private float _maxDistance = 20f;

    //Will convert this to an addressable system in a later tutorial
    private Dictionary<SoundId, AudioClip> _soundMap;
    private int _lastFootstepIndex = -1;
    private List<AudioSource> _pool = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of AudioManager found. Destroying.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _soundMap = new Dictionary<SoundId, AudioClip>
        {
            { SoundId.KeyPickup, _keyPickupClip },
            { SoundId.DoorOpen, _doorOpenClip },
            { SoundId.DoorClose, _doorCloseClip },
        };
    }

    /// <summary>
    /// Plays a one-shot sound at the given world position from the audio pool.
    /// </summary>
    public void Play(SoundId id, Vector3 position)
    {
        if (id == SoundId.Footstep)
        {
            PlayFootstep(position);
            return;
        }

        if (_soundMap.TryGetValue(id, out AudioClip clip) && clip != null)
        {
            PlayOneShot(clip, position);
        }
    }

    /// <summary>
    /// Picks a random footstep clip from the array, avoiding consecutive repeats.
    /// </summary>
    private void PlayFootstep(Vector3 position)
    {
        if (_footstepClips == null || _footstepClips.Length == 0)
            return;

        int index;
        if (_footstepClips.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, _footstepClips.Length);
            } while (index == _lastFootstepIndex);
            _lastFootstepIndex = index;
        }

        PlayOneShot(_footstepClips[index], position, _footstepVolume);
    }

    /// <summary>
    /// Rents a pooled AudioSource, positions it, plays the clip, then returns it after the clip finishes.
    /// </summary>
    private void PlayOneShot(AudioClip clip, Vector3 position, float volumeScale = 0.5f)
    {
        AudioSource source = Rent();
        source.transform.position = position;
        source.gameObject.SetActive(true);
        source.PlayOneShot(clip, volumeScale);
        StartCoroutine(ReturnAfterDelay(source, clip.length));
    }

    /// <summary>
    /// Returns the first inactive AudioSource from the pool, or creates one if none are available.
    /// </summary>
    private AudioSource Rent()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (!_pool[i].gameObject.activeInHierarchy)
                return _pool[i];
        }

        GameObject go = new GameObject("Pooled Audio Source");
        go.transform.SetParent(transform);

        AudioSource source = go.AddComponent<AudioSource>();
        source.spatialBlend = _spatialBlend;
        source.minDistance = _minDistance;
        source.maxDistance = _maxDistance;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        go.SetActive(false);

        _pool.Add(source);

        return source;
    }

    /// <summary>
    /// Waits for the clip duration, then deactivates the AudioSource so it can be re-used by Rent().
    /// </summary>
    private IEnumerator ReturnAfterDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
            source.gameObject.SetActive(false);
    }
}
