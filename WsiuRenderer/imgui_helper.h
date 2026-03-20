#pragma once
#include <stdint.h>
#include <imgui.h>
namespace ImGuiHelper
{
    ImGuiKey VirtualKeyToImGuiKey(WPARAM wParam);

    template <typename _format_type>
    constexpr const char* GetImguiOutputFormat()
    {
        using format_type = _format_type;
        // 부동 소수점
        if constexpr (std::is_same_v<format_type, float>)
            return "%.3f";
        else if constexpr (std::is_same_v<format_type, double>)
            return "%.3lf";

        // 8비트 정수 (char / byte)
        else if constexpr (std::is_same_v<format_type, char>)
            return "%d";
        else if constexpr (std::is_same_v<format_type, unsigned char>)
            return "%u";

        // 16비트 정수 (short)
        else if constexpr (std::is_same_v<format_type, short>)
            return "%hd";
        else if constexpr (std::is_same_v<format_type, unsigned short>)
            return "%hu";

        // 32비트 정수 (int)
        else if constexpr (std::is_same_v<format_type, int>)
            return "%d";
        else if constexpr (std::is_same_v<format_type, unsigned int>)
            return "%u";

        // 64비트 정수 (long long)
        else if constexpr (std::is_same_v<format_type, long long>)
            return "%lld";
        else if constexpr (std::is_same_v<format_type, unsigned long long>)
            return "%llu";

        else
            static_assert(false, "Unsupported type.");
    }
    constexpr const char* format_float     = GetImguiOutputFormat<float>();
    constexpr const char* format_double    = GetImguiOutputFormat<double>();
    constexpr const char* format_char      = GetImguiOutputFormat<char>();
    constexpr const char* format_uchar     = GetImguiOutputFormat<unsigned char>();
    constexpr const char* format_short     = GetImguiOutputFormat<short>();
    constexpr const char* format_ushort    = GetImguiOutputFormat<unsigned short>();
    constexpr const char* format_int       = GetImguiOutputFormat<int>();
    constexpr const char* format_uint      = GetImguiOutputFormat<unsigned int>();
    constexpr const char* format_longlong  = GetImguiOutputFormat<long long>();
    constexpr const char* format_ulonglong = GetImguiOutputFormat<unsigned long long>();
    constexpr const char* format_int16     = GetImguiOutputFormat<int16_t>();
    constexpr const char* format_uint16    = GetImguiOutputFormat<uint16_t>();
    constexpr const char* format_int32     = GetImguiOutputFormat<int32_t>();
    constexpr const char* format_uint32    = GetImguiOutputFormat<uint32_t>();
    constexpr const char* format_int64     = GetImguiOutputFormat<int64_t>();
    constexpr const char* format_uint64    = GetImguiOutputFormat<uint64_t>();

    template <typename _scala_type> 
    constexpr ImGuiDataType GetImGuiDataType()
    {
        if constexpr (std::is_same_v<float, _scala_type>)
            return ImGuiDataType_Float;
        else if constexpr (std::is_same_v<double, _scala_type>)
            return ImGuiDataType_Double;
        else if constexpr (std::is_same_v<int16_t, _scala_type>)

            return ImGuiDataType_S16;
        else if constexpr (std::is_same_v<uint16_t, _scala_type>)
            return ImGuiDataType_U16;
        else if constexpr (std::is_same_v<int32_t, _scala_type>)

            return ImGuiDataType_S32;
        else if constexpr (std::is_same_v<uint32_t, _scala_type>)
            return ImGuiDataType_U32;

        else if constexpr (std::is_same_v<int64_t, _scala_type>)
            return ImGuiDataType_S64;
        else if constexpr (std::is_same_v<uint64_t, _scala_type>)
            return ImGuiDataType_U64;

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
