using System;
using System.Collections;
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
        private delegate void DrawFieldHandler(ImguiContext ctx, string name, object value, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback);
        private static readonly Dictionary<Type, DrawFieldHandler> typeByDrawFieldHandler = new(ReferenceEqualityComparer.Instance)
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
            [typeof(Single)] = (ctx, n, v, atts, cb) => ctx.DragFloat(n, (Single)v, v => cb(v)),
            [typeof(Double)] = (ctx, n, v, atts, cb) => ctx.DragDouble(n, (Double)v, v => cb(v)),
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

        public static void DrawField(ImguiContext context, Type type, string name, object value, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback)
        {
            if (typeByDrawFieldHandler.TryGetValue(type, out var handle) == true)
            {
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                }
                context.Text(name);
                context.SameLine();
                handle(context, $"[{type.Name}]##{name}", value, attributes, callback);
                if (isReadOnly)
                {
                    context.PopStyleVar();
                }
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                DrawEnumerable(context, type, name, (IEnumerable)value, isReadOnly, attributes, callback);
            }
            else
            {
                context.Selectable($"{name} {value} [{type.Name}]", false, ImGuiSelectableFlags.None, ()=>{ });
            }
        }

        public static void DrawField(ImguiContext context, Type type, string name, object value, bool isReadOnly, Action<object> callback)
        {
            DrawField(context, type, name, value, isReadOnly, null, callback);
        }

        public static void DrawEnumerable(ImguiContext context, Type type, string name, IEnumerable values, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<IEnumerable> callback)
        {    
            //TODO: List 및 배열은 따로 처리
            if (false && values is IList list)
            {
                if (type.IsArray)
                {

                }
                else 
                {

                }
            }
            else
            {
                context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                context.TreeNodeEx($"{name} [{type.Name}]", ImGuiTreeNodeFlags.None);
                uint index = 0;
                foreach (object? value in values)
                {
                    if(value == null)
                    {
                        context.Selectable($"({index}) null", false, ImGuiSelectableFlags.None, () => { });
                    }
                    else
                    {
                        object val = value;
                        Type valueType = val.GetType();
                        context.Text($"({index})".PadRight(5)); 
                        context.SameLine();
                        if (typeByDrawFieldHandler.TryGetValue(valueType, out var handle) == true)
                        {
                            handle(context, $"[{valueType.Name}]##({index})", val, attributes, (obj) => { });
                        }
                        else
                        {
                            context.Selectable($"({index}) {val} [{valueType.Name}]", false, ImGuiSelectableFlags.None, () => { });
                        }
                    }
                    ++index;
                }
                context.TreePop();
                context.PopStyleVar();
            }
        }

        public static void DrawEnumerable(ImguiContext context, Type type, string name, IEnumerable values, bool isReadOnly, Action<IEnumerable> callback)
        {
            DrawEnumerable(context, type, name, values, isReadOnly, null, callback);
        }

        public static void DrawFields(ImguiContext context, object target)
        {
            if (target.GetType().IsClass == false)
                return;

            IReadOnlyList<Field> fields = GetFields(target);
            foreach (var field in fields)
            {
                object? value = field.Get(target);
                if (value == null)
                    continue;

                Type type = field.Type;
                if (type.IsClass && IsSystemNamespace(type) == false)
                {
                    if (Member.HasAttribute<SerializableClassAttribute>(field.TypeAttributes))
                    {
                        context.TreeNodeEx($"{field.Name} [{type.Name}]", ImGuiTreeNodeFlags.None);
                        DrawFields(context, value);
                        context.TreePop();
                    }
                    else if (typeof(IIdentity).IsAssignableFrom(type))
                    {
                        context.PushStyleVar(ImGuiStyleVar.Alpha, 0.70f);
                        context.Text($"(Reference: {type.Name})");
                        context.PopStyleVar();
                    }             
                    continue;
                }

                var attributes = field.FieldAttributes;
                if (Member.HasAttribute<HideInInspectorAttribute>(attributes))
                {
                    continue;
                }

                string name = field.Name;
                bool isReadOnly = field.Set == null;
                DrawField(context, type, name, value, isReadOnly, attributes, (obj) => 
                { 
                    field.Set?.Invoke(target, obj); 
                });
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

                        DrawField(context, parameterType, parameterName, buffer[index], false, (v) =>
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