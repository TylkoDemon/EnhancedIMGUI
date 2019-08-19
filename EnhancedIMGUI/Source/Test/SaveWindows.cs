//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI.Test
{
    /// <inheritdoc />
    /// <summary>
    ///     Example script of how to save and load windows data.
    ///     This types of scripts need to have set execution order lower than every window you have. (This is why Test2 and Test script has exec order set to 10)
    /// </summary>
    internal class SaveWindows : MonoBehaviour
    {
        private void Awake() => EnhancedGUISave.LoadSave();
        private void OnApplicationQuit() => EnhancedGUISave.WriteSave();
    }
}
