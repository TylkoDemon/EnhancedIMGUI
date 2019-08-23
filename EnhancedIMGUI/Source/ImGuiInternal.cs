//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace EnhancedIMGUI
{
    /// <summary/>
    internal static class ImGuiInternal
    {
        /// <summary/>
        internal static void ControlLabel(string str, int controlId)
        {
            ImGui.CheckControlDraw();
            if (ImGui.DrawControlId)
                str += $" ({controlId})";

            GUILayout.Label(str, Renderer.ActiveSkin.LabelText, GUILayout.Width(ImGui.LabelWidth));
        }

        /// <summary/>
        internal static void InternalToggle(int controlId, string label, ref bool b, float controlWidth)
        {
            ImGui.CheckControlDraw();
            GUILayout.BeginHorizontal(GUILayout.Width(controlWidth + LabelWidth), GUILayout.Height(ControlHeight));
            GUILayout.FlexibleSpace();
            GUI.SetNextControlName(controlId.ToString());
            b = GUILayout.Toggle(b, string.Empty);
            ImGui.CheckControlAndDrawPointer();
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();
        }

        /// <summary/>
        internal static void InternalSlider(int controlId, string label, ref float f, float min, float max, bool fullNumbers, float controlWidth)
        {
            ImGui.CheckControlDraw();
            GUILayout.BeginHorizontal(GUILayout.Width(controlWidth + LabelWidth), GUILayout.Height(ControlHeight));
            GUI.SetNextControlName(controlId.ToString());
            f = GUILayout.HorizontalSlider(f, min, max, GUILayout.Width(controlWidth));
            ImGui.CheckControlAndDrawPointer();
            if (fullNumbers)
                f = (int) f;
            var rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, fullNumbers ?$"{f:0}": $"{f:0.000}", Renderer.ActiveSkin.SliderText);
            ControlLabel(label, controlId);
            GUILayout.EndHorizontal();
        }

        /// <summary/>
        internal static void InternalInputText(int controlId, string label, ref string input, float controlWidth)
        {
            ImGui.CheckControlDraw();
            var isLabel = !string.IsNullOrEmpty(label);
            if (isLabel)
                GUILayout.BeginHorizontal(GUILayout.Width(controlWidth + LabelWidth), GUILayout.Height(ControlHeight));

            GUI.SetNextControlName(controlId.ToString());
            var style = new GUIStyle(GUI.skin.textField) { alignment = TextAnchor.MiddleLeft }; // default string input should have text to left 
            if (isLabel)
                input = GUILayout.TextField(input, style, GUILayout.ExpandWidth(true), GUILayout.Height(ControlHeight));
            else
            {
                input = GUILayout.TextField(input, style, GUILayout.Width(controlWidth), GUILayout.Height(ControlHeight));
            }
            ImGui.CheckControlAndDrawPointer();

            if (isLabel)
            {
                ControlLabel(label, controlId);
                GUILayout.EndHorizontal();
            }
        }

        /// <summary/>
        internal static void InternalFloatField(int controlId, string label, ref float f, float controlWidth)
        {
            const NumberStyles fieldStyle =
                NumberStyles.Float | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
            var cultureInfo = CultureInfo.InvariantCulture;

            ImGui.CheckControlDraw();
            var originalStr = f.ToString(CultureInfo.InvariantCulture);
            InternalDoNumberField(label, controlId, ref originalStr, controlWidth, result =>
            {
                result = result.Replace(',', '.');
                if (result.Split('.').Length > 2) result = result.Remove(result.IndexOf('.'));
                float.TryParse(result, fieldStyle, cultureInfo, out var f2);
                if (float.IsNaN(f2) || float.IsInfinity(f2))
                    f2 = 0f;

                return f2.ToString(CultureInfo.InvariantCulture);
            });
            f = float.Parse(originalStr, fieldStyle, cultureInfo);
        }

        /// <summary/>
        internal static void InternalIntField(int controlId, string label, ref int i, float controlWidth)
        {
            const NumberStyles fieldStyle = NumberStyles.Integer;
            var cultureInfo = CultureInfo.InvariantCulture;

            ImGui.CheckControlDraw();
            var originalStr = i.ToString();
            InternalDoNumberField(label, controlId, ref originalStr, controlWidth, result =>
            {
                result = result.Replace(',', '.');
                if (result.Split('.').Length > 2) result = result.Remove(result.IndexOf('.'));
                int.TryParse(result, fieldStyle, cultureInfo, out var i2);
                return i2.ToString();
            });
            i = int.Parse(originalStr, fieldStyle, cultureInfo);
        }

        /// <summary/>
        private static void InternalDoNumberField(string label, int controlId, ref string originalStr, float controlWidth, Func<string, string> parse)
        {
            var isLabel = !string.IsNullOrEmpty(label);

            if (isLabel)
                GUILayout.BeginHorizontal(GUILayout.Width(controlWidth + LabelWidth), GUILayout.Height(ControlHeight));

            if (!NumberFields.ContainsKey(controlId))
                NumberFields.Add(controlId, originalStr);

            GUI.SetNextControlName(controlId.ToString());
            if (isLabel)
                NumberFields[controlId] = GUILayout.TextField(NumberFields[controlId], GUILayout.ExpandWidth(true), GUILayout.Height(ControlHeight));
            else
                NumberFields[controlId] = GUILayout.TextField(NumberFields[controlId], GUILayout.Width(controlWidth), GUILayout.Height(ControlHeight));
            ImGui.CheckControlAndDrawPointer();

            if (isLabel)
            {
                ControlLabel(label, controlId);
                GUILayout.EndHorizontal();
            }

            var incomingControl = GUI.GetNameOfFocusedControl();
            if (_selectedNumberField != _nextNumberField || Event.current.isKey && Event.current.keyCode == KeyCode.Return)
            {
                if (_nextNumberField == controlId.ToString() || controlId.ToString() == "0")
                {
                    if (controlId.ToString() == "0")
                    {
                        try
                        {
                            controlId = Convert.ToInt32(_nextNumberField);
                        }
                        catch (Exception)
                        {
                            controlId = 0;
                        }
                    }

                    if (NumberFields.ContainsKey(controlId))
                    {
                        var str = NumberFields[controlId];
                        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                            originalStr = "0";
                        else
                        {
                            originalStr = parse.Invoke(str);
                        }

                        NumberFields.Remove(controlId);
                    }
                }
            }

            _nextNumberField = _selectedNumberField;
            _selectedNumberField = incomingControl;
        }

        /// <summary/>
        internal static float ControlWidth => ImGui.ControlWidth;

        /// <summary/>
        internal static float ControlHeight => ImGui.ControlHeight;

        /// <summary/>
        internal static float LabelWidth => ImGui.LabelWidth;

        /// <summary/>
        internal static EnhancedGUIRenderer Renderer => ImGui.Renderer;

        private static readonly Dictionary<int, string> NumberFields = new Dictionary<int, string>();
        private static string _nextNumberField;
        private static string _selectedNumberField;
    }
}
