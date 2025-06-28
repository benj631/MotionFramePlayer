using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
    private readonly Dictionary<string, Vector3> defaultOffsets = new()
    {
        { "Torso",    Vector3.zero },
        { "Head",     new Vector3(0, 0.6f, 0) },     // Head above torso
        { "LeftArm",  new Vector3(-0.5f, 0.3f, 0) }, // Arms out from upper torso
        { "RightArm", new Vector3(0.5f, 0.3f, 0) },
        { "LeftLeg",  new Vector3(-0.3f, -0.6f, 0) }, // Legs below torso
        { "RightLeg", new Vector3(0.3f, -0.6f, 0) }
    };

    /// <summary>
    /// Optional prefab overrides for each part name.
    /// You can populate this externally (e.g. from Main.cs).
    /// </summary>
    public Dictionary<string, GameObject> prefabOverrides = new();

    /// <summary>
    /// Generates a set of GameObjects in humanoid form.
    /// </summary>
    public List<GameObject> Generate(GameObject fallbackPrefab, string[] bodyPartNames)
    {
        List<GameObject> objects = new();

        Vector3 torsoOrigin = new Vector3(0, 1.2f, 0);

        Dictionary<string, Vector3> offsetsFromTorso = new()
        {
            { "Torso",    Vector3.zero },
            { "Head",     new Vector3(0, 0.0f, 0) },
            { "LeftArm",  new Vector3( 1.2f, 0.6f, 0) },
            { "RightArm", new Vector3(-1.2f, 0.6f, 0) },
            { "LeftLeg",  new Vector3(0.0f, 0.0f, 0) },
            { "RightLeg", new Vector3(0.0f, 0.0f, 0) }
        };

        foreach (string part in bodyPartNames)
        {
            GameObject prefab = prefabOverrides.ContainsKey(part) && prefabOverrides[part] != null
                ? prefabOverrides[part]
                : fallbackPrefab;

            if (prefab == null)
            {
                Debug.LogError($"Missing prefab for body part: {part}");
                continue;
            }

            Vector3 offset = offsetsFromTorso.ContainsKey(part) ? offsetsFromTorso[part] : Vector3.zero;
            Vector3 pos = torsoOrigin + offset;

            GameObject obj = GameObject.Instantiate(prefab, pos, prefab.transform.rotation);
            obj.name = part;
            objects.Add(obj);
        }

        return objects;
    }

    public void ApplyJitter(List<GameObject> objects, float jitterAmount)
    {
        foreach (var obj in objects)
        {
            Vector3 offset = Random.insideUnitSphere * jitterAmount;
            Vector3 newPos = obj.transform.position + offset;
            newPos.y = Mathf.Max(0f, newPos.y);
            obj.transform.position = newPos;
        }
    }

    public void ApplyRotation(List<GameObject> objects, string[] bodyPartNames, float angleRange = 10f, float correctionStrength = 0.2f)
    {
        int torsoIndex = System.Array.IndexOf(bodyPartNames, "Torso");
        if (torsoIndex == -1) return;

        GameObject torso = objects[torsoIndex];
        Vector3 torsoPos = torso.transform.position;
        Quaternion torsoRot = torso.transform.rotation;

        for (int i = 0; i < bodyPartNames.Length; i++)
        {
            if (i == torsoIndex) continue;

            string partName = bodyPartNames[i];
            GameObject part = objects[i];

            if (!defaultOffsets.TryGetValue(partName, out Vector3 localOffset))
                continue;

            // Step 1: Compute the current joint anchor in world space
            Vector3 rotatedOffset = torsoRot * localOffset;
            Vector3 anchorPoint = torsoPos + rotatedOffset;

            // Step 2: Pull part gently to correct position relative to torso
            Vector3 idealPos = anchorPoint;
            part.transform.position = Vector3.Lerp(part.transform.position, idealPos, correctionStrength);

            // Step 3: Apply a small rotation around the joint
            Vector3 axis = Random.onUnitSphere;
            float angle = Random.Range(-angleRange, angleRange) * 0.5f;
            part.transform.RotateAround(anchorPoint, axis, angle);
        }
    }


    public void ApplyPositions(List<GameObject> objects, Vector3[] positions)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            Vector3 pos = positions[i];
            pos.y = Mathf.Max(0f, pos.y);
            objects[i].transform.position = pos;
        }
    }
}
