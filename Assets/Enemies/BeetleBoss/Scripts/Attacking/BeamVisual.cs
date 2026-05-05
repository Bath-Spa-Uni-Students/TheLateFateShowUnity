using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class BeamVisual : MonoBehaviour
{
    [Header("Beam Shape")]
    [SerializeField] private float beamLength = 6f;
    [SerializeField] private float beamWidthStart = 0.15f;
    [SerializeField] private float beamWidthEnd = 0.08f;

    [Header("Telegraph State")]
    [SerializeField] private Color telegraphColour = new Color(0.55f, 0.04f, 0.04f, 0.6f);

    [Header("Active State")]
    [SerializeField] private Color activeColour = new Color(1f, 0.08f, 0.08f, 1f);
    [SerializeField] private Color activeCoreColour = new Color(1f, 0.65f, 0.65f, 1f);

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseWidthAmount = 0.04f;

    private LineRenderer lr;
    private LineRenderer coreLR;       // second thinner LineRenderer for the inner glow
    private bool isActive = false;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        SetupLineRenderer();
        SetupCoreLineRenderer();

    }

    private void OnEnable()
    {
        // Default to telegraph state when the object is switched on
        SetTelegraphState();

    }

    private void OnDisable()
    {
        isActive = false;
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

    }

    public void SetTelegraphState()
    {
        isActive = false;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }

        ApplyColour(lr, telegraphColour, telegraphColour);
        ApplyWidth(lr, beamWidthStart * 0.6f, beamWidthEnd * 0.6f);

        if (coreLR != null)
            coreLR.enabled = false;

    }

    public void SetActive()
    {
        isActive = true;

        if (coreLR != null)
            coreLR.enabled = true;

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        pulseCoroutine = StartCoroutine(PulseCoroutine());

    }

    private void SetupLineRenderer()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;          // positions are local 
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);     // origin 
        lr.SetPosition(1, Vector3.up * beamLength);
        lr.textureMode = LineTextureMode.Tile;
        lr.numCapVertices = 6;
        lr.numCornerVertices = 4;

        lr.material = CreateBeamMaterial(activeColour);
        ApplyWidth(lr, beamWidthStart, beamWidthEnd);
        ApplyColour(lr, telegraphColour, telegraphColour);

    }

    private void SetupCoreLineRenderer()
    {
        var coreGO = new GameObject("BeamCore");
        coreGO.transform.SetParent(transform, false);

        coreLR = coreGO.AddComponent<LineRenderer>();// second LineRenderer for the inner glow
        coreLR.useWorldSpace = false;// positions are local to the parent
        coreLR.positionCount = 2;// same as main LR, but thinner and brighter for the inner glow
        coreLR.SetPosition(0, Vector3.zero);
        coreLR.SetPosition(1, Vector3.up * beamLength);// same length as main beam
        coreLR.textureMode = LineTextureMode.Tile;// allows the texture to repeat along the length of the beam
        coreLR.numCapVertices = 6;
        coreLR.sortingOrder = (lr.sortingOrder + 1); // render on top
        coreLR.material = CreateBeamMaterial(activeCoreColour);// brighter material for the core
        ApplyWidth(coreLR, beamWidthStart * 0.35f, beamWidthEnd * 0.35f);
        ApplyColour(coreLR, activeCoreColour, activeCoreColour);
        coreLR.enabled = false;

    }

    private IEnumerator PulseCoroutine()
    {
        while (isActive)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float wStart = beamWidthStart + pulseWidthAmount * t;
            float wEnd = beamWidthEnd + pulseWidthAmount * t;

            Color pulsed = Color.Lerp(activeColour, activeCoreColour, t * 0.4f);
            ApplyColour(lr, pulsed, pulsed);
            ApplyWidth(lr, wStart, wEnd);

            if (coreLR != null)
            {
                float coreW = (beamWidthStart * 0.35f) + pulseWidthAmount * 0.5f * t;
                ApplyWidth(coreLR, coreW, coreW * 0.7f);
            }

            yield return null;
        }
    }


    private static void ApplyColour(LineRenderer target, Color start, Color end)
    {
        target.startColor = start;
        target.endColor = end;
    }

    private static void ApplyWidth(LineRenderer target, float start, float end)
    {
        target.startWidth = start;
        target.endWidth = end;
    }

    private static Material CreateBeamMaterial(Color colour)
    {
        // Sprites/Default supports vertex colours and partial transparency
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = colour;
        return mat;
    }

}
