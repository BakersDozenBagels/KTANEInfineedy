using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InfinityScript : MonoBehaviour
{
    private Material _mat;
    private float _angle, _snap = 14f;

    private Coroutine _snapAnimation, _rotationAnimation;
    private Quaternion _rotation;

    private const float _speed = 1f;
    private const float Acceleration = 10f;

    public void LookAt(Vector2 pos)
    {
        if (_snapAnimation != null)
            StopCoroutine(_snapAnimation);
        if (_rotationAnimation != null)
            StopCoroutine(_rotationAnimation);
        _snapAnimation = StartCoroutine(AnimateSnap(2));
        _rotationAnimation = StartCoroutine(AnimateRotation(-pos.x, -pos.y));
    }

    public void Release()
    {
        if (_snapAnimation != null)
            StopCoroutine(_snapAnimation);
        if (_rotationAnimation != null)
            StopCoroutine(_rotationAnimation);
        _snapAnimation = StartCoroutine(AnimateSnap(14));
        _rotationAnimation = StartCoroutine(AnimateRotation(0, 0));
    }

    private IEnumerator AnimateRotation(float x, float y)
    {
        var start = _rotation;
        var end = Quaternion.LookRotation(new Vector3(x, y, 4f), Vector3.up);
        float halfDuration = 0.075f;
        float midTime = Time.time + halfDuration;
        float endTime = midTime + halfDuration;
        while (Time.time < midTime)
        {
            float t = midTime - Time.time;
            t /= halfDuration;
            t = 1 - t;
            t *= t;
            t /= 2;
            _rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        while (Time.time < endTime)
        {
            float t = endTime - Time.time;
            t /= halfDuration;
            t *= t;
            t /= -2;
            t++;
            _rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        _rotation = end;
    }

    private IEnumerator AnimateSnap(float endsnap)
    {
        var startsnap = _snap;
        float halfDuration = Mathf.Sqrt(Mathf.Abs(startsnap - endsnap) / (2 * Acceleration));
        float midTime = Time.time + halfDuration;
        float endTime = midTime + halfDuration;
        while (Time.time < midTime)
        {
            float t = midTime - Time.time;
            t /= halfDuration;
            t = 1 - t;
            t *= t;
            t /= 2;
            _snap = Mathf.Lerp(startsnap, endsnap, t);
            yield return null;
        }
        while (Time.time < endTime)
        {
            float t = endTime - Time.time;
            t /= halfDuration;
            t *= t;
            t /= -2;
            t++;
            _snap = Mathf.Lerp(startsnap, endsnap, t);
            yield return null;
        }
        _snap = endsnap;
    }

    private void Start()
    {
        _mat = GetComponent<Renderer>().material;
        _angle = Random.Range(0f, 2 * Mathf.PI);
    }

    private void Update()
    {
        _angle += _speed * Time.deltaTime;
        var mtx = Matrix4x4.LookAt(Vector3.zero, new Vector3(Mathf.Cos(_angle), Mathf.Sin(_angle), Mathf.Tan(Mathf.Deg2Rad * (90 - _snap))), Vector3.up);
        mtx *= Matrix4x4.Rotate(_rotation);
        mtx = Matrix4x4.Translate(new Vector3(0f, 0f, -0.1f)) * mtx * Matrix4x4.Translate(new Vector3(0f, 0f, 0.1f));
        _mat.SetMatrix("_Mtx", mtx.transpose);
    }
}
