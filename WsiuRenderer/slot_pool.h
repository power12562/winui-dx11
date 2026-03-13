#pragma once
#include <vector>

template <typename _value_type, typename _cleaner_type> 
class slot_pool
{
    using value_type   = _value_type;
    using cleaner_type = _cleaner_type;
    using element_type = std::pair<value_type, bool>;
public:

    template <typename __value_type> 
    size_t create(__value_type&& value)
    {
        if (_freeIndices.empty() == false)
        {
            size_t id = _freeIndices.back();
            auto& [val, isValid] = _elements[id];
            val = std::forward<__value_type>(value);
            isValid = true;
            return id;
        }
        else
        {
            size_t id = _elements.size();
            _elements.emplace_back(std::forward<__value_type>(value), true);
            return id;
        }  
    }
    void reserve(size_t newCapacity)
    {
        _elements.reserve(newCapacity);
        _freeIndices.reserve(newCapacity);
    }
    void erase(size_t index) 
    {
        if (index < _elements.size())
        {
            auto& [value, isValid] = _elements[index];
            if (isValid)
            {
                _cleaner(value);
                isValid = false;
                _freeIndices.push_back(index);
            }
        }
    }
    value_type& at(size_t index)
    {
        if (index < _elements.size() == false)
            throw std::out_of_range("out of index!");

        auto& [value, isValid] = _elements[index];
        if (isValid == false)
            throw std::invalid_argument("invalid indexe argument!");

        return value;
    }
    const value_type& at(size_t index) const 
    {
        if (index < _elements.size() == false)
            throw std::out_of_range("out of index!");

        auto& [value, isValid] = _elements[index];
        if (isValid == false)
            throw std::invalid_argument("invalid indexe argument!");

        return value;
    }
    
private:
    std::vector<element_type> _elements;
    std::vector<size_t>       _freeIndices;
    cleaner_type              _cleaner;

};