//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI
{
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
        public GUIStyle HeaderText;
        public GUIStyle Window;
        public GUIStyle Resize;

        [Header("Enhanced Styles (Msc)")]
        public GUIStyle Hidden;

        [Header("Cursor")]
        public Texture2D CursorHand;

        public const char RightwardsArrowChar = '►';
        public const char DownwardsArrowChar = '▼';
    }
}
