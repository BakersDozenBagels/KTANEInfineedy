using System.Collections;
using System.Collections.Generic;
using KModkit;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public partial class InfineedyScript : MonoBehaviour
{
    [SerializeField]
    private ButtonScript[] _buttons;
    [SerializeField]
    private TextMesh _screen;

    private MonoRandom _rng;
    private KMNeedyModule _needy;
    private KMBombInfo _info;
    private static bool _fetching;
    private static List<RawJson.Module> _allModules = new List<RawJson.Module>(2000);
    private bool _active, _stageActive, _finished, _panic, _interacting;
    private string[] _hash;
    private float[] _hashWidths = new float[] { 1f, 1f, 1f, 1f, 1f, 1f };
    private int _currentActivation, _lastActivation, _mashCount;
    private static int _idc;
    private static readonly Dictionary<string, int> _activationCounters = new Dictionary<string, int>();
    private readonly string _loggingTag = "[Infineedy #" + (++_idc) + "] ";
    private Solution _currentSolution;
    private float _holdTime;

    private double NextDouble()
    {
        return _rng.NextDouble();
    }

    private void Log(string s)
    {
        Debug.Log(_loggingTag + s);
    }

    private void Awake()
    {
        _rng = GetComponent<KMRuleSeedable>().GetRNG();
        _info = GetComponent<KMBombInfo>();
        _needy = GetComponent<KMNeedyModule>();
        _needy.OnNeedyDeactivation += Cleanup;
        _needy.OnNeedyActivation += Activate;
        _needy.OnTimerExpired += TimeUp;
        _needy.ResetDelayMin = 45f;
        _needy.ResetDelayMax = 75f;
#if UNITY_EDITOR
        if (!Application.isEditor)
        {
#endif
            var needy = GetComponent("NeedyComponent");
            var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            needy.GetType().GetField("ResetDelayMin", flags).SetValue(needy, 45);
            needy.GetType().GetField("ResetDelayMax", flags).SetValue(needy, 75);
#if UNITY_EDITOR
        }
#endif
        for (int i = 0; i < 10; i++)
        {
            int j = i;
            _buttons[j].OnPress += () => Hold(j);
            _buttons[j].OnRelease += () => Release(j);
        }
        StartCoroutine(AnimateScreen());
        StartCoroutine(Prepare());
    }

    private void Hold(int ix)
    {
        if (!_stageActive) return;
        if (_currentSolution.Label != ix)
        {
            Strike("Wrong button (" + ix + "). Strike.");
            return;
        }
        switch (_currentSolution.Type)
        {
            case SolutionType.Press:
                Pass();
                break;
            case SolutionType.PressAtTime:
                if ((int)_info.GetTime() % 10 != _currentSolution.Param)
                {
                    Strike("Wrong time (" + ((int)_info.GetTime() % 10) + "). Strike.");
                    return;
                }
                Pass();
                break;
            case SolutionType.Hold:
                _interacting = true;
                _holdTime = _info.GetTime();
                break;
            case SolutionType.Mash:
                _holdTime = Time.time;
                if (!_interacting)
                {
                    _interacting = true;
                    _mashCount = 0;
                    StartCoroutine(ListenForMashes());
                }
                _mashCount++;
                break;
        }
    }

    private void Release(int ix)
    {
        if (!_stageActive || !_interacting) return;
        if (_currentSolution.Type == SolutionType.Hold)
        {
            var holdTime = _info.GetTime() - _holdTime;
            var diff = Mathf.Abs(holdTime - _currentSolution.Param);
            if (diff > 0.5f)
                Strike("Wrong hold time (" + holdTime + "s). Strike.");
            else
                Pass();
        }
    }

    private IEnumerator ListenForMashes()
    {
        while (Time.time < _holdTime + 0.6f)
            yield return null;
        if (!_interacting)
            yield break;
        if (_mashCount != _currentSolution.Param)
            Strike("Wrong press count(" + _mashCount + "). Strike.");
        else
            Pass();
    }

    private void Strike(string message)
    {
        Log(message);
        _needy.HandleStrike();
        _needy.HandlePass();
        _stageActive = false;
        _interacting = false;
    }
    private void Pass()
    {
        Log("Correct input.");
        _needy.HandlePass();
        _stageActive = false;
        _interacting = false;
    }

    private void Activate()
    {
        if (_stageActive)
            return;
        if (!_active || _finished)
        {
            _needy.HandlePass();
            return;
        }
        _stageActive = true;
        _currentActivation = ++_activationCounters[_info.GetSerialNumber()];

        var skipped = _currentActivation - _lastActivation - 1;
        if (skipped != 0)
        {
            Log("Skipping " + skipped + " stages that appeared on other modules.");
            for (int i = 0; i < skipped; i++)
                Stage.Random(NextDouble);
        }
        var currentStage = Stage.Random(NextDouble);
        _currentSolution = currentStage.Calculate(_info);
        _lastActivation = _currentActivation;
        Log("Activation " + _currentActivation + ": " + currentStage.ToString());
        Log("Expected solution: " + _currentSolution);
    }
    private void TimeUp()
    {
        if (_active && !_finished && _stageActive)
            Strike("Time's up. Strike.");
        _stageActive = false;
    }

    private void Cleanup()
    {
        _finished = true;
        _stageActive = false;
        _activationCounters[_info.GetSerialNumber()] = 0;
    }

    private IEnumerator Prepare()
    {
        if (_fetching)
        {
            while (_fetching) yield return null;
            FinishPrepare();
            yield break;
        }
        _fetching = true;
        using (var req = UnityWebRequest.Get("https://ktane.timwi.de/json/raw"))
        {
            yield return req.SendWebRequest();
            if (req.isNetworkError || req.isHttpError)
            {
                Log("Network error! The module will not activate.");
                _hash = new string[] { "Error!", "Error!", "Error!", "Try again later.", "Try again later.", "Try again later." };
                _screen.color = Color.red;
                _panic = true;
                yield break;
            }
            var json = JsonConvert.DeserializeObject<RawJson>(req.downloadHandler.text);
            _allModules.Clear();
            for (int i = 0; i < json.KtaneModules.Length; i++)
                if (json.KtaneModules[i].Type == "Regular")
                    _allModules.Add(json.KtaneModules[i]);
        }
        _fetching = false;
        FinishPrepare();
    }

    private void FinishPrepare()
    {
        for (int i = 0; i < _allModules.Count % 25; i++)
            NextDouble();
        _hash = new string[] {
                _allModules[(int)(NextDouble() * _allModules.Count)].Name,
                _allModules[(int)(NextDouble() * _allModules.Count)].Name,
                _allModules[(int)(NextDouble() * _allModules.Count)].Name
            };
        Log("Hash: " + _hash.Join(" - "));
        for (int i = 0; i < 3; i++)
        {
            var width = TextWidth(_hash[i]);
            _hashWidths[i] = width <= 2488f ? 1f : 2488f / width;
        }
        _active = true;
        if (!_activationCounters.ContainsKey(_info.GetSerialNumber()))
            _activationCounters[_info.GetSerialNumber()] = 0;
    }

    private float TextWidth(string s)
    {
        // Hack to make Unity generate the Character Info
        var origText = _screen.text;
        _screen.text = s;
        _screen.text = origText;

        float width = 0;
        CharacterInfo ci;
        for (int i = 0; i < s.Length; i++)
        {
            _screen.font.GetCharacterInfo(s[i], out ci, _screen.font.fontSize);
            width += ci.advance;
        }

        return width;
    }

    private IEnumerator AnimateScreen()
    {
        int index = 0;
        while (!_active && !_panic)
        {
            index++;
            if (index > 3)
                index -= 3;
            _screen.text = "Getting Ready" + new string('.', index);
            yield return new WaitWithCancel(0.2f, () => _active || _panic);
        }
        while (!_finished)
        {
            for (index = 0; index < _hash.Length; index++)
            {
                _screen.text = _hash[index];
                _screen.transform.localScale = new Vector3(_hashWidths[index], 1f, 1f);
                yield return new WaitWithCancel(0.5f, () => _finished);
            }
            if (_stageActive)
            {
                _screen.text = "Activation " + _currentActivation;
                _screen.transform.localScale = Vector3.one;
                yield return new WaitWithCancel(0.5f, () => !_stageActive);
            }
            _screen.text = "";
            yield return new WaitForSeconds(0.5f);
        }
    }
}
