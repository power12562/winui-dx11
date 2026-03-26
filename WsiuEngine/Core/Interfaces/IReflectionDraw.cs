using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsiuRenderer;

namespace WsiuEngine.Core.Interfaces
{
    /// <summary>
    /// 리플렉션 기반 UI 렌더링 시 커스텀 드로우 로직을 제공합니다.
    /// </summary>
    /// <remarks>
    /// 기본 드로우를 무시하고 ImGui 레이아웃을 직접 제어할 때 사용합니다. <br/>
    /// 맴버 함수 오염을 방지하기 위해 <b>명시적 인터페이스 구현</b>을 지향합니다.
    /// </remarks>
    public interface IReflectionDrawer
    {
        Boolean UseCustomDrawing { get; }

        void DrawFields(ImguiContext context, string name, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes);
    }
}
