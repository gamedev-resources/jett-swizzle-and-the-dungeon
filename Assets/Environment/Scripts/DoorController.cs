using System.Collections;
using Dungeon.UI;
using UnityEngine;

namespace Dungeon.Environment
{
    public class DoorController : MonoBehaviour
    {
        [Tooltip("Set to false to lock this door (e.g. requires a key).")]
        public bool canBeOpened = true;

        [Header("References")]
        [SerializeField] private Transform doorPanel;

        [Header("Settings")]
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private float _targetAngle;
        private float _currentAngle;
        private Coroutine _swingRoutine;

        private void DisplayUI()
        {
            if (InteractionPromptUI.Instance != null && !canBeOpened)
            {
                InteractionPromptUI.Instance.ShowAt(transform.position,"Door is locked");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }
            
            DisplayUI();

            if (!canBeOpened)
            {
                return;
            }

            // Dot product: positive = player in front, negative = player behind
            Vector3 toPlayer = other.transform.position - transform.position;
            float dot = Vector3.Dot(transform.forward, toPlayer.normalized);

            // Swing away from the player
            _targetAngle = dot >= 0f ? -openAngle : openAngle;

            Swing(_targetAngle, openDuration);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (InteractionPromptUI.Instance != null)
            {
                InteractionPromptUI.Instance.Hide();
            }

            Swing(0f, closeDuration);
        }

        private void Swing(float target, float duration)
        {
            if (_swingRoutine != null)
            {
                StopCoroutine(_swingRoutine);
            }

            _swingRoutine = StartCoroutine(SwingRoutine(target, duration));
        }

        private IEnumerator SwingRoutine(float target, float duration)
        {
            float start = _currentAngle;
            float elapsed = 0f;

            while (elapsed < duration)
            {

                elapsed += Time.deltaTime;
                float t = openCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                _currentAngle = Mathf.LerpAngle(start, target, t);
                doorPanel.localEulerAngles = new Vector3(0f, _currentAngle, 0f);

                Debug.Log($"Target: {target} | CurrentAngle: {_currentAngle}");


                yield return null;
            }

            Debug.Log($"[FINAL] Target: {target} | CurrentAngle: {_currentAngle}");

            _currentAngle = target;
            doorPanel.localEulerAngles = new Vector3(0f, target, 0f);
            _swingRoutine = null;
        }
    }
}
