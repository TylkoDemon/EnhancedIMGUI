//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI.Test
{
    [DefaultExecutionOrder(10)]
    internal class Test2 : EnhancedGUIRenderer
    {
        internal static bool _isWindowActive1 = true;
        internal static bool _isWindowContentDrawn = false;

        private string _someStr1;

        private void OnEnhancedGUI()
        {
            ImGui.StyleColorsDark();

            ImGui.Begin("Hello Nr. 2, EnhancedIMGUI!", ref _isWindowActive1);
            ImGui.Text("Hello World!");
            if (ImGui.Button("Save"))
            {

            }
            ImGui.InputText("string", ref _someStr1);

            _isWindowContentDrawn = ImGui.IsContentDrawn();
            ImGui.End();
        }
    }
}
