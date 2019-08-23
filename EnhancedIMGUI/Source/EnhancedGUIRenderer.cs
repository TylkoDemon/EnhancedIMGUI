//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace EnhancedIMGUI
{
    /// <inheritdoc />
    /// <summary>
    ///     EnhancedGUI Renderer.
    ///     Main GUI component thanks to you can render your EnhancedGUI!
    ///     Because of Unity's GUI.depth implementation to utilize gui Z order we need to call each window in separate OnGUI.
    /// </summary>
    [DefaultExecutionOrder(-666)]
    public class EnhancedGUIRenderer : MonoBehaviour
    {
        internal bool CanBeginWindow { get; set; } = true;
        internal bool CanDrawControl { get; set; }

        internal int FrameTarget { get; set; } = -1;
        internal bool IsWindowMoving { get; set; }
        internal bool IsWindowResize { get; set; }
        internal Vector2 MouseStartPosition { get; set; }

        internal Rect StartRect { get; set; }

        internal int WindowsIndex { get; set; }
        internal EnhancedGUIWindow[] Windows { get; set; } = new EnhancedGUIWindow[0];
        internal Rect ScreenRect => new Rect(0f, 0f, Screen.width, Screen.height);
        internal EnhancedGUISkin ActiveSkin { get; set; }

        private SmartMethod _onEnhancedGUI;
        private bool _isFrameEnd;

        internal static List<EnhancedGUIRenderer> Renderers { get;  } = new List<EnhancedGUIRenderer>();
        private void Awake() => Renderers.Add(this);
        private void OnDestroy() => Renderers.Remove(this);
        private void OnGUI() => RenderImage();

        private void RenderImage()
        {
            if (EnhancedGUIManager.Instance == null)
            {
                Debug.LogError("No EnhancedGUIManager Instance found! Make sure that EnhancedIMGUIManager prefab is always on scene.", this);
                enabled = false;
                return;
            }

            Profiler.BeginSample("EnhancedGUIRenderer.RenderImage", this);
            ImGui.Renderer = this;
            // apply default style just for sure
            ImGui.StyleColorsDark();
            // draw EnhancedGUI!
            DrawEnhancedGUI();
            // apply cursor icon
            ImGui.DrawCursor();
            ImGui.EndWindows();
            ImGui.Renderer = null;
            Profiler.EndSample();
            Profiler.BeginSample("EnhancedGUIRenderer.CheckForEndFrame", this);
            CheckForEndFrame();
            Profiler.EndSample();
        }

        private void CheckForEndFrame()
        {
            _isFrameEnd = true;
            foreach (var r in Renderers)
            {
                if (r.isActiveAndEnabled && !r._isFrameEnd)
                    return;
            }

            ImGui.EndFrame();
        }

        private void DrawEnhancedGUI()
        {
            Profiler.BeginSample("EnhancedGUIRenderer.DrawEnhancedGUI", this);
            try
            {
                if (_onEnhancedGUI == null) _onEnhancedGUI = new SmartMethod(this, "OnEnhancedGUI");
                _onEnhancedGUI.Invoke();

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            Profiler.EndSample();
        }

        internal void MoveDepth()
        {
            for (int index = 0; index < Windows.Length; index++)
                Windows[index].Depth = Windows[index].Depth + 1;
        }

        /// <summary>
        ///     Run raycast test for window at given point on the screen.
        /// </summary>
        internal static bool TestRaycast(Vector2 screenPoint, [NotNull] Func<EnhancedGUIWindow, Rect> getScreen,
            out EnhancedGUIWindow window)
        {
            if (getScreen == null) throw new ArgumentNullException(nameof(getScreen));
            window = default(EnhancedGUIWindow);
            var allWindows = new List<EnhancedGUIWindow>();
            foreach (var r in Renderers)
            {
                allWindows.AddRange(r.Windows);
            }

            var sorted = allWindows.OrderBy(o => o.Depth);
            foreach (var s in sorted)
            {
                var screen = getScreen.Invoke(s);
                if (!screen.Contains(screenPoint)) continue;
                window = s;
                break;
            }

            return !window.Equals(default(EnhancedGUIWindow));
        }
    }
}
