#pragma once
#include <sal.h>
#include <vector>
#include "default_cleaner.h"

template <typename _value_type, typename _cleaner_type> 
class slot_pool
{
    using value_type   = _value_type;
    using cleaner_type = _cleaner_type;
    using element_type = std::pair<value_type, bool>;
public:
    slot_pool() = default;
    slot_pool(const cleaner_type& cleaner) : _cleaner(cleaner) {};
    slot_pool(const cleaner_type&& cleaner) noexcept : _cleaner(std::move(cleaner)) {};
    ~slot_pool() 
    {
        for (auto& [value, valid] : _elements)
        {
            if (valid)
            {
                _cleaner(value);
            }
        }
        _elements.clear();
        _freeIndices.clear();
        _beginIndex = 0;
        _endIndex   = 0;
        _size       = 0;
    }

    template <bool CONST_ITER> 
    class iterator_template
    {
        using iter_type            = iterator_template<CONST_ITER>;
        using iterator             = iterator_template<false>;
        using const_iterator       = iterator_template<true>;
        using iter_value_type      = std::conditional_t<CONST_ITER, const slot_pool::value_type, slot_pool::value_type>;
        using iter_element_type    = std::conditional_t<CONST_ITER, const slot_pool::element_type, slot_pool::element_type>;
        friend class container_type;

    public:
        using iterator_category = std::bidirectional_iterator_tag;
        using value_type        = slot_pool::value_type;
        using difference_type   = std::ptrdiff_t;
        using pointer           = iter_value_type*;
        using reference         = iter_value_type&;

        iterator_template(iter_element_type* ptr, iter_element_type* begin, iter_element_type* end) 
            : 
            _pointer(ptr), 
            _begin(begin), 
            _end(end)
        {}
        ~iterator_template() = default;

        template <bool rhsConst>
        iterator_template(const iterator_template<rhsConst>& rhs) requires(CONST_ITER && !rhsConst)
            : 
            _pointer(rhs._pointer), 
            _begin(rhs._begin), 
            _end(rhs._end)
        {}
        
        void prev()
        {
            if (_pointer == _begin)
                return;

            do
            {
                --_pointer;
            } while (_pointer != _begin && _pointer->second == false);
        }
        void next()
        {
            if (_pointer == _end)
                return;

            do
            {
                ++_pointer;
            } while (_pointer != _end && _pointer->second == false);
        }
        iter_type& operator=(const iter_type& rhs)
        {
            _pointer = rhs._pointer;
            _begin = rhs._begin;
            _end = rhs._end;

            return *this;
        }
        reference operator*() const noexcept { return _pointer->first; }
        pointer   operator->() const noexcept 
        { 
            pointer ptr = std::addressof(_pointer->first);
            return ptr; 
        }
        iter_type& operator++()
        {
            next();
            return *this;
        }
        iter_type operator++(int)
        {
            iter_type temp = *this;
            next();
            return temp;
        }
        iter_type& operator--()
        {
            prev();
            return *this;
        }
        iter_type operator--(int)
        {
            iter_type temp = *this;
            prev();
            return temp;
        }
        template <bool rhsConst> bool operator==(const iterator_template<rhsConst>& rhs) const
        {
            return _pointer == rhs._pointer;
        }
        template <bool rhsConst> bool operator!=(const iterator_template<rhsConst>& rhs) const
        {
            return !(_pointer == rhs._pointer);
        }

    private:
        iter_element_type* _pointer = nullptr;
        iter_element_type* _begin   = nullptr;
        iter_element_type* _end     = nullptr;
    };
    using iterator       = iterator_template<false>;
    using const_iterator = iterator_template<true>;
    using reverse_iterator       = std::reverse_iterator<iterator>;
    using const_reverse_iterator = std::reverse_iterator<const_iterator>;

    size_t size() const { return _size; } 
    bool empty() const { return _size == 0; }
    
    size_t create()
    { 
        return create(value_type());
    }

    template <typename __value_type> 
    size_t create(__value_type&& value)
    {
        size_t id;
        if (_freeIndices.empty() == false)
        {
            id = _freeIndices.back();
            _freeIndices.pop_back();
            auto& [val, isValid] = _elements[id];
            val = std::forward<__value_type>(value);
            isValid = true;
        }
        else
        {
            id = _elements.size();
            _elements.emplace_back(std::forward<__value_type>(value), true);
        }  
        ++_size;

        if (id < _beginIndex)
            _beginIndex = id;
        else if (_endIndex < id)
            _endIndex = id;
        else if (size() == 1)
        {
            _beginIndex = id;
            _endIndex  = id;
        }

        return id;
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
                --_size;

                if (index == _beginIndex)
                {
                    for (size_t i = _beginIndex + 1; i <= _endIndex; ++i)
                    {
                        auto& [v, ok] = _elements[i];
                        if (ok)
                        {
                            _beginIndex = i;
                            break;
                        }
                    }
                }
                else if (index == _endIndex)
                {
                    for (size_t i = _endIndex - 1; i >= _beginIndex; --i)
                    {
                        auto& [v, ok] = _elements[i];
                        if (ok)
                        {
                            _endIndex = i;
                            break;
                        }
                    }
                }
            }
        }
    }
    void clear()
    {
        size_t size = _elements.size();
        for (size_t i = 0; i < size; ++i)
        {
            auto& [value, valid] = _elements[i];
            if (valid)
            {
                _cleaner(value);
                valid = false;
                _freeIndices.push_back(i);
            }
        }
        _beginIndex = 0;
        _endIndex   = 0;
        _size       = 0;
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
    iterator begin() 
    { 
        element_type *begin, *end;
        get_begin_end_ptr(&begin, &end);
        return iterator(begin, begin, end);
    }
    iterator end()
    {
        element_type *begin, *end;
        get_begin_end_ptr(&begin, &end);
        return iterator(end, begin, end);
    }
    const_iterator begin() const { return cbegin(); }
    const_iterator end() const { return cend(); }
    const_iterator cbegin() const 
    {
        const element_type *begin, *end;
        get_begin_end_ptr(&begin, &end);
        return const_iterator(begin, begin, end);
    }
    const_iterator cend() const
    {
        const element_type *begin, *end;
        get_begin_end_ptr(&begin, &end);
        return const_iterator(end, begin, end);
    }
    reverse_iterator rbegin() { return reverse_iterator(end()); }
    reverse_iterator rend() { return reverse_iterator(begin()); }
    const_reverse_iterator rbegin() const { return crbegin(); }
    const_reverse_iterator rend() const { return crend(); }
    const_reverse_iterator crbegin() const { return const_reverse_iterator(cend()); }
    const_reverse_iterator crend() const { return const_reverse_iterator(cbegin()); }

private:
    size_t _size       = 0;
    size_t _beginIndex = 0;
    size_t _endIndex   = 0;

    std::vector<element_type> _elements;
    std::vector<size_t>       _freeIndices;
    cleaner_type              _cleaner;

    void get_begin_end_ptr(_Out_ element_type** begin, _Out_ element_type** end) 
    {
        if (empty())
        {
            *begin = nullptr;
            *end   = nullptr;
            return;
        }

        *begin = &_elements[_beginIndex];
        element_type* endTemp = &_elements[_endIndex];
        ++endTemp;
        *end = endTemp;
    }

    void get_begin_end_ptr(_Out_ const element_type** begin, _Out_ const element_type** end) const
    {
        if (empty())
        {
            *begin = nullptr;
            *end   = nullptr;
            return;
        }

        *begin = &_elements[_beginIndex];
        const element_type* endTemp = &_elements[_endIndex];
        ++endTemp;
        *end = endTemp;
    }

};