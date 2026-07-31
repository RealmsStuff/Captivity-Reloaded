using System;
using UnityEngine;

public static class AnimationTools
{
	public enum Transition
	{
		Smooth = 0,
		Steep = 1
	}

	private static float TimeCosine(float i_start, float i_end, float i_time, float i_cosineMin = 0f, float i_cosineMax = 1f)
	{
		float num = Mathf.Cos((i_cosineMin + i_time * (i_cosineMax - i_cosineMin)) * (float)Math.PI);
		float num2 = (0.5f - num * 0.5f - i_cosineMin) / (i_cosineMax - i_cosineMin);
		return i_start + (i_end - i_start) * num2;
	}

	public static float CalculateOverTime(Transition i_transitionStart, Transition i_transitionEnd, float i_start, float i_end, float i_time)
	{
		float num = Mathf.Clamp01(i_time);
		if (i_transitionStart == Transition.Steep && i_transitionEnd == Transition.Steep)
		{
			return Mathf.Lerp(i_start, i_end, num);
		}
		float i_cosineMin = ((i_transitionStart == Transition.Steep) ? 0.5f : 0f);
		float i_cosineMax = ((i_transitionEnd == Transition.Steep) ? 0.5f : 1f);
		return TimeCosine(i_start, i_end, num, i_cosineMin, i_cosineMax);
	}

	public static Vector2 CalculateOverTimeVector2(Transition i_transitionStart, Transition i_transitionEnd, Vector2 i_start, Vector2 i_end, float i_time)
	{
		return new Vector3(CalculateOverTime(i_transitionStart, i_transitionEnd, i_start.x, i_end.x, i_time), CalculateOverTime(i_transitionStart, i_transitionEnd, i_start.y, i_end.y, i_time));
	}

	public static Quaternion CalculateOverTime(Transition i_transitionStart, Transition i_transitionEnd, Quaternion i_start, Quaternion i_end, float i_time)
	{
		return Quaternion.Slerp(i_start, i_end, CalculateOverTime(i_transitionStart, i_transitionEnd, 0f, 1f, i_time));
	}
}
