//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI
{
    /// <inheritdoc />
    /// <summary>
    ///     EnhancedGUI Manager.
    ///     It contains some main EnhancedGUI references like default styles. (for now)
    /// </summary>
    [DefaultExecutionOrder(-667)]
    public class EnhancedGUIManager : MonoBehaviour
    {
        /// <summary>
        ///     Default (dark) skin reference.
        /// </summary>
        [Header("Resources ")]
        public EnhancedGUISkin DefaultDarkSkin;

        private void Awake()
        {
            if (Instance != null)
                Debug.LogWarning($"Two or more {nameof(EnhancedGUIManager)} objects detected! Make sure that there is always one active on scene.", this);

            Instance = this;
        }

        // private void OnGUI() => DebugDraw();
        private void DebugDraw()
        {
            GUILayout.Space(25);
            GUILayout.Label($"Renderers: {EnhancedGUIRenderer.Renderers.Count}");
            foreach (var r in EnhancedGUIRenderer.Renderers)
            {
                GUILayout.Label($"Renderer: {r.name}");
                GUILayout.Label($"\tIndex: {r.WindowsIndex}");
                GUILayout.Label($"\tWindows: {r.Windows.Length}");
                for (var index = 0; index < r.Windows.Length; index++)
                {
                    var p = r.Windows[index];
                    GUILayout.Label($"\t- #{index}: ");
                    GUILayout.Label($" \t\t- Position:\t{p.Rect.ToString()}");
                    GUILayout.Label($" \t\t- GUID:\t{p.Guid}");
                    GUILayout.Label($" \t\t- Depth:\t{p.Depth}");
                }
            }
        }

        internal static EnhancedGUIManager Instance { get; private set; }
    }
}
