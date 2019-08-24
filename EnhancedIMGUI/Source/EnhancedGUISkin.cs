//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

// disable xml doc warn
#pragma warning disable 1591

namespace EnhancedIMGUI
{
    /// <inheritdoc />
    /// <summary>
    ///     The EnhancedGUI Skin.
    ///     It defines style of every object used in EnhancedGUI.
    /// </summary>
    [CreateAssetMenu(fileName = "newEnhancedGUISkin", menuName = "Enhanced GUI Skin")]
    public class EnhancedGUISkin : ScriptableObject
    {
        [Header("Base")]
        public GUISkin BaseSkinReference;

        [Header("Enhanced Styles (Controls)")]
        public GUIStyle FoldoutOpen;
        public GUIStyle FoldoutClose;
        public GUIStyle SliderText;
        public GUIStyle LabelText;

        [Header("Enhanced Styles (Windows)")]
        public GUIStyle Header;
        public GUIStyle HeaderClosed;
        public GUIStyle HeaderInactive;
        public GUIStyle HeaderInactiveClosed;
        public GUIStyle HeaderText;
        public GUIStyle Window;
        public GUIStyle Resize;

        [Header("Enhanced Styles (Msc)")]
        public GUIStyle Hidden;

        [Header("Cursor")]
        public Texture2D CursorPoint;
        public Texture2D CursorDrag;

        public const char RightwardsArrowChar = '►';
        public const char DownwardsArrowChar = '▼';
    }
}
