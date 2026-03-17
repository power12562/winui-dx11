using System;
using System.Numerics;
using WsiuEngine.Extensions;
using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedType<T> where T : class
    {
        public static void DragFields(ImguiContext context, T target)
        {
            foreach (var field in Fields)
            {
                object? value = field.Get(target);
                if (value == null)
                    continue;

                string name = field.Name;
                bool isReadOnly = field.Set == null;
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                }

                Action drawAction = field.Type switch
                {
                    var type when type == typeof(double) => () => context.DragDouble(name, (double)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var type when type == typeof(float) => () => context.DragFloat(name, (float)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var type when type == typeof(Vector2) => () => context.DragVector2(name, (Vector2)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var type when type == typeof(Vector3) => () => context.DragVector3(name, (Vector3)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var type when type == typeof(Vector4) => () => context.DragVector4(name, (Vector4)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    _ => () => context.Text($"{name}: {value}")
                };
                drawAction.Invoke();

                if (isReadOnly)
                {
                    context.PopStyleVar();
                }
            }
        }
    }
}