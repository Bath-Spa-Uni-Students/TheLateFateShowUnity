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
        
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
    }

    public void SetTelegraphState()
    {
        isActive = false;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    public void SetActive()
    {
        isActive = true;
  
    }
 
    private void SetupLinerRenderer()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;          // positions are local 
        lr.positionCount = 2;
        lr.SetPosition(0, Vector3.zero);     // origin 
        lr.SetPosition(1, Vector3.up * beamLength);
        lr.textureMode = LineTextureMode.Tile;
        lr.numCapVertices = 6;
        lr.numCornerVertices = 4;

    }

    private void SetupCoreLineRenderer()
    {
        GameObject coreObj = new GameObject("BeamCore");
        coreObj.transform.SetParent(transform, false);
        coreLR = coreObj.AddComponent<LineRenderer>();
        coreLR.useWorldSpace = false;
        coreLR.positionCount = 2;
        coreLR.SetPosition(0, Vector3.zero);
        coreLR.SetPosition(1, Vector3.up * beamLength);
        coreLR.textureMode = LineTextureMode.Tile;
        coreLR.numCapVertices = 6;
        coreLR.numCornerVertices = 4;
    }

    private IEnumerator PulseCoroutine()
    {
        while (isActive)
        {
           
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
