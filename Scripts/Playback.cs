// Playback.cs

using System.Collections.Generic;
using UnityEngine;

public class Playback
{
    private readonly ObjectHistoryManager historyManager;
    private readonly List<GameObject> objects;

    private float timer = 0f;
    private bool frameChanged = false;

    public float PlaybackSpeed { get; set; } = 1f;
    public bool IsPlaying { get; set; } = false;
    public bool PlayForward { get; set; } = true;

    public Playback(List<GameObject> objects, ObjectHistoryManager historyManager)
    {
        this.objects = objects;
        this.historyManager = historyManager;
    }

    public void Update(float deltaTime)
    {
        if (!IsPlaying || historyManager.FrameCount == 0)
            return;

        timer += deltaTime;

        if (timer >= 1f / PlaybackSpeed)
        {
            timer = 0f;

            bool advanced = PlayForward ? historyManager.StepForward() : historyManager.StepBack();
            if (advanced)
            {
                historyManager.ApplyCurrentFrame(objects);
            }
            else
            {
                IsPlaying = false; //Stop playback at end
            }
        }
    }

    public void StepForward()
    {
        if (historyManager.StepForward())
        {
            historyManager.ApplyCurrentFrame(objects);
            frameChanged = true;
        }
    }

    public void StepBack()
    {
        if (historyManager.StepBack())
        {
            historyManager.ApplyCurrentFrame(objects);
            frameChanged = true;
        }
    }

    public void Reset()
    {
        timer -= 1f / PlaybackSpeed;
        historyManager.ApplyCurrentFrame(objects);
        frameChanged = true;
    }

    public int CurrentFrame => historyManager.CurrentFrame;

    public bool ShouldRenderFrame
    {
        get
        {
            bool result = frameChanged;
            frameChanged = false; // Reset flag
            return result;
        }
    }
}
