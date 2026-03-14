#pragma once

template<typename _value_type>
struct ComPtrCleaner
{
    using value_type = _value_type;
    void operator()(ComPtr<value_type>& value) { value.Reset(); }
};

template <typename _value_type> 
struct UniquePtrCleaner
{
    using value_type = _value_type;
    void operator()(std::unique_ptr<value_type>& value) { value.reset(); }
};