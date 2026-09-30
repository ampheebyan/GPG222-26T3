using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LightToggle))]
public class LightToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Toggle"))
        {
            // ‘target’ is the magic variable that editors use to link back to the original component. It’s in the BASE CLASS, so you have to ‘cast’ to get access to YOUR functions.
            LightToggle lt;
            lt = target as LightToggle;
            lt?.ToggleRpc();
        }
    }

}
