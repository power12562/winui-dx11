#pragma once
#include <imgui.h>
namespace ImGuiHelper
{
    ImGuiKey VirtualKeyToImGuiKey(WPARAM wParam);

    template <typename _scala_type> constexpr ImGuiDataType GetImGuiDataType()
    {
        if constexpr (std::is_same_v<float, _scala_type>)
            return ImGuiDataType_Float;
        else if constexpr (std::is_same_v<double, _scala_type>)
            return ImGuiDataType_Double;
        else
            static_assert(false, "Unsupported type.");
    }

    template <typename _scala_type, typename _callback_type>
    void DragScalaWithCallback(const char* label, const _callback_type& callback, _scala_type* val, float speed,
                                _scala_type* min, _scala_type* max, const char* format, ImGuiSliderFlags flags)
    {
        constexpr ImGuiDataType type = GetImGuiDataType<_scala_type>();
        if (ImGui::DragScalarN(label, type, val, 1, speed, min, max, format, flags))
        {
            callback(*val);
        }
    }

    template <typename _scala_type, typename _callback_type>
    void DragScalaNWithCallback(const char* label, const _callback_type& callback, int n, _scala_type* val, float speed,
                               _scala_type* min, _scala_type* max, const char* format, ImGuiSliderFlags flags)
    {
        constexpr ImGuiDataType type = GetImGuiDataType<_scala_type>();
        if (ImGui::DragScalarN(label, type, val, n, speed, min, max, format, flags))
        {      
            callback(winrt::array_view<_scala_type const>(val, val + n));
        }
    }
}
