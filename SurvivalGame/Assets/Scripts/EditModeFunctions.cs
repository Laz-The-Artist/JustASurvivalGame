using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGeneratorV3))]
public class EditModeFunctions : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        WorldGeneratorV3 world = (WorldGeneratorV3)target;

        if (GUILayout.Button("Gen Voronoi")) {

            world.InitialiseWorld();
            world.GenVoronoiV2();
        }
    }
}