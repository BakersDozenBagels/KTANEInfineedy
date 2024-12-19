using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class ButtonScript : MonoBehaviour
{
    [SerializeField]
    private Transform _button;

    private Coroutine _animation;

    private KMSelectable _sel;
    private KMAudio _audio;
    private InfinityScript _infinity;

    private Vector3 _origPos;
    private const float Delta = -0.01f;
    private const float Acceleration = 0.75f;

    public event Action OnPress = () => { };
    public event Action OnRelease = () => { };

    private void Start()
    {
        _origPos = _button.localPosition;
        _audio = GetComponentInParent<KMAudio>();
        _sel = GetComponent<KMSelectable>();
        _infinity = GetComponentInParent<InfineedyScript>().GetComponentInChildren<InfinityScript>();
        _sel.OnInteract += () =>
        {
            _sel.AddInteractionPunch(0.2f);
            _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
            Animate(To(Delta));
            _infinity.LookAt(new Vector2(transform.localPosition.x, transform.localPosition.z).normalized * 2f);
            OnPress();
            return false;
        };
        _sel.OnInteractEnded += () =>
        {
            _sel.AddInteractionPunch(0.1f);
            _audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, transform);
            Animate(To(0));
            _infinity.LookAt(new Vector2(transform.localPosition.x, transform.localPosition.z).normalized);
            OnRelease();
        };
        _sel.OnHighlight += () => _infinity.LookAt(new Vector2(transform.localPosition.x, transform.localPosition.z).normalized);
        _sel.OnHighlightEnded += () => _infinity.Release();
    }

    private void Animate(IEnumerator animation)
    {
        if (_animation != null)
            StopCoroutine(_animation);
        _animation = StartCoroutine(animation);
    }

    private IEnumerator To(float offset)
    {
        Vector3 start = _button.localPosition;
        Vector3 end = new Vector3(_origPos.x, _origPos.y + offset, _origPos.z);
        float halfDuration = Mathf.Sqrt(Mathf.Abs(start.y - end.y) / (2 * Acceleration));
        float midTime = Time.time + halfDuration;
        float endTime = midTime + halfDuration;
        while (Time.time < midTime)
        {
            float t = midTime - Time.time;
            t /= halfDuration;
            t = 1 - t;
            t *= t;
            t /= 2;
            _button.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }
        while (Time.time < endTime)
        {
            float t = endTime - Time.time;
            t /= halfDuration;
            t *= t;
            t /= -2;
            t++;
            _button.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }
        _button.localPosition = end;
    }
}
