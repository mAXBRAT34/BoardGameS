using UnityEngine;

/// <summary>
/// Optional helper for GameScene. At the END of a real match, call SubmitFinalScore(points)
/// exactly once. Do not call it on scene load or at the start of the match.
/// </summary>
public class CirksLeaderboardScoreReporter : MonoBehaviour
{
    private bool alreadySubmitted;

    public void SubmitFinalScore(int points)
    {
        if (alreadySubmitted) return;
        alreadySubmitted = true;
        CirksLeaderboardStore.SubmitCurrentPlayerScore(points);
    }
}
