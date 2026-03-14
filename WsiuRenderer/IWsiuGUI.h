#pragma once

class IWsiuGUI
{
public:
    virtual ~IWsiuGUI() = default;
    virtual void OnCreate() = 0;
    virtual void OnDraw() = 0;
    virtual void OnDestroy() = 0;
};