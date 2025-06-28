using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RenderUI
{

    private Playback playback;
    private TMP_Text debugText;
    private TMP_Text statsText;
    private ObjectHistoryManager history;
    private List<GameObject> objects;

    public RenderUI(TMP_Text debug, TMP_Text stats, ObjectHistoryManager history, List<GameObject> objs, Playback playback)
    {
        debugText = debug;
        statsText = stats;
        this.history = history;
        objects = objs;
        this.playback = playback;
    }

    private string DistanceStatsOverFrames(string partA, string partB, int frameWindow)
    {
        if (history.FrameCount == 0)
            return "n/a";

        int start = Mathf.Max(0, history.CurrentFrame - (frameWindow - 1));
        float min = float.MaxValue;
        float max = float.MinValue;
        bool valid = false;

        for (int i = start; i <= history.CurrentFrame; i++)
        {
            var frame = history.GetFrame(i);
            if (frame == null) continue;

            if (frame.TryGetValue(partA, out Vector3 posA) &&
                frame.TryGetValue(partB, out Vector3 posB))
            {
                float d = Vector3.Distance(posA, posB);
                min = Mathf.Min(min, d);
                max = Mathf.Max(max, d);
                valid = true;
            }
        }

        return valid ? $"min {min:F2}, max {max:F2}" : "n/a";
    }

    public void Update(int frameIndex)
    {
        if (debugText == null || history.FrameCount == 0) return;

        var frame = history.GetFrame(frameIndex);
        if (frame == null) return;

        int currentFrame = frameIndex + 1;
        int totalFrames = history.FrameCount;

        var sb = new System.Text.StringBuilder();

        // Playback state
        string state = playback != null && playback.IsPlaying ? "Playing" : "Paused";
        sb.AppendLine($"<b>{state}</b>");
        sb.AppendLine($"<b>Frame {currentFrame} / {totalFrames}</b>\n");

        foreach (var obj in objects)
        {
            if (frame.TryGetValue(obj.name, out Vector3 pos))
                sb.AppendLine($"{obj.name}: {pos}");
        }

        (float min, float max, string minPair, string maxPair) = GetDistanceStats(frame);
        sb.AppendLine($"\n<b>Min:</b> {min:F2} between {minPair}");
        sb.AppendLine($"<b>Max:</b> {max:F2} between {maxPair}");

        debugText.text = sb.ToString();

        // statsText
        if (statsText != null)
        {
            string ArmStats(int window) => DistanceStatsOverFrames("LeftArm", "RightArm", window);
            string LegStats(int window) => DistanceStatsOverFrames("LeftLeg", "RightLeg", window);

            var statsBuilder = new System.Text.StringBuilder();
            statsBuilder.AppendLine($"<b>Arm Distances</b>");
            statsBuilder.AppendLine($"1f: {ArmStats(1)}");
            statsBuilder.AppendLine($"5f: {ArmStats(5)}");
            statsBuilder.AppendLine($"10f: {ArmStats(10)}");
            statsBuilder.AppendLine();
            statsBuilder.AppendLine($"<b>Leg Distances</b>");
            statsBuilder.AppendLine($"1f: {LegStats(1)}");
            statsBuilder.AppendLine($"5f: {LegStats(5)}");
            statsBuilder.AppendLine($"10f: {LegStats(10)}");

            statsText.text = statsBuilder.ToString();
        }
    }


    private (float min, float max, string minPair, string maxPair) GetDistanceStats(Dictionary<string, Vector3> frame)
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        string minPair = "", maxPair = "";

        var names = new List<string>(frame.Keys);

        for (int i = 0; i < names.Count; i++)
        {
            for (int j = i + 1; j < names.Count; j++)
            {
                Vector3 posA = frame[names[i]];
                Vector3 posB = frame[names[j]];
                float dist = Vector3.Distance(posA, posB);
                string pair = $"{names[i]} & {names[j]}";

                if (dist < min)
                {
                    min = dist;
                    minPair = pair;
                }

                if (dist > max)
                {
                    max = dist;
                    maxPair = pair;
                }
            }
        }

        return (min, max, minPair, maxPair);
    }
}