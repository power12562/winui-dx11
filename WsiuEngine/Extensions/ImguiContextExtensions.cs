using System;
using System.Numerics;
using WsiuRenderer;

namespace WsiuEngine.Extensions
{
    public enum ContextType
    {
        Default,
        Closable
    }

    public static class ImguiContextExtensions
    {
        public static ImguiContext CreateImguiContext(EngineCore core, string name, ContextType type = ContextType.Default)
        {
            ImguiContext context = new(core);
            switch (type)
            {
                case ContextType.Default:
                    context.InitializeWindow(name);
                    break;
                case ContextType.Closable:
                    context.InitializeWindowClosable(name);
                    break;
            }
            return context;
        }

        public static void DragVector2(this ImguiContext context, string label, in Vector2 value, Action<Vector2> callback)
        {
            float[] vector2 = [value.X, value.Y];
            context.DragFloatN(label, vector2, (v)=>
            {
                Vector2 outVector = new(v[0], v[1]);
                callback(outVector);
            });
        }

        public static void DragVector3(this ImguiContext context, string label, in Vector3 value, Action<Vector3> callback)
        {
            float[] vector3 = [value.X, value.Y, value.Z];
            context.DragFloatN(label, vector3, (v)=>
            {
                Vector3 outVector = new(v[0], v[1], v[2]);
                callback(outVector);
            });
        }

        public static void DragVector4(this ImguiContext context, string label, in Vector4 value, Action<Vector4> callback)
        {
            float[] vector4 = [value.X, value.Y, value.Z, value.W];
            context.DragFloatN(label, vector4, (v)=>
            {
                Vector4 outVector = new(v[0], v[1], v[2], v[3]);
                callback(outVector);
            });
        }
    }
}
