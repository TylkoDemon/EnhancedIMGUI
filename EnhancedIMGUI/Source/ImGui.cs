//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace EnhancedIMGUI
{
    /// <summary>
    ///     Core EnhancedIMGUI class.
    ///     Contains all controls EnhancedIMGUI can draw.
    /// </summary>
    public static class ImGui
    {
        /// <summary>
        ///     Begins new window area.
        /// </summary>
        public static void Begin(string windowName)
        {
            var isWindowActive = true;
            Begin(windowName, ref isWindowActive, false);
        }

        /// <summary>
        ///     Begins new window area.
        /// </summary>
        public static void Begin(string windowName, ref bool isWindowOpen)
        {
            Begin(windowName, ref isWindowOpen, true);
        }

        /// <summary>
        ///     Begins new window area.
        /// </summary>
        public static void Begin(string windowName, ref bool isWindowOpen, bool canDeactivate)
        {
            if (!Renderer.CanBeginWindow)
                throw new InvalidOperationException("You are calling Begin() without first ending previous window by calling End().");

            Renderer.CanDrawControl = true;
            Renderer.CanBeginWindow = false;

            //
            // CONST
            //
            const int headerHeight = 25;
            const int resizeButtonSize = 20;

            //
            // INIT
            //
            var pushDepth = false;
            var originalRect = NextWindow(windowName, out bool isContentActive, out bool isWindowOpen2, out var depth, out var guid);
            var pushingRect = new Rect(originalRect);
            var header = new Rect(originalRect.x, originalRect.y, originalRect.width, headerHeight);
            GUI.depth = depth;
            var enabled = GUI.depth == 0;

            if (!isWindowOpen)
            {
                _nextWindowIsInactive = true;

                if (isWindowOpen2)
                {
                    // need to save last window state here to be able to restore rect in next activation
                    EnhancedGUISave.ApplyWindowOnce(windowName, originalRect, isContentActive);
                }

                PushWindow(pushingRect, header, isContentActive, false);
                return;
            }

            //
            // DRAW: HEADER
            //

            var headerStyle = isContentActive ? Renderer.ActiveSkin.Header : Renderer.ActiveSkin.HeaderClosed;
            if (!enabled)
                headerStyle = isContentActive ? Renderer.ActiveSkin.HeaderInactive : Renderer.ActiveSkin.HeaderInactiveClosed;

            GUILayout.BeginArea(header, headerStyle);
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(
                    (isContentActive ? EnhancedGUISkin.DownwardsArrowChar : EnhancedGUISkin.RightwardsArrowChar).ToString(),
                    isContentActive ? Renderer.ActiveSkin.FoldoutClose : Renderer.ActiveSkin.FoldoutOpen))
                {
                    isContentActive = !isContentActive;
                }

                GUILayout.Label(windowName, Renderer.ActiveSkin.HeaderText);
                GUILayout.FlexibleSpace();
                if (canDeactivate)
                {
                    if (GUILayout.Button("X", Renderer.ActiveSkin.HeaderClose))
                    {
                        isWindowOpen = false;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            //
            // TEST RAYCAST: Test what window is on top.
            //
            var e = Event.current;
            EnhancedGUIRenderer.TestRaycast(e.mousePosition, window => window.IsContentActive ? window.Rect : window.Header, out var hit);
            if (hit.Name == windowName)
            {
                if (!enabled && e.type == EventType.MouseDown)
                {
                    pushDepth = true;
                }
            }

            //
            // INPUT: Window movement.
            //
            if (Renderer.FrameTarget == -1 && hit.Guid == guid)
            {
                if (header.Contains(e.mousePosition))
                {
                    SetCursor(Renderer.ActiveSkin.CursorPoint);
                    if (e.type == EventType.MouseDown)
                    {
                        Renderer.IsWindowMoving = true;
                        Renderer.StartRect = originalRect;
                        Renderer.MouseStartPosition = e.mousePosition;

                        Renderer.FrameTarget = Renderer.WindowsIndex;
                        pushDepth = true;
                    }
                }     
            }

            if (e.type == EventType.MouseUp)
            {
                Renderer.IsWindowMoving = false;
                Renderer.FrameTarget = -1;
            }

            //
            //  PROCESS: Window Movement.
            //
            if (CanInteract() && Renderer.IsWindowMoving && Renderer.FrameTarget == Renderer.WindowsIndex)
            {
                SetCursor(Renderer.ActiveSkin.CursorDrag);

                var delta = new Vector2(e.mousePosition.x - Renderer.MouseStartPosition.x, e.mousePosition.y - Renderer.MouseStartPosition.y);
                pushingRect.position = new Vector2(Renderer.StartRect.x + delta.x, Renderer.StartRect.y + delta.y);

                pushingRect.height = header.height;
                ClampRectToScreen(ref pushingRect, Renderer.ScreenRect);
                pushingRect.height = originalRect.height;
            }

            //
            //  DRAW: Begin window area.
            //
            var windowRect = new Rect(originalRect.x,
                originalRect.y + header.height,
                originalRect.width,
                isContentActive ? originalRect.height - header.height : 0);
            GUILayout.BeginArea(windowRect, isContentActive ? Renderer.ActiveSkin.Window : Renderer.ActiveSkin.Hidden);
            // GUILayout.Label($"{GUI.depth}, {guid}");

            //
            //  INPUT: RESIZE.
            // 
            var resizeButton = new Rect(originalRect.width - resizeButtonSize - 1,
                originalRect.height - header.height - resizeButtonSize - 1,
                resizeButtonSize,
                resizeButtonSize);
            GUI.enabled = enabled;
            GUI.Box(resizeButton, string.Empty, Renderer.ActiveSkin.Resize);
            GUI.enabled = true;

            //
            //  INPUT: Window Resize.
            // 
            if (Renderer.FrameTarget == -1 && hit.Guid == guid)
            {
                if (resizeButton.Contains(e.mousePosition))
                {
                    SetCursor(Renderer.ActiveSkin.CursorPoint);
                    if (e.type == EventType.MouseDown && Renderer.FrameTarget == -1)
                    {
                        Renderer.IsWindowResize = true;
                        Renderer.StartRect = originalRect;
                        Renderer.MouseStartPosition = e.mousePosition;

                        Renderer.FrameTarget = Renderer.WindowsIndex;
                        pushDepth = true;
                    }
                }
            }

            if (e.type == EventType.MouseUp)
            {
                Renderer.IsWindowResize = false;
                Renderer.FrameTarget = -1;
            }

            //
            //  PROCESS: Window Resize.
            //
            if (CanInteract() && Renderer.IsWindowResize && Renderer.FrameTarget == Renderer.WindowsIndex)
            {
                SetCursor(Renderer.ActiveSkin.CursorDrag);

                var delta = new Vector2(e.mousePosition.x - Renderer.MouseStartPosition.x,
                    e.mousePosition.y - Renderer.MouseStartPosition.y);
                pushingRect.size =
                    new Vector2(Renderer.StartRect.size.x + delta.x, Renderer.StartRect.size.y + delta.y);

                ClampRectSize(ref pushingRect);
            }

            if (hit.Guid == guid && e.type == EventType.MouseDown && Renderer.FrameTarget == -1 && pushingRect.Contains(e.mousePosition))
            {
                pushDepth = true;
            }

            if (pushDepth) PushDepth();
            PushWindow(pushingRect, header, isContentActive, true);

            GUI.enabled = enabled;
        }

        /// <summary>
        ///     Ends window area.
        /// </summary>
        public static void End()
        {
            if (Renderer.CanBeginWindow)
                throw new InvalidOperationException("You are calling End() without calling Begin().");

            Renderer.CanDrawControl = false;
            Renderer.CanBeginWindow = true;

            GUI.enabled = true;
            if (!_nextWindowIsInactive)
                GUILayout.EndArea();

            _nextWindowIsInactive = false;
        }

        /// <summary>
        ///     Draws text.
        /// </summary>
        public static void Text(string str)
        {
            if (!CheckControlDraw()) return;
            GUILayout.Label(str);
        }

        /// <summary>
        ///     Draws a button.
        /// </summary>
        public static bool Button(string str)
        {
            if (!CheckControlDraw()) return false;
            var controlId = GetControlId(nameof(Button));

            GUI.SetNextControlName(controlId.ToString());
            if (DrawControlId)
                str += $" ({controlId})";
            var b = GUILayout.Button(str);
            CheckControlAndDrawPointer();
            return b;
        }

        /// <summary>
        ///     Draws a toggle.
        /// </summary>
        public static void Toggle(string label, ref bool b) => ImGuiInternal.InternalToggle(GetControlId(nameof(Toggle)), label, ref b, ControlWidth);
        
        /// <summary>
        ///     Draws a text input.
        /// </summary>
        public static void InputText(string label, ref string input) => ImGuiInternal.InternalInputText(GetControlId(nameof(InputText)), label, ref input, ControlWidth);
        
        /// <summary>
        ///     Draws a float slider.
        /// </summary>
        public static void SliderFloat(string label, ref float f, float min, float max) => ImGuiInternal.InternalSlider(GetControlId(nameof(SliderFloat)), label, ref f, min, max, false, ControlWidth);
        
        /// <summary>
        ///     Draws a int32 slider.
        /// </summary>
        public static void SliderInt(string label, ref int i, int min, int max)
        {
            float f = i;
            ImGuiInternal.InternalSlider(GetControlId(nameof(SliderInt)), label, ref f, min, max, true, ControlWidth);
            i = (int) f;
        }

        /// <summary>
        ///     Draws a float field.
        /// </summary>
        public static void FloatField(string label, ref float f) => ImGuiInternal.InternalFloatField(GetControlId(nameof(FloatField)), label, ref f, ControlWidth);
        
        /// <summary>
        ///     Draws a int32 field.
        /// </summary>
        public static void IntField(string label, ref int i) => ImGuiInternal.InternalIntField(GetControlId(nameof(IntField)), label, ref i, ControlWidth);

        /// <summary>
        ///     Draws a color4 field.
        /// </summary>
        /// <remarks>Not fully implemented yet.</remarks>
        public static void ColorEdit4(string label, ref Color c)
        {
            if (!CheckControlDraw()) return;
            var controlId1 = GetControlId(nameof(ColorEdit4) + "_0");
            var controlId2 = GetControlId(nameof(ColorEdit4) + "_1");
            var controlId3 = GetControlId(nameof(ColorEdit4) + "_2");
            var controlId4 = GetControlId(nameof(ColorEdit4) + "_3");

            var fieldWidth = ControlWidth / 4 - 4;
            GUILayout.BeginHorizontal(GUILayout.Width(ControlWidth + LabelWidth), GUILayout.Height(ControlHeight));

            var r = c.r * 255f;
            ImGuiInternal.InternalFloatField(controlId1, string.Empty, ref r, fieldWidth);
            c.r = r / 255f;

            var g = c.g * 255f;
            ImGuiInternal.InternalFloatField(controlId2, string.Empty, ref g, fieldWidth);
            c.g = g / 255f;

            var b = c.b * 255f;
            ImGuiInternal.InternalFloatField(controlId3, string.Empty, ref b, fieldWidth);
            c.b = b / 255f;

            var a = c.a * 255f;
            ImGuiInternal.InternalFloatField(controlId4, string.Empty, ref a, fieldWidth);
            c.a = a / 255f;

            ImGuiInternal.ControlLabel(label, controlId1);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Apply default dark style colors.
        /// </summary>
        public static void StyleColorsDark() => StyleColors(EnhancedGUIManager.Instance.DefaultDarkSkin);

        /// <summary>
        ///     Apply your style colors.
        /// </summary>
        public static void StyleColors([NotNull] EnhancedGUISkin skin)
        {
            if (skin == null) throw new ArgumentNullException(nameof(skin));
            Renderer.ActiveSkin = skin;

            // apply
            ApplyStyle();
        }

        /// <summary>
        ///     Returns True if last window content is/or was drawn on screen.
        /// </summary>
        public static bool IsContentDrawn() => LastWindow.IsContentActive && LastWindow.IsWindowOpen;

        /// <summary>
        ///     Apply currently active style.
        /// </summary>
        internal static void ApplyStyle()
        {
            if (Renderer.ActiveSkin == null)
            {
                Debug.LogError("No active skin detected!");
                return;
            }

            GUI.skin = Renderer.ActiveSkin.BaseSkinReference;
        }

        /// <summary>
        ///     Clamps rect position to the screen one.
        /// </summary>
        internal static void ClampRectToScreen(ref Rect rect, Rect screen)
        {
            if (rect.x < screen.x)
                rect.x = screen.x;
            if (rect.y < screen.y)
                rect.y = screen.y;

            if (rect.x + rect.width > screen.width)
                rect.x = screen.width - rect.width;
            if (rect.y + rect.height > screen.height)
                rect.y = screen.height - rect.height;
        }

        /// <summary>
        ///     Clamps size of rect.
        /// </summary>
        /// <remarks>It currently only can clamp rect size to screen size.</remarks>
        internal static void ClampRectSize(ref Rect rect)
        {
            const float minSize = 48;
            if (rect.height < minSize)
                rect.height = minSize;
            if (rect.width < minSize)
                rect.width = minSize;

            if (rect.height > Screen.height - minSize)
                rect.height = Screen.height - minSize;
            if (rect.width > Screen.width - minSize)
                rect.width = Screen.width - minSize;
        }

        /// <summary>
        ///     Ends drawn windows.
        /// </summary>
        /// <remarks>Called right after the OnEnhancedGUI method.</remarks>
        internal static void EndWindows()
        {
            Profiler.BeginSample("ImGui.EndWindows");
            if (Renderer.WindowsIndex < Renderer.Windows.Length)
            {
                var newWindows = new List<EnhancedGUIWindow>(Renderer.Windows);
                newWindows.RemoveRange(Renderer.WindowsIndex, Renderer.Windows.Length - Renderer.WindowsIndex);
                Renderer.Windows = newWindows.ToArray();
            }

            Renderer.WindowsIndex = 0;
            Profiler.EndSample();
        }

        /// <summary>
        ///     Ends current frame.
        /// </summary>
        /// <remarks>Called at the end of last GUIRenderer.</remarks>
        internal static void EndFrame()
        {
            // restore default!
            LastWindow = default(EnhancedGUIWindow);
            DrawControlId = false;
            ControlWidth = 140;

            if (_cursorIcon != null && _frame > _drawCursorIcon)
                SetCursor(null);

            foreach (var r in EnhancedGUIRenderer.Renderers)
            {
                r.CanBeginWindow = true;
                r.CanDrawControl = false;
            }

            _frame++;
            _nextWindowIsInactive = false;
            _isFirstRect = true;
        }

        /// <summary>
        ///     Get rect for next window.
        /// </summary>
        internal static Rect NextWindow(string name, out bool isContentActive, out bool isWindowOpen, out int depth, out string guid)
        {
            EnhancedGUIWindow window;
            if (Renderer.WindowsIndex >= Renderer.Windows.Length)
            {
                if (!EnhancedGUISave.GetWindowRect(name, out var rect, out var active))
                {
                    rect = new Rect(LastWindow.Rect.x + 20f, LastWindow.Rect.y + 20f, 300f, 200f);
                    isContentActive = true;
                }
                else isContentActive = active;

                window = LastWindow = new EnhancedGUIWindow(Guid.NewGuid().ToString(), name)
                {
                    Rect = rect,
                    Depth = _isFirstRect ? 0 : Renderer.WindowsIndex + 1
                };

                var list = new List<EnhancedGUIWindow>(Renderer.Windows) {window};
                Renderer.Windows = list.ToArray();
                Renderer.WindowsIndex++;
            }
            else
            {
                window = LastWindow = Renderer.Windows[Renderer.WindowsIndex];
                Renderer.WindowsIndex++;
                isContentActive = window.IsContentActive;
            }

            depth = window.Depth;
            guid = window.Guid;
            isWindowOpen = window.IsWindowOpen;

            _isFirstRect = false;
            return window.Rect;
        }

        /// <summary>
        ///     Push window rect and state.
        /// </summary>
        internal static void PushWindow(Rect rect, Rect header, bool isContentActive, bool isWindowOpen)
        {
            var i = Renderer.WindowsIndex - 1;
            if (i >= Renderer.Windows.Length)
            {
                return;
            }

            Renderer.Windows[i].Rect = rect;
            Renderer.Windows[i].Header = header;
            Renderer.Windows[i].IsContentActive = isContentActive;
            Renderer.Windows[i].IsWindowOpen = isWindowOpen;
        }

        /// <summary>
        ///     Push depth of currently drawn window to top.
        /// </summary>
        internal static void PushDepth()
        {
            var i = Renderer.WindowsIndex - 1;
            if (i >= Renderer.Windows.Length)
                return;

            if (Renderer.Windows[i].Depth == 0)
                return; // already on top

            foreach (var r in EnhancedGUIRenderer.Renderers)
                r.MoveDepth();

            Renderer.Windows[i].Depth = 0;
        }

        /// <summary>
        ///     Checks if any control is currently under the cursor and if so, sets the texture to 'Pointer'.
        /// </summary>
        internal static void CheckControlAndDrawPointer()
        {
            if (GUI.depth != 0)
                return; // only if last control is on top?

            if (IsControlUnderMouse())
            {
                SetCursor(Renderer.ActiveSkin.CursorPoint);
            }
        }

        /// <summary>
        ///     Sets cursor icon for next frame.
        /// </summary>
        internal static void SetCursor(Texture2D cursorTex)
        {
            _cursorIcon = cursorTex;
            if (cursorTex != null)  _drawCursorIcon = _frame + 3;     
        }

        /// <summary/>
        internal static void DrawCursor() => Cursor.SetCursor(_cursorIcon, Vector2.zero, CursorMode.Auto);
        
        /// <summary>
        ///     Checks if any control can be currently drawn.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        internal static bool CheckControlDraw()
        {
            if (!Renderer.CanDrawControl)
                throw new InvalidOperationException("You are trying to draw control outside a window.");

            return !_nextWindowIsInactive;
        }

        /// <summary>
        ///     Get ID of last control.
        /// </summary>
        internal static int GetControlId(string hint)
        {
            if (_nextWindowIsInactive)
                return -1;

            return GUIUtility.GetControlID(hint.GetHashCode(), FocusType.Keyboard, GUILayoutUtility.GetLastRect()) + 1;
        }

        /// <summary>
        ///     Checks is mouse is under last drawn control.
        /// </summary>
        internal static bool IsControlUnderMouse()
        {
            var controlRect = GUILayoutUtility.GetLastRect();
            return controlRect.Contains(Event.current.mousePosition);
        }

        /// <summary>
        ///     It checks if current depth is equals zero.
        /// </summary>
        /// <remarks>User can only interact with currently rendered controls if depth is set to zero (on top of all).</remarks>
        internal static bool CanInteract() => GUI.depth == 0;

        /// <summary>
        ///     Width of next drawn control.
        /// </summary>
        public static float ControlWidth = 140;

        /// <summary>
        ///     Height of next drawn control.
        /// </summary>
        public static float ControlHeight = 25;

        /// <summary>
        ///     Width of next drawn label.
        /// </summary>
        public static float LabelWidth = 50;

        /// <summary>
        ///     (debug) If true, draws controlId of next control.
        /// </summary>
        public static bool DrawControlId { get; set; } = false;

        /// <summary/>
        internal static EnhancedGUIWindow LastWindow { get; set; }

        /// <summary/>
        internal static EnhancedGUIRenderer Renderer { get; set; }

        /// <summary/>
        private static Texture2D _cursorIcon;
        private static int _drawCursorIcon;

        private static bool _nextWindowIsInactive = false;
        private static bool _isFirstRect = true;
        private static int _frame;
    }
}
