//
// EnhancedIMGUI Source
//
// Copyright (c) 2019 ADAM MAJCHEREK ALL RIGHTS RESERVED
//

using JetBrains.Annotations;
using System;
using System.Reflection;

namespace EnhancedIMGUI
{
    internal class SmartMethod
    {
        private readonly object _classObj;
        private readonly string _methodName;
        private readonly MethodInfo _methodInfo;

        public SmartMethod([NotNull] object classObj, string methodName)
        {
            _classObj = classObj ?? throw new ArgumentNullException(nameof(classObj));
            _methodName = methodName;
            _methodInfo = classObj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        }

        public bool IsValid()
        {
            return _methodInfo != null;
        }

        public object Invoke()
        {
            return Invoke(null);
        }

        public object Invoke(params object[] parameters) => _methodInfo?.Invoke(_classObj, parameters);      
    }
}
