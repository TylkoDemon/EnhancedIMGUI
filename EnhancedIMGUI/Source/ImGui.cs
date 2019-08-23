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
        public static int Begin(string windowName, ref bool isActive)
        {
            if (!Renderer.CanBeginWindow)
                throw new InvalidOperationException("You are calling Begin() without first ending previous window by calling End().");

            //
            // CONST
            //
            const int headerHeight = 25;
            const int resizeButtonSize = 20;

            //
            // INIT
            //
            var pushDepth = false;
            var originalRect = NextWindow(windowName, ref isActive, out var depth, out var guid);
            var pushingRect = new Rect(originalRect);
            var header = new Rect(originalRect.x, originalRect.y, originalRect.width, headerHeight);
            GUI.depth = depth;
            var enabled = GUI.depth == 0;

            //
            // DRAW: HEADER
            //
            GUILayout.BeginArea(header, Renderer.ActiveSkin.Header);
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(
                    (isActive ? EnhancedGUISkin.DownwardsArrowChar : EnhancedGUISkin.RightwardsArrowChar).ToString(),
                    isActive ? Renderer.ActiveSkin.FoldoutClose : Renderer.ActiveSkin.FoldoutOpen))
                {
                    isActive = !isActive;
                }

                GUILayout.Label(windowName, Renderer.ActiveSkin.HeaderText);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();

            //
            // TEST RAYCAST: Test what window is on top.
            //
            var e = Event.current;
            EnhancedGUIRenderer.TestRaycast(e.mousePosition, window => window.IsActive ? window.Rect : window.Header, out var hit);

            //
            // INPUT: Window movement.
            //
            if (Renderer.FrameTarget == -1 && hit.Guid == guid)
            {
                if (header.Contains(e.mousePosition))
                {
                    Cursor.SetCursor(EnhancedGUIManager.Instance.DefaultDarkSkin.CursorHand, Vector2.zero, CursorMode.Auto);
                    if (e.type == EventType.MouseDown)
                    {
                        Renderer.IsWindowMoving = true;
                        Renderer.StartRect = originalRect;
                        Renderer.MouseStartPosition = e.mousePosition;

                        Renderer.FrameTarget = Renderer.WindowsIndex;
                        pushDepth = true;
                    }
                }
                else Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);           
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
                isActive ? originalRect.height - header.height : 0);
            GUILayout.BeginArea(windowRect, isActive ? Renderer.ActiveSkin.Window : Renderer.ActiveSkin.Hidden);
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
                    Cursor.SetCursor(EnhancedGUIManager.Instance.DefaultDarkSkin.CursorHand, Vector2.zero,
                        CursorMode.Auto);
                    if (e.type == EventType.MouseDown && Renderer.FrameTarget == -1)
                    {
                        Renderer.IsWindowResize = true;
                        Renderer.StartRect = originalRect;
                        Renderer.MouseStartPosition = e.mousePosition;

                        Renderer.FrameTarget = Renderer.WindowsIndex;
                        pushDepth = true;
                    }
                }
                else
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
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
            PushWindow(pushingRect, header, windowRect, resizeButton, isActive);

            GUI.enabled = enabled;

            Renderer.CanDrawControl = true;
            Renderer.CanBeginWindow = false;

            return Renderer.WindowsIndex;
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
            GUILayout.EndArea();
        }

        /// <summary>
        ///     Draws text.
        /// </summary>
        public static void Text(string str)
        {
            CheckControlDraw();
            GUILayout.Label(str);
        }

        /// <summary>
        ///     Draws a button.
        /// </summary>
        public static bool Button(string str)
        {
            CheckControlDraw();
            var controlId = GetControlId(nameof(Button));

            GUI.SetNextControlName(controlId.ToString());
            if (DrawControlId)
                str += $" ({controlId})";
            return GUILayout.Button(str);
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
        ///     Draws a int slider.
        /// </summary>
        public static void SliderInt(string label, ref int i, int min, int max)
        {
            float f = i;
            ImGuiInternal.InternalSlider(GetControlId(nameof(SliderInt)), label, ref f, min, max, true, ControlWidth);
            i = (int) f;
        }

        /// <summary>
        ///     Draws a Single field.
        /// </summary>
        public static void FloatField(string label, ref float f) => ImGuiInternal.InternalFloatField(GetControlId(nameof(FloatField)), label, ref f, ControlWidth);
        
        /// <summary>
        ///     Draws a Int32 field.
        /// </summary>
        public static void IntField(string label, ref int i) => ImGuiInternal.InternalIntField(GetControlId(nameof(IntField)), label, ref i, ControlWidth);

        /// <summary>
        ///     Draws a color4 field.
        /// </summary>
        /// <remarks>Not fully implemented yet.</remarks>
        public static void ColorEdit4(string label, ref Color c)
        {
            CheckControlDraw();
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
        ///     Get rect for next window.
        /// </summary>
        internal static Rect NextWindow(string name, ref bool isActive, out int depth, out string guid)
        {
            EnhancedGUIWindow window;
            if (Renderer.WindowsIndex >= Renderer.Windows.Length)
            {
                if (!EnhancedGUISave.GetWindowRect(name, out var rect, out bool active))
                    rect = new Rect(LastWindow.Rect.x + 20f, LastWindow.Rect.y + 20f, 300f, 200f);
                else isActive = active;

                window = LastWindow = new EnhancedGUIWindow(Guid.NewGuid().ToString(), name)
                {
                    Rect = rect,
                    Depth = Renderer.WindowsIndex + 1
                };

                var list = new List<EnhancedGUIWindow>(Renderer.Windows) {window};
                Renderer.Windows = list.ToArray();
                Renderer.WindowsIndex++;
            }
            else
            {
                window = LastWindow = Renderer.Windows[Renderer.WindowsIndex];
                Renderer.WindowsIndex++;
            }

            depth = window.Depth;
            guid = window.Guid;
            return window.Rect;
        }

        /// <summary>
        ///     Push window rect and state.
        /// </summary>
        internal static void PushWindow(Rect rect, Rect header, Rect content, Rect resize, bool isActive)
        {
            var i = Renderer.WindowsIndex - 1;
            if (i >= Renderer.Windows.Length)
            {
                return;
            }

            Renderer.Windows[i].Rect = rect;
            Renderer.Windows[i].Header = header;
            Renderer.Windows[i].Content = content;
            Renderer.Windows[i].Resize = resize;
            Renderer.Windows[i].IsActive = isActive;
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
        ///     Checks if any control can be currently drawn.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        internal static void CheckControlDraw()
        {
            if (!Renderer.CanDrawControl)
                throw new InvalidOperationException("You are trying to draw control outside a window.");
        }

        /// <summary>
        ///     Get ID of last control.
        /// </summary>
        internal static int GetControlId(string hint) => GUIUtility.GetControlID(hint.GetHashCode(), FocusType.Keyboard, GUILayoutUtility.GetLastRect()) + 1;

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
    }
}
