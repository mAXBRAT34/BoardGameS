using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Local TOP 10, stored on this device via PlayerPrefs.
/// The best score per player is kept. Higher score is better.
/// </summary>
public static class CirksLeaderboardStore
{
    private const string SaveKey = "Cirks_Leaderboard_v1";
    private const string CurrentPlayerKey = "Cirks_PlayerName"; // CharacterSelect already uses this.
    private const int MaxEntries = 10;

    [Serializable]
    public class Entry
    {
        public string playerName;
        public int score;
        public long playedAtUtcTicks;
    }

    [Serializable]
    private class SaveData
    {
        public List<Entry> entries = new List<Entry>();
    }

    public static List<Entry> GetTopScores()
    {
        SaveData save = Read();
        Sort(save.entries);
        if (save.entries.Count > MaxEntries)
            save.entries.RemoveRange(MaxEntries, save.entries.Count - MaxEntries);
        return save.entries;
    }

    public static void SubmitCurrentPlayerScore(int finalScore)
    {
        string selectedName = PlayerPrefs.GetString(CurrentPlayerKey, "Player");
        SubmitScore(selectedName, finalScore);
    }

    public static void SubmitScore(string playerName, int finalScore)
    {
        playerName = SanitizeName(playerName);
        if (finalScore < 0)
        {
            Debug.LogWarning("Leaderboard: Negative score was ignored.");
            return;
        }

        SaveData save = Read();
        Entry existing = save.entries.Find(x =>
            string.Equals(x.playerName, playerName, StringComparison.OrdinalIgnoreCase));

        if (existing == null)
        {
            save.entries.Add(new Entry
            {
                playerName = playerName,
                score = finalScore,
                playedAtUtcTicks = DateTime.UtcNow.Ticks
            });
        }
        else if (finalScore > existing.score)
        {
            existing.score = finalScore;
            existing.playedAtUtcTicks = DateTime.UtcNow.Ticks;
            existing.playerName = playerName;
        }

        Sort(save.entries);
        if (save.entries.Count > MaxEntries)
            save.entries.RemoveRange(MaxEntries, save.entries.Count - MaxEntries);
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
        PlayerPrefs.Save();
        Debug.Log("Leaderboard: saved " + playerName + " with " + finalScore + " points.");
    }

    private static string SanitizeName(string name)
    {
        name = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Player";
        return name.Length > 18 ? name.Substring(0, 18) : name;
    }

    private static SaveData Read()
    {
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(json)) return new SaveData();

        try
        {
            SaveData result = JsonUtility.FromJson<SaveData>(json);
            if (result == null || result.entries == null) return new SaveData();
            result.entries.RemoveAll(x => x == null || x.score < 0);
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Leaderboard save is corrupt: " + ex.Message);
            return new SaveData();
        }
    }

    private static void Sort(List<Entry> entries)
    {
        entries.Sort((a, b) =>
        {
            int comparison = b.score.CompareTo(a.score);
            return comparison != 0 ? comparison : a.playedAtUtcTicks.CompareTo(b.playedAtUtcTicks);
        });
    }
}
