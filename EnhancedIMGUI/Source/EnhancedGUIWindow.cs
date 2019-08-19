//
// Enhanced IMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using UnityEngine;

namespace EnhancedIMGUI
{
    internal struct EnhancedGUIWindow
    {
        public string Guid { get; }
        public string Name { get; }
        public Rect Rect { get; set; }
        public Rect Header { get; set; }
        public Rect Content { get; set; }
        public Rect Resize { get; set; }
        public int Depth { get; set; }
        public bool IsActive { get; set; }

        public EnhancedGUIWindow(string guid, string name)
        {
            Guid = guid;
            Name = name;
            Rect = default(Rect);
            Header = default(Rect);
            Content = default(Rect);
            Resize = default(Rect);
            Depth = 0;
            IsActive = false;
        }
    }
}