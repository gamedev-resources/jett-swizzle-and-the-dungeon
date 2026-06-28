using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField] private Light _torchLight;
    [SerializeField] private float _baseIntensity = 4f;
    [SerializeField] private float _intensityAmplitude = 0.25f;
    [SerializeField] private float _flickerSpeed = 2.0f;
    [SerializeField] private float _rangeAmplitude = 0.08f;
    private float _seed;

    private void Awake()
    {
        _seed = Random.value * 100f; // unique per torch
        if (_torchLight == null)
        {
            _torchLight = GetComponent<Light>();
        }
    }

    private void Update()
    {
        float t = Time.time * _flickerSpeed + _seed;
        float n = Mathf.PerlinNoise(t, 0f); // 0..1
        float centered = (n - 0.5f) * 2f;   // -1..1

        _torchLight.intensity = _baseIntensity + centered * _intensityAmplitude;
        _torchLight.range = 8f + centered * _rangeAmplitude;
    }
}