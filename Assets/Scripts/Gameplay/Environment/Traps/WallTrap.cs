using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class WallTrap : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private float travelDuration = 1f;
    [SerializeField] private float distance = 10f;
    [SerializeField] private Transform _spawner;

    [SerializeField] private GameObject _arrow;

    private bool _spawnArrow = true;

    void Update()
    {
        if (_spawnArrow)
        {
            Move();
        }
    }

    private async Awaitable Move()
    { 
        float elapsed = Time.deltaTime;
        _spawnArrow = false;
        _arrow.SetActive(true);

        while (_arrow.activeSelf)
        {
            elapsed += Time.deltaTime;





            // Where in the curve to evaluate
            float t = Mathf.Clamp01(
                elapsed / travelDuration);

            // Get the curve value based on % in curve
            float curveValue = speedCurve.Evaluate(t);

            // Move forward based on 
            // curve-driven speed
            _arrow.transform.position += 
                _spawner.transform.forward 
                * curveValue * distance 
                * Time.deltaTime;




            await Awaitable.NextFrameAsync();

            if (t >= 1f)
            {
                ResetArrow();
                await Awaitable.WaitForSecondsAsync(3f);
                _spawnArrow = true;
            }
        }

    }

    private void ResetArrow()
    {
        _arrow.SetActive(false);
        _arrow.transform.position = _spawner.position;
    }
}