using TMPro;
using UnityEngine;

public class CirksLeaderboardRow : MonoBehaviour
{
    public TMP_Text rankLabel;
    public TMP_Text playerLabel;
    public TMP_Text scoreLabel;

    public void Display(int rank, CirksLeaderboardStore.Entry entry)
    {
        if (rankLabel != null) rankLabel.text = rank.ToString("00");
        if (playerLabel != null)
            playerLabel.text = entry != null ? entry.playerName : (rank == 1 ? "BE THE FIRST!" : "—");
        if (scoreLabel != null) scoreLabel.text = entry != null ? entry.score.ToString("N0") : "—";
    }
}
