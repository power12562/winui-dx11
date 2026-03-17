using System.Numerics;
using WsiuRenderer;

namespace WsiuEngine.Extensions
{
    public static class ImguiContextExtensions
    {
        public static void DragVector2(this ImguiContext context, string label, in Vector2 value, FloatNChangedCallback callback)
        {
            float[] vector2 = [value.X, value.Y];
            context.DragFloatN(label, vector2, callback);
        }

        public static void DragVector3(this ImguiContext context, string label, in Vector3 value, FloatNChangedCallback callback)
        {
            float[] vector3 = [value.X, value.Y, value.Z];
            context.DragFloatN(label, vector3, callback);
        }

        public static void DragVector4(this ImguiContext context, string label, in Vector4 value, FloatNChangedCallback callback)
        {
            float[] vector4 = [value.X, value.Y, value.Z, value.W];
            context.DragFloatN(label, vector4, callback);
        }

    }
}
