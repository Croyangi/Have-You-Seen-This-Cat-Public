using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using Shapes;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugCopyCatXRay : ImmediateModeShapeDrawer
{
    public Vector3 playerPos;
    public Vector3 copyCatPos;

    public Color color;
    public float lineThickness;
    public float sphereRadius;
    public float fontSize;
    public bool doShow;
    
    private new void OnEnable()
    {
        base.OnEnable();  // Ensures Shapes' OnEnable is called
        //DevTool_Toggle.togglePerformed += OnTogglePerformed;
    }

    private new void OnDisable()
    {
        //DevTool_Toggle.togglePerformed -= OnTogglePerformed;
        base.OnDisable(); // Optionally call base.OnDisable() to be fully consistent
    }


    public void OnTogglePerformed()
    {
        doShow = !doShow;
    }
    
    private void Update()
    {
        if (!doShow) return;
        playerPos = ManagerPlayer.instance.PlayerObj.transform.position;
        copyCatPos = ManagerCopyCat.Instance.CopyCat.transform.position;
    }

    public override void DrawShapes(Camera cam)
    {
        if (!Application.isPlaying) return;
        if (!doShow) return;

        using (Draw.Command(cam))
        {
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Meters;
            Draw.Thickness = lineThickness;
            Draw.ZTest = CompareFunction.Always;
            
            Draw.Line(playerPos, copyCatPos, color);
            Draw.Sphere(copyCatPos, sphereRadius, color);

            Draw.Text(
                pos: copyCatPos + Vector3.up * 4f,
                rot: cam.transform.rotation,
                content: ManagerCopyCat.Instance.CopyCatStateMachine.CurrentState.ToString(),
                fontSize: fontSize,
                color: Color.white,
                align: TextAlign.Center
            );

        }
    }
}