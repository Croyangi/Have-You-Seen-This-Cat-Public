using Shapes;
using UnityEngine;

public class FlatscreenDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProShapes textMesh;

    public void SetText(string text)
    {
        textMesh.text = text;
    }
}
