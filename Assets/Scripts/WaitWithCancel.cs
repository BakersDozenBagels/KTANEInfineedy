using System;
using UnityEngine;

internal class WaitWithCancel : CustomYieldInstruction
{
    private readonly float _endTime;
    private readonly Func<bool> _cancel;

    public WaitWithCancel(float duration, Func<bool> cancel)
    {
        _endTime = Time.time + duration;
        _cancel = cancel;
    }

    public override bool keepWaiting
    {
        get
        {
            return Time.time < _endTime && !_cancel();
        }
    }
}

