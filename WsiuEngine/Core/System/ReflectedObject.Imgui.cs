using System;
using System.Collections.Generic;
using System.Numerics;
using WsiuEngine.Extensions;
using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedObject
    {
        private static HashSet<object> AlreadyDrawnObjects = new(ReferenceEqualityComparer.Instance);

        public static void DragFields(ImguiContext context, object target, bool isRoot = true)
        {
            if (target.GetType().IsClass == false)
                return;

            if (isRoot)
                AlreadyDrawnObjects.Clear();

            if (AlreadyDrawnObjects.Contains(target))
            {
                context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                context.Text($"(Shared Reference: {target})");
                context.PopStyleVar();
                return;
            }
            AlreadyDrawnObjects.Add(target);

            IReadOnlyList<Field> fields = GetFields(target);
            foreach (var field in fields)
            {
                object? value = field.Get(target);
                if (value == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && type.Namespace != null && type.Namespace.StartsWith("System") == false)
                {
                    context.TreeNodeEx(field.Name, ImGuiTreeNodeFlags.None);
                    DragFields(context, value, false);
                    context.TreePop();
                    continue;
                }

                string name = field.Name;
                bool isReadOnly = field.Set == null;
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                }

                Action drawAction = type switch
                {
                    var t when t == typeof(double) => () => context.DragDouble(name, (double)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var t when t == typeof(float) => () => context.DragFloat(name, (float)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var t when t == typeof(Vector2) => () => context.DragVector2(name, (Vector2)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var t when t == typeof(Vector3) => () => context.DragVector3(name, (Vector3)value, (v) =>
                    {
                        field.Set?.Invoke(target, v);
                    }),
                    var t when t == typeof(Vector4) => () => context.DragVector4(name, (Vector4)value, (v) =>
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