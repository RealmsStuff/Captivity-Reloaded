using UnityEngine;

// Counts a walker's failed tries at the same climb. A climb fails when the walker's jump never brings its head
// within reach of the ledge, which happens on a climb taller than the jump. Retrying keeps the walker from skipping
// ledges, but a climb that can never succeed would leave it stuck below forever and the wave could not end, so after
// a few failed tries at the same climb the caller places it on the ledge, as the game did before every retry.
public sealed class ClimbRetryCounter
{
	public const int TriesBeforePlacing = 3;
	// Tries aimed this close together are the same climb.
	private const float SameClimbDistance = 0.5f;

	private Vector2 m_target;
	private int m_failures;

	public int GetFailures()
	{
		return m_failures;
	}

	// Returns true when the walker should now be placed on the ledge; the count then starts over.
	public bool RecordFailure(Vector2 i_target)
	{
		if (m_failures == 0 || Vector2.Distance(i_target, m_target) > SameClimbDistance)
		{
			m_target = i_target;
			m_failures = 0;
		}
		m_failures++;
		if (m_failures < TriesBeforePlacing) return false;
		m_failures = 0;
		return true;
	}

	public void Reset()
	{
		m_failures = 0;
	}
}
