using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using WsiuEngine.Extensions;
using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedObject
    {
        private delegate void DrawHandler(ImguiContext ctx, string name, object value, Action<object> callback);
        private static readonly Dictionary<Type, DrawHandler> drawFieldHandler = new(ReferenceEqualityComparer.Instance)
        {
            [typeof(float)] = (ctx, name, val, cb) => ctx.DragFloat(name, (float)val, v => cb(v)),
            [typeof(double)] = (ctx, name, val, cb) => ctx.DragDouble(name, (double)val, v => cb(v)),
            [typeof(Int16)] = (ctx, name, val, cb) => ctx.DragInt16(name, (Int16)val, v => cb(v)),
            [typeof(Int32)] = (ctx, name, val, cb) => ctx.DragInt32(name, (Int32)val, v => cb(v)),
            [typeof(Int64)] = (ctx, name, val, cb) => ctx.DragInt64(name, (Int64)val, v => cb(v)),
            [typeof(UInt16)] = (ctx, name, val, cb) => ctx.DragUInt16(name, (UInt16)val, v => cb(v)),
            [typeof(UInt32)] = (ctx, name, val, cb) => ctx.DragUInt32(name, (UInt32)val, v => cb(v)),
            [typeof(UInt64)] = (ctx, name, val, cb) => ctx.DragUInt64(name, (UInt64)val, v => cb(v)),
            [typeof(Vector2)] = (ctx, name, val, cb) => ctx.DragVector2(name, (Vector2)val, v => cb(v)),
            [typeof(Vector3)] = (ctx, name, val, cb) => ctx.DragVector3(name, (Vector3)val, v => cb(v)),
            [typeof(Vector4)] = (ctx, name, val, cb) => ctx.DragVector4(name, (Vector4)val, v => cb(v)),
        };

        public static void DrawField(ImguiContext context, Type type, string name, object value, Action<object> callback)
        {
            if (drawFieldHandler.TryGetValue(type, out var handle))
            {
                handle(context, name, value, callback);
            }
            else
            {
                context.Text($"{name}: {value} ({type.Name})");
            }
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
                context.Text($"(Shared Reference: {target})");
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
                if (type.IsClass && type.Namespace != null && type.Namespace.StartsWith("System") == false)
                {
                    context.TreeNodeEx(field.Name, ImGuiTreeNodeFlags.None);
                    DrawFields(context, value, false);
                    context.TreePop();
                    continue;
                }

                string name = field.Name;
                bool isReadOnly = field.Set == null;
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                }

                DrawField(context, type, name, value, (v) =>
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
                parametersBuffer = [parametersCount];
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