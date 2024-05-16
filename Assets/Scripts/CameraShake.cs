using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    private Transform _camTransform;
    private float _shakeDur = 1f;
    private const float SHAKE_AMOUNT = .1f, DECREASE_FACTOR = 1.5f;
    private Vector3 _originPosition;

    private void Start()
    {
        _camTransform = GetComponent<Transform>();
        _originPosition = _camTransform.localPosition;
    }

    private void Update()
    {
        if (_shakeDur > 0)
        {
            _camTransform.localPosition = _originPosition + Random.insideUnitSphere * SHAKE_AMOUNT;
            _shakeDur -= Time.deltaTime * DECREASE_FACTOR;
        }
        else
        {
            _shakeDur = 0;
            _camTransform.localPosition = _originPosition;
        }
    }
}
