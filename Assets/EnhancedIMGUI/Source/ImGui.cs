//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace EnhancedIMGUI
{
    public static class ImGui
    {
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
            var originalRect = NextWindow(out var depth, out var guid);
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

        public static void End()
        {
            if (Renderer.CanBeginWindow)
                throw new InvalidOperationException("You are calling End() without calling Begin().");

            Renderer.CanDrawControl = false;
            Renderer.CanBeginWindow = true;

            GUI.enabled = true;
            GUILayout.EndArea();
        }

        public static void Text(string str)
        {
            CheckControlDraw();
            GUILayout.Label(str);
        }

        internal static void ControlLabel(string str, int controlId)
        {
            CheckControlDraw();
            if (DrawControlId)
                str += $" ({controlId})";

            GUILayout.Label(str);
        }

        public static bool Button(string str)
        {
            CheckControlDraw();
            var controlId = GetControlID(nameof(FloatField));

            GUI.SetNextControlName(controlId.ToString());
            if (DrawControlId)
                str += $" ({controlId})";
            return GUILayout.Button(str);
        }

        public static void Toggle(string label, ref bool b)
        {
            CheckControlDraw();
            b = GUILayout.Toggle(b, label);
        }

        public static void InputText(string label, ref string input)
        {
            CheckControlDraw();
            var controlId = GetControlID(nameof(FloatField));
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(controlId.ToString());
            input = GUILayout.TextField(input);
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();
        }

        private static readonly Dictionary<int, string> FloatFields = new Dictionary<int, string>();
        private static string _nextSelectedInput;
        private static string _selectedInput;

        public static void FloatField(string label, ref float f)
        {
            CheckControlDraw();
            var controlId = GetControlID(nameof(FloatField));

            GUILayout.BeginHorizontal();
            if (!FloatFields.ContainsKey(controlId))
                FloatFields.Add(controlId, f.ToString(CultureInfo.InvariantCulture));

            GUI.SetNextControlName(controlId.ToString());
            FloatFields[controlId] = GUILayout.TextField(FloatFields[controlId]);
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();

            var incomingControl = GUI.GetNameOfFocusedControl();
            if (_selectedInput != _nextSelectedInput || Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                // Debug.Log($"{label} '{_nextSelectedInput}' '{_selectedInput}' '{controlId.ToString()}'");
                if (_nextSelectedInput == controlId.ToString() || controlId.ToString() == "0")
                {
                    if (controlId.ToString() == "0")
                    {
                        // Debug.Log("trying to fix " + _nextSelectedInput);
                        try
                        {
                            controlId = Convert.ToInt32(_nextSelectedInput);
                        }
                        catch (Exception)
                        {
                            controlId = 0;
                        }
                    }

                    // Debug.Log("continue at " + label);
                    if (FloatFields.ContainsKey(controlId))
                    {
                        var str = FloatFields[controlId];
                        bool remove = true;
                        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                            f = 0;
                        else
                        {
                            str = str.Replace(',', '.');
                            if (str.Split('.').Length > 2) str = str.Remove(str.IndexOf('.'));
                            remove = float.TryParse(str,
                                NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                                CultureInfo.InvariantCulture, out f);
                            if (float.IsNaN(f) || float.IsInfinity(f))
                                f = 0f;
                        }

                        if (remove) FloatFields.Remove(controlId);

                        // Debug.Log($"{label} finally updated to {f} from {str}");
                    }
                }
            }

            _nextSelectedInput = _selectedInput;
            _selectedInput = incomingControl;
        }

        public static void SliderFloat(string label, ref float f, float min, float max)
        {
            CheckControlDraw();
            var controlId = GetControlID(nameof(FloatField));
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(controlId.ToString());
            f = GUILayout.HorizontalSlider(f, min, max);
            var rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, $"{f:0.000}", Renderer.ActiveSkin.SliderText);
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();
        }

        public static void SliderInt(string label, ref int i, int min, int max)
        {
            CheckControlDraw();
            var controlId = GetControlID(nameof(FloatField));
            GUILayout.BeginHorizontal();

            GUI.SetNextControlName(controlId.ToString());
            i = (int) GUILayout.HorizontalSlider(i, min, max);
            var rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, $"{i}", Renderer.ActiveSkin.SliderText);
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();
        }

        public static void ColorEdit4(string label, ref Color color)
        {
            CheckControlDraw();
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
        }

        public static void StyleColorsDark() => StyleColors(EnhancedGUIManager.Instance.DefaultDarkSkin);

        public static void StyleColors([NotNull] EnhancedGUISkin skin)
        {
            if (skin == null) throw new ArgumentNullException(nameof(skin));
            Renderer.ActiveSkin = skin;
        }

        internal static void ApplyStyle()
        {
            if (Renderer.ActiveSkin == null)
            {
                Debug.LogWarning("No active skin detected! Setting dark style.");
                StyleColorsDark();
            }

            GUI.skin = Renderer.ActiveSkin.BaseSkinReference;
        }

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

        internal static void EndWindows()
        {
            Profiler.BeginSample("ImGui.EndWindows");
            if (Renderer.WindowsIndex < Renderer.Windows.Length)
            {
                var newWindows = new List<EnhancedGUIWindow>(Renderer.Windows);
                newWindows.RemoveRange(Renderer.WindowsIndex, Renderer.Windows.Length - Renderer.WindowsIndex);
                Renderer.Windows = newWindows.ToArray();
            }

            //Debug.Log($"ending windows while windows index was " + Renderer.WindowsIndex);
            Renderer.WindowsIndex = 0;
            Profiler.EndSample();
        }

        internal static Rect NextWindow(out int depth, out string guid)
        {
            EnhancedGUIWindow window;
            if (Renderer.WindowsIndex >= Renderer.Windows.Length)
            {
                var rect = new Rect(LastWindow.Rect.x + 20f, LastWindow.Rect.y + 20f, 300f, 200f);
                window = LastWindow = new EnhancedGUIWindow(Guid.NewGuid().ToString())
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

            // Debug.Log($"newst window was " + WindowsIndex);
            depth = window.Depth;
            guid = window.Guid;
            return window.Rect;
        }

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

        internal static void PushDepth()
        {
            var i = Renderer.WindowsIndex - 1;
            if (i >= Renderer.Windows.Length)
            {
                // Debug.LogError($"cant push depth for {i}");
                return;
            }

            // Debug.Log("pushing depth for " + i, Renderer);

            if (Renderer.Windows[i].Depth == 0)
                return; // already on top

            foreach (var r in EnhancedGUIRenderer.Renderers)
                r.MoveDepth();

            Renderer.Windows[i].Depth = 0;
        }

        internal static void CheckControlDraw()
        {
            if (!Renderer.CanDrawControl)
                throw new InvalidOperationException("You are trying to draw control outside a window.");
        }

        public static bool DrawControlId { get; set; } = false;

        internal static int GetControlID(string hint) =>
            GUIUtility.GetControlID(hint.GetHashCode(), FocusType.Keyboard, GUILayoutUtility.GetLastRect()) + 1;

        /// <summary>
        ///     User can only interact with currently rendered controls if depth is set to zero (on top of all).
        /// </summary>
        internal static bool CanInteract() => GUI.depth == 0;

        internal static EnhancedGUIWindow LastWindow { get; set; }
        internal static EnhancedGUIRenderer Renderer { get; set; }
    }
}
