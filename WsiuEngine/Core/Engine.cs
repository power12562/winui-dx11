using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using WsiuEngine.Core.Interfaces;
using WsiuEngine.Core.System;
using WsiuRenderer;

namespace WsiuEngine.Core
{
    public class Engine
    {
        private static Engine instance = null!;
        private readonly EngineCore _engineCore;

        public EngineCore EngineCore { get { return _engineCore; } }

        public Engine(nint hwnd, SwapChainPanel enginePanel)
        {     
            if (instance != null) 
                throw new InvalidOperationException("Engine is already initialized!");
            instance = this;
            
            _engineCore = new EngineCore();
            _engineCore.Initialize((ulong)hwnd, enginePanel);

            InputSystem.Initialize(_engineCore);
            Time.Initialize();
            Screen.Initialize(hwnd);
        }

        public void Update()
        {
            Time.Update();
            _engineCore.BeginFrame();
            InputSystem.Update();
            _engineCore.Tick();
            _engineCore.EndFrame();
        }

        private readonly static Dictionary<Type, ISingleton> typeToSingletonInstance = [];
        internal static void RegisterSingleton(ISingleton instance)
        {
            Type type = instance.GetType();
            if (typeToSingletonInstance.ContainsKey(type))
                throw new InvalidOperationException($"{type.Name} is already registered!");

            typeToSingletonInstance.Add(type, instance);
        }

        public static TSingleton GetSingleton<TSingleton>() where TSingleton : ISingleton
        {
            Type type = typeof(TSingleton);
            if (typeToSingletonInstance.TryGetValue(type, out ISingleton? instance) == false)
                throw new InvalidOperationException($"{type.Name} is not registered!");

            return (TSingleton)instance;
        }
    }
}
