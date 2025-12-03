using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class VirtualScreen : MonoBehaviour
{
    [Header("References")]
    public Camera uiCamera;                   // Camera rendering UI to the RenderTexture
    public GraphicRaycaster uiRaycaster;      // The raycaster on the original UI canvas
    public EventSystem eventSystem;           // The active EventSystem in the scene

    private PointerEventData pointerData;
    private List<RaycastResult> results = new List<RaycastResult>();

    void Update()
    {
        // 1️⃣ Create pointer event based on current mouse position
        if (pointerData == null)
            pointerData = new PointerEventData(eventSystem);

        pointerData.position = Input.mousePosition;
        results.Clear();

        // 2️⃣ Perform a raycast into the *original* UI canvas
        uiRaycaster.Raycast(pointerData, results);

        // 3️⃣ If we hit something, simulate interaction
        if (results.Count > 0)
        {
            var go = results[0].gameObject;

            // Hover / Press
            if (Input.GetMouseButtonDown(0))
                ExecuteEvents.Execute(go, pointerData, ExecuteEvents.pointerDownHandler);

            if (Input.GetMouseButtonUp(0))
                ExecuteEvents.Execute(go, pointerData, ExecuteEvents.pointerUpHandler);

            if (Input.GetMouseButton(0))
                ExecuteEvents.Execute(go, pointerData, ExecuteEvents.dragHandler);

            // Hover event
            ExecuteEvents.Execute(go, pointerData, ExecuteEvents.pointerEnterHandler);
        }
    }
}