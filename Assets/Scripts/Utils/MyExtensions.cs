using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine;

public static class MyExtensions
{
    public static void SetRangeValues<T>(this IList<T> source, int start, int end, T value)
    {
        if (start > 0 && end < source.Count)
        {
            for (int i = start; i <= end; i++)
            {
                source[i] = value;
            }
        }
    }

    public static T Pop<T>(this IList<T> source, int idx)
    {
        var ret = source[idx];
        source.RemoveAt(idx);
        return ret;
    }

    public static TweenerCore<float, float, FloatOptions> DOMaxValue(this Slider target, float endValue, float duration, bool snapping = false)
    {
        TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.maxValue, x => target.maxValue = x, endValue, duration);
        t.SetOptions(snapping).SetTarget(target);
        return t;
    }
}