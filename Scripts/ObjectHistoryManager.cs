using System.Collections.Generic;
using UnityEngine;

public class ObjectHistoryManager
{
    private readonly List<Dictionary<string, Vector3>> history = new();
    public int FrameCount => history.Count;
    public int CurrentFrame { get; private set; } = 0;

    /// <summary>
    /// Records the current positions of all objects by name.
    /// </summary>
    public void Store(List<GameObject> objects)
    {
        Dictionary<string, Vector3> frame = new();
        foreach (var obj in objects)
        {
            if (!frame.ContainsKey(obj.name))
                frame[obj.name] = obj.transform.position;
        }

        history.Add(frame);
        CurrentFrame = history.Count - 1;
    }

    public Dictionary<string, Vector3> GetFrame(int index)
    {
        if (index < 0 || index >= history.Count) return null;
        return history[index];
    }

    public Dictionary<string, Vector3> GetCurrentFrame() => GetFrame(CurrentFrame);

    public bool StepForward()
    {
        if (CurrentFrame < history.Count - 1)
        {
            CurrentFrame++;
            return true;
        }
        return false;
    }

    public bool StepBack()
    {
        if (CurrentFrame > 0)
        {
            CurrentFrame--;
            return true;
        }
        return false;
    }

    public void ApplyFrame(List<GameObject> objects, int frameIndex)
    {
        var frame = GetFrame(frameIndex);
        if (frame == null) return;

        foreach (var obj in objects)
        {
            if (frame.TryGetValue(obj.name, out Vector3 pos))
                obj.transform.position = pos;
        }
    }

    public void ApplyCurrentFrame(List<GameObject> objects)
    {
        ApplyFrame(objects, CurrentFrame);
    }

    /// <summary>
    /// Returns a deep copy of the history data.
    /// </summary>
    public List<Dictionary<string, Vector3>> GetRawHistory() => new(history);

    /// <summary>
    /// Loads position frames from external data.
    /// </summary>
    public void LoadFromFrames(List<Dictionary<string, Vector3>> loadedFrames)
    {
        history.Clear();
        history.AddRange(loadedFrames);
        CurrentFrame = 0;
    }

    public void GoToBeginning()
    {
        CurrentFrame = 0;
    }
}