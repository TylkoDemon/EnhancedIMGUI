//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

namespace EnhancedIMGUI.Test
{
    public class Test2 : EnhancedGUIRenderer
    {
        private bool _isActive1;
        private string _someStr1;

        private void OnEnhancedGUI()
        {
            ImGui.StyleColorsDark();

            ImGui.Begin("Hello Nr. 2, EnhancedIMGUI!", ref _isActive1);
            ImGui.Text("Hello World!");
            if (ImGui.Button("Save"))
            {

            }
            ImGui.InputText("string", ref _someStr1);
            ImGui.End();
        }
    }
}
