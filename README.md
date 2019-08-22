# Enhanced IMGUI

Enhanced IMGUI is a small extension for UnityEngine that improves build-in IMGUI system to look and work more like [Dear ImGui](https://github.com/ocornut/imgui)

## Unity IMGUI Z order problem.

There is one major problem with unity's IMGUI system that we just can't avoid, and this is `GUI.depth`. In short, the way it works is how you expect it to work, and this is to force fixed Z order of next drawn control. In unity however it is only available for objects defined in different `OnGUI()` methods.

What this means for `EnhancedIMGUI` is that you can't define multiple windows areas in one OnGUI method unless you don't want to use depth. 

You can learn more about `GUI.depth` [here](https://docs.unity3d.com/ScriptReference/GUI-depth.html).

## Examples

Full API doc will be available in near future!
For now you can find some help in test scripts that can be found [here](https://github.com/TylkoDemon/EnhancedIMGUI/blob/master/EnhancedIMGUI/Source/Test/Test.cs).

*(c) 2019 Adam Majcherek*
