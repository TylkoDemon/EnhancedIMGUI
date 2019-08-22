//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// disable xml doc warn
#pragma warning disable 1591

namespace EnhancedIMGUI
{
    /// <summary>
    ///     EnhancedGUI Save.
    ///     Contains data for all enhanced GUI elements.
    /// </summary>
    [Serializable]
    public class EnhancedGUISave
    {
        [Serializable]
        public class SerializableWindow
        {
            public string Name;
            public float X, Y, Width, Height;
            public bool IsActive;

            internal bool Ready = true;
        }

        /// <summary>
        ///     List of saved/loaded windows.
        /// </summary>
        public List<SerializableWindow> Windows = new List<SerializableWindow>();

        private SerializableWindow GetWindowData(string windowName)
        {
            foreach (var w in Windows)
            {
                if (w.Name == windowName)
                    return w;
            }

            return null;
        }

        internal static bool GetWindowRect(string windowName, out Rect r, out bool isActive)
        {
            r = new Rect();
            isActive = false;
            if (Loaded == null)
                return false;

            foreach (var w in Loaded.Windows)
            {
                if (!w.Ready)
                    continue;

                if (w.Name == windowName)
                {
                    w.Ready = false;
                    r = new Rect(w.X, w.Y, w.Width, w.Height);
                    isActive = w.IsActive;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Load the save.
        /// </summary>
        public static void LoadSave()
        {
            if (!File.Exists(SaveFileName))
                Loaded = new EnhancedGUISave();
            else Loaded = JsonUtility.FromJson<EnhancedGUISave>(File.ReadAllText(SaveFileName));
        }

        /// <summary>
        ///     Write the save file.
        /// </summary>
        public static void WriteSave()
        {
            if (Loaded == null) Loaded = new EnhancedGUISave();
            foreach (var renderer in EnhancedGUIRenderer.Renderers)
            {
                foreach (var window in renderer.Windows)
                {
                    var data = Loaded.GetWindowData(window.Name);
                    if (data == null)
                    {
                        data = new SerializableWindow();
                        Loaded.Windows.Add(data);
                    }

                    data.Name = window.Name;
                    data.Ready = true;
                    data.X = window.Rect.x;
                    data.Y = window.Rect.y;
                    data.Height = window.Rect.height;
                    data.Width = window.Rect.width;
                    data.IsActive = window.IsActive;
                }
            }

            File.WriteAllText(SaveFileName, JsonUtility.ToJson(Loaded, true));
        }

        /// <summary>
        ///     Loaded data of GUI windows.
        /// </summary>
        internal static EnhancedGUISave Loaded { get; private set; }

        /// <summary>
        ///     JSON file name of enhancedGUISave data.
        /// </summary>
        public static string SaveFileName = "enhancedIMGUI.json";
    }
}
