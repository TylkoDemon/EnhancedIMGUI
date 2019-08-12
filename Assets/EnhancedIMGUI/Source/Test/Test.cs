//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

namespace EnhancedIMGUI.Test
{
    public class Test : EnhancedGUIRenderer
    {
        private bool _isActive1;
        private string _someStr1;
        private float _someFloat1;
        private float _someFloat2 = 4.4f;
        private int _someInt1;

        private void OnEnhancedGUI()
        {
            ImGui.DrawControlId = true;
            ImGui.StyleColorsDark();

            ImGui.Begin("Hello, EnhancedIMGUI!", ref _isActive1);
            {
                ImGui.Text("Hello World!");
                if (ImGui.Button("Save"))
                {

                }

                ImGui.InputText("string", ref _someStr1);
                ImGui.SliderFloat("float", ref _someFloat1, 0f, 1f);
                ImGui.SliderInt("int", ref _someInt1, 0, 10);
                ImGui.FloatField("float1", ref _someFloat2);
            }
            ImGui.End();
        }
    }
}
