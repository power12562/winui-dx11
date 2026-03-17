using System.Numerics;
namespace WsiuRenderer
{
    public sealed partial class ImguiContext
    {
        public void DragVector2(string label, in Vector2 value, FloatNChangedCallback callback)
        {
            float[] vector2 = [value.X, value.Y];
            DragFloatN(label, vector2, callback);
        }

        public void DragVector3(string label, in Vector3 value, FloatNChangedCallback callback)
        {
            float[] vector3 = [value.X, value.Y, value.Z];
            DragFloatN(label, vector3, callback);
        }

        public void DragVector4(string label, in Vector4 value, FloatNChangedCallback callback)
        {
            float[] vector4 = [value.X, value.Y, value.Z, value.W];
            DragFloatN(label, vector4, callback);
        }

    }
}
