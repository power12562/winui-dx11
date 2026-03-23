using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using WsiuEngine.Extensions;
using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectionObject
    {
        private delegate void DrawHandler(ImguiContext ctx, string name, object value, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback);
        private static readonly Dictionary<Type, DrawHandler> drawFieldHandler = new(ReferenceEqualityComparer.Instance)
        {
            [typeof(string)] = (ctx, n, v, atts, cb) =>
            {
                string value = (string)v;
                if (atts == null || atts.Count == 0)
                {
                    ctx.InputText(n, value, v => cb(v));
                }
                else
                {                 
                    if (Member.GetAttribute<MultilineStringFieldAttribute>(atts) is { } attr)
                    {
                        ctx.InputTextMultiline(n, value, attr.Width, attr.Height, v => cb(v));
                    }
                    else
                    {
                        ctx.InputText(n, value, v => cb(v));
                    }
                }
            },
            [typeof(float)] = (ctx, n, v, atts, cb) => ctx.DragFloat(n, (float)v, v => cb(v)),
            [typeof(double)] = (ctx, n, v, atts, cb) => ctx.DragDouble(n, (double)v, v => cb(v)),
            [typeof(Int16)] = (ctx, n, v, atts, cb) => ctx.DragInt16(n, (Int16)v, v => cb(v)),
            [typeof(Int32)] = (ctx, n, v, atts, cb) => ctx.DragInt32(n, (Int32)v, v => cb(v)),
            [typeof(Int64)] = (ctx, n, v, atts, cb) => ctx.DragInt64(n, (Int64)v, v => cb(v)),
            [typeof(UInt16)] = (ctx, n, v, atts, cb) => ctx.DragUInt16(n, (UInt16)v, v => cb(v)),
            [typeof(UInt32)] = (ctx, n, v, atts, cb) => ctx.DragUInt32(n, (UInt32)v, v => cb(v)),
            [typeof(UInt64)] = (ctx, n, v, atts, cb) => ctx.DragUInt64(n, (UInt64)v, v => cb(v)),
            [typeof(Vector2)] = (ctx, n, v, atts, cb) => ctx.DragVector2(n, (Vector2)v, v => cb(v)),
            [typeof(Vector3)] = (ctx, n, v, atts, cb) => ctx.DragVector3(n, (Vector3)v, v => cb(v)),
            [typeof(Vector4)] = (ctx, n, v, atts, cb) => ctx.DragVector4(n, (Vector4)v, v => cb(v)),
        };

        public static void DrawField(ImguiContext context, Type type, string name, object value, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback)
        {
            if (drawFieldHandler.TryGetValue(type, out var handle))
            {
                handle(context, name, value, attributes, callback);
            }
            else
            {
                context.Text($"{name}: {value} ({type.Name})");
            }
        }

        public static void DrawField(ImguiContext context, Type type, string name, object value, Action<object> callback)
        {
            DrawField(context, type, name, value, null, callback);
        }

        private static readonly HashSet<object> alreadyDrawnObjects = new(ReferenceEqualityComparer.Instance);
        public static void DrawFields(ImguiContext context, object target, bool isRoot = true)
        {
            if (target.GetType().IsClass == false)
                return;

            if (isRoot)
                alreadyDrawnObjects.Clear();

            if (alreadyDrawnObjects.Contains(target))
            {
                context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                context.Text($"(Shared Reference: {target.GetType().Name})");
                context.PopStyleVar();
                return;
            }
            alreadyDrawnObjects.Add(target);

            IReadOnlyList<Field> fields = GetFields(target);
            foreach (var field in fields)
            {
                object? value = field.Get(target);
                if (value == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                {
                    context.TreeNodeEx(field.Name, ImGuiTreeNodeFlags.None);
                    DrawFields(context, value, false);
                    context.TreePop();
                    continue;
                }

                var attributes = field.CustomAttributes;
                if (Member.HasAttribute<HideInInspectorAttribute>(attributes))
                {
                    continue;
                }

                string name = field.Name;
                bool isReadOnly = field.Set == null;
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                }

                DrawField(context, type, name, value, attributes, (v) =>
                {
                    field.Set?.Invoke(target, v);
                });

                if (isReadOnly)
                {
                    context.PopStyleVar();
                }
            }
        }

        public static void DrawMethods(ImguiContext context, object target)
        {
            if (target.GetType().IsClass == false)
                return;

            IReadOnlyList<Method> methods = GetMethods(target);
            foreach (Method method in methods)
            {
                List<ParameterInfo> parameters = method.Parameters;
                if (parameters.Count == 0)
                {
                    context.Selectable(method.DisplayName, false, ImGuiSelectableFlags.None, () =>
                    {
                        method.Invoker(target, null);
                    });
                }
                else
                {
                    context.TreeNodeEx(method.DisplayName, ImGuiTreeNodeFlags.None);
                    object[] buffer = GetMethodParametersBuffer(target, method);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        ParameterInfo info = parameters[i];
                        Type parameterType = info.ParameterType;
                        string parameterName = string.Empty;
                        if (info.Name != null)
                        {
                            parameterName = info.Name;
                        }
                        int index = i;

                        DrawField(context, parameterType, parameterName, buffer[index], (v) =>
                        {
                            buffer[index] = v;
                        });
                    }
                    context.Button("call", () =>
                    {
                        method.Invoker(target, buffer);
                    });
                    context.TreePop();
                }
            }
        }

        private static readonly ConditionalWeakTable<object, Dictionary<Method, object[]>> inputTable = [];
        private static object[] GetMethodParametersBuffer(object obj, Method method)
        {
            if (method.Parameters.Count == 0)
                return [];

            if (inputTable.TryGetValue(obj, out var dictionary) == false)
            {
                dictionary = new(ReferenceEqualityComparer.Instance);
                inputTable.Add(obj, dictionary);
            }

            if (dictionary.TryGetValue(method, out var parametersBuffer) == false)
            {
                int parametersCount = method.Parameters.Count;
                parametersBuffer = new object[parametersCount];
                for (int i = 0; i < parametersCount; i++)
                {
                    ParameterInfo info = method.Parameters[i];
                    Type pType = info.ParameterType;
                    if (info.HasDefaultValue)
                    {
                        if (info.DefaultValue != null)
                            parametersBuffer[i] = info.DefaultValue;
                    }
                    else if (pType.IsValueType)
                    {
                        object? value = Activator.CreateInstance(pType);
                        if (value != null)
                            parametersBuffer[i] = value;
                    }
                    else if (pType == typeof(string))
                    {
                        parametersBuffer[i] = string.Empty;
                    }
                }
                dictionary.Add(method, parametersBuffer);
            }
            return parametersBuffer;
        }
    }
}