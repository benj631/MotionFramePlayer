//Main.cs

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Main : MonoBehaviour
{
    [Header("Setup")]
    public GameObject bodyPartPrefab;
    public TMP_Text debugText; // For locations
    public TMP_Text statsText; // For stats

    [Header("Body Part Prefabs (Optional Overrides)")]
    public GameObject headPrefab;
    public GameObject torsoPrefab;
    public GameObject leftArmPrefab;
    public GameObject rightArmPrefab;
    public GameObject leftLegPrefab;
    public GameObject rightLegPrefab;

    [Header("Config")]
    public float jitter = 0.1f;
    public float spawnRange = 5f;
    public float playbackSpeed = 1f;

    private string[] bodyPartNames = new[] { "Head", "Torso", "LeftArm", "RightArm", "LeftLeg", "RightLeg" };

    private ObjectManager objectManager = new ObjectManager();
    private ObjectHistoryManager historyManager = new ObjectHistoryManager();
    private Playback playback;
    private RenderUI ui;

    private List<GameObject> bodyParts;

    void Start()
    {
        
        objectManager.prefabOverrides["Head"] = headPrefab;
        objectManager.prefabOverrides["Torso"] = torsoPrefab;
        objectManager.prefabOverrides["LeftArm"] = leftArmPrefab;
        objectManager.prefabOverrides["RightArm"] = rightArmPrefab;
        objectManager.prefabOverrides["LeftLeg"] = leftLegPrefab;
        objectManager.prefabOverrides["RightLeg"] = rightLegPrefab;


        bodyParts = objectManager.Generate(bodyPartPrefab, bodyPartNames);
        historyManager.Store(bodyParts);
        playback = new Playback(bodyParts, historyManager) { PlaybackSpeed = playbackSpeed };
        ui = new RenderUI(debugText, statsText, historyManager, bodyParts, playback);
        ui.Update(playback.CurrentFrame);
    }

    void Update()
    {
        HandleInput();

        if (playback != null)
        {
            playback.Update(Time.deltaTime);

            ui?.Update(playback.CurrentFrame);
        }
    }

    void HandleInput()
    {
        // ▶️ Toggle play/pause
        if (Input.GetKeyDown(KeyCode.Space))
            playback.IsPlaying = !playback.IsPlaying;

        // ➕ Add 1 jittered frame
        if (Input.GetKeyDown(KeyCode.F))
        {
            objectManager.ApplyRotation(bodyParts, bodyPartNames);
            historyManager.Store(bodyParts);
            ui.Update(playback.CurrentFrame);
        }

        // ➕➕ Add 10 jittered frames
        if (Input.GetKeyDown(KeyCode.G))
        {
            for (int i = 0; i < 10; i++)
            {
                objectManager.ApplyRotation(bodyParts, bodyPartNames);
                historyManager.Store(bodyParts);
            }
            ui.Update(playback.CurrentFrame);
        }

        // 🔄 Reset structure
        if (Input.GetKeyDown(KeyCode.R))
        {
            bodyParts = objectManager.Generate(bodyPartPrefab, bodyPartNames);
            historyManager.Store(bodyParts);
            ui = new RenderUI(debugText, statsText, historyManager, bodyParts, playback);
            ui.Update(playback.CurrentFrame);
        }

        // ⏪ Step back
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            playback.StepBack();
            ui.Update(playback.CurrentFrame);
        }

        // ⏩ Step forward
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            playback.StepForward();
            ui.Update(playback.CurrentFrame);
        }

        // 🔼 Increase speed (max 20fps)
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            playback.PlaybackSpeed = Mathf.Min(20f, playback.PlaybackSpeed + 5f);

        // 🔽 Decrease speed (min 5fps)
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            playback.PlaybackSpeed = Mathf.Max(5f, playback.PlaybackSpeed - 5f);

        // ⏮ Go to frame 1
        if (Input.GetKeyDown(KeyCode.B))
        {
            historyManager.GoToBeginning();
            historyManager.ApplyCurrentFrame(bodyParts);
            ui.Update(playback.CurrentFrame);
        }
    }
}
