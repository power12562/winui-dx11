#pragma once

// DirectX 11 
#include <d3d11_4.h>
#include <dxgi1_6.h>
#include <d3dcompiler.h>
#include <DirectXMath.h>
#include <wrl/client.h>

// DirectXTK 
#include "CommonStates.h"
#include "SpriteBatch.h"
#include "SpriteFont.h"
#include "SimpleMath.h"
#include "WICTextureLoader.h"
#include "GeometricPrimitive.h"
#include "Effects.h"

#pragma comment(lib, "d3d11.lib")
#pragma comment(lib, "dxgi.lib")
#pragma comment(lib, "d3dcompiler.lib")

// namespace
namespace DX = DirectX;
namespace SimpleMath = DirectX::SimpleMath;
using Microsoft::WRL::ComPtr;