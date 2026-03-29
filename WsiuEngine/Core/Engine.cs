using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using WsiuEngine.Core.Interfaces;
using WsiuEngine.Core.System;
using WsiuRenderer;
using WsiuApplication = WsiuEngine.Core.System.Application;

namespace WsiuEngine.Core
{
    public class Engine : IDisposable
    {
        private static Engine instance = null!;
        private readonly EngineCore _engineCore;

        public EngineCore EngineCore { get { return _engineCore; } }

        public Engine(Window mainWindow, SwapChainPanel enginePanel)
        {
            if (instance != null)
                throw new InvalidOperationException("Engine is already initialized!");
            instance = this;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(mainWindow);
            _engineCore = new EngineCore();
            _engineCore.Initialize((ulong)hwnd, enginePanel);
            InputSystem.Initialize(_engineCore);
            Time.Initialize();
            Screen.Initialize(hwnd);
            WindowService.Initialize(mainWindow);
            TaskDispatcher.Initialize();
            WsiuApplication.Initialize();
            Log.Initialize();
        }

        public void Update()
        {
            Time.Update();                         // 델타타임 갱신
            _engineCore.BeginFrame();              // 렌더링 프레임 시작 백버퍼 초기화
            InputSystem.Update();                  // 입력 상태 갱신 및 이벤트 호출
            TaskDispatcher.DispatchPendingTasks(); // 완료된 Tasks 처리

            _engineCore.Tick();                    // 렌더링 드로우콜 실행
            _engineCore.EndFrame();                // 렌더링 프레임 종료 및 백버퍼 Flip
        }

        public void Dispose()
        {
            Log.Shutdown();
            GC.SuppressFinalize(this);
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
