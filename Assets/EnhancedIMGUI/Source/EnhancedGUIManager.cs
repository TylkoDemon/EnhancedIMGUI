//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI
{
    [DefaultExecutionOrder(-999)]
    public class EnhancedGUIManager : MonoBehaviour
    {
        [Header("Resources ")]
        public EnhancedGUISkin DefaultDarkSkin;

        private void Awake()
        {
            if (Instance != null)
                Debug.LogWarning($"Two or more {nameof(EnhancedGUIManager)} objects detected! Make sure that there is always one active on scene.", this);

            Instance = this;
        }

        private void OnGUI()
        {
            /*
            GUILayout.Space(25);
            GUILayout.Label($"Index: {ImGui.WindowsIndex}");
            GUILayout.Label($"Windows: {ImGui.Windows.Length}");
            for (var index = 0; index < ImGui.Windows.Length; index++)
            {
                var p = ImGui.Windows[index];
                GUILayout.Label($"- #{index}: ");
                GUILayout.Label($" \t- Position:\t{p.Rect.ToString()}");
                GUILayout.Label($" \t- GUID:\t{p.Guid}");
                GUILayout.Label($" \t- Depth:\t{p.Depth}");
            }
            */
            //ImGui.EndWindows();
        }

        internal static EnhancedGUIManager Instance { get; private set; }
    }
}
