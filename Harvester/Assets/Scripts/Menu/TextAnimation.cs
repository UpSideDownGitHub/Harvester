using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    public TMP_Text text;

    [Header("X")]
    public float value1 = 0f;
    public float value2 = 0f;
    public float value3 = 0f;
    [Header("Y")]
    public float value4 = 2f;
    public float value5 = 0.01f;
    public float value6 = 10f;
    [Header("Z")]
    public float value7 = 0f;
    public float value8 = 0f;
    public float value9 = 0f;

    public void Update()
    {
        text.ForceMeshUpdate();
        var textInfo = text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) { continue; }

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                var orig = verts[charInfo.vertexIndex + j];
                verts[charInfo.vertexIndex + j] = orig + new Vector3(
                    Mathf.Sin(Time.time * value1 + orig.x * value2) * value3,
                    Mathf.Sin(Time.time * value4 + orig.x * value5) * value6,
                    Mathf.Sin(Time.time * value7 + orig.x * value8) * value9);

            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
