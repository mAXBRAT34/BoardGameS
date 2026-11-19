using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CirksLeaderboardScreen : MonoBehaviour
{
    public CirksLeaderboardRow[] rows;
    public TMP_Text footerLabel;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        List<CirksLeaderboardStore.Entry> scores = CirksLeaderboardStore.GetTopScores();
        if (rows != null)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null)
                    rows[i].Display(i + 1, i < scores.Count ? scores[i] : null);
            }
        }

        if (footerLabel != null)
            footerLabel.text = scores.Count == 0
                ? "NO SCORES YET  •  FINISH A MATCH TO GET ON THE BOARD"
                : "YOUR BEST SCORE COUNTS  •  SAVED ON THIS DEVICE";
    }
}
