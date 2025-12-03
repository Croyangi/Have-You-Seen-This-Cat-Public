using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugCatXRay : ImmediateModeShapeDrawer
{
    public Vector3 playerPos;
    public List<GameObject> cats;

    public Color color;
    public float lineThickness;
    public float sphereRadius;
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
        cats = ManagerCat.instance.CatObjs;
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

            foreach (GameObject cat in cats)
            {
                Draw.Line(playerPos, cat.transform.position, color);
                Draw.Sphere(cat.transform.position, sphereRadius, color);
            }

        }
    }
}
