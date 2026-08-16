using Dungeon.Core.Events;
using Dungeon.Core.Events.Combat;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Detects when this projectile enters a collider on a target layer, fires an effect, then disables or destroys itself.</summary>
public class ProjectileHit : MonoBehaviour
{
    [Tooltip("Layers that count as a valid target. Set this to the Player layer.")]
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("Effect to run on a valid hit (damage, FX, etc.). Wire it in the Inspector.")]
    [SerializeField] private UnityEvent onHit;

    [Tooltip("On hit, disable the projectile (so a spawner can re-enable it) instead of destroying it.")]
    [SerializeField] private bool destroyOnHit = true;

    private void OnTriggerEnter(Collider other)
    {
        // Ignore anything that isn't on a target layer.
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        onHit?.Invoke();

        GameplayEventBus.Raise(new AttackHitEvent(transform, other.transform));

        if (destroyOnHit)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }

    public void Disable() => gameObject.SetActive(false);
}
