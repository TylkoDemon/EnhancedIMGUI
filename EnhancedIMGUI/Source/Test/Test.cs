//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI.Test
{
    [DefaultExecutionOrder(10)]
    internal class Test : EnhancedGUIRenderer
    {
        private bool _isActive1;
        private string _someStr1;

        private float _someFloat1;
        private float _someFloat2 = 4.4f;
        private float _controlsWidth = 140;
        private float _labelWidth = 50f;

        private int _someInt1;
        private int _someInt2;

        private bool _someBool1;

        private Color _color1;

        private void OnEnhancedGUI()
        {
            ImGui.DrawControlId = true;
            ImGui.StyleColorsDark();

            ImGui.Begin("Hello, EnhancedIMGUI!", ref _isActive1);
            {
                ImGui.Text("Hello World!");
                ImGui.SliderFloat("Controls Width", ref _controlsWidth, 100, 400);
                ImGui.ControlWidth = _controlsWidth;
                ImGui.SliderFloat("Label Width", ref _labelWidth, 10, 200);
                ImGui.LabelWidth = _labelWidth;

                if (ImGui.Button("Save"))
                {

                }

                ImGui.InputText("string", ref _someStr1);

                ImGui.SliderFloat("float1", ref _someFloat1, 0f, 1f);
                ImGui.FloatField("float2", ref _someFloat2);

                ImGui.SliderInt("int1", ref _someInt1, 0, 10);
                ImGui.IntField("int2", ref _someInt2);

                ImGui.Toggle("bool", ref _someBool1);

                ImGui.ColorEdit4("color4", ref _color1);
            }
            ImGui.End();
        }
    }
}
