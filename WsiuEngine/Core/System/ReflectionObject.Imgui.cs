using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using WsiuEngine.Core.Interfaces;
using WsiuEngine.Extensions;
using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectionObject
    {
        private delegate void DrawFieldHandler(ImguiContext ctx, string name, object value, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback);
        private static readonly ButtonCallback readonlyButtonCallback = () => { };
        private static readonly Action<object> readonlyFieldSetter = (obj) => { };
        private static readonly Action<IEnumerable> readonlyEnumerableSetter = (obj) => { };
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
            [typeof(Boolean)] = (ctx, n, v, atts, cb) => ctx.Checkbox(n, (Boolean)v, v => cb(v)),
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

        public static void DrawField(ImguiContext context, Type type, string strId, object value, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<object> callback)
        {
            if (value is IReflectionDrawer drawer)
            {
                if (drawer.UseCustomDrawing)
                {
                    drawer.DrawFields(context, strId, isReadOnly, attributes);
                    return;
                }
            }

            if (typeByDrawFieldHandler.TryGetValue(type, out var handle) == true)
            {
                if (isReadOnly)
                    context.PushStyleReadOnly();
                handle(context, $"[{type.Name}]###{strId}", value, attributes, callback);
                if (isReadOnly)
                    context.PopStyleReadOnly();

                return;
            }

            if (value is IEnumerable enumerable)
            {
                DrawEnumerable(context, type, strId, enumerable, isReadOnly, attributes, callback);
                return;
            }

            if (type.IsClass && IsSystemNamespace(type) == false)
            {
                if (attributes != null && Member.HasAttribute<SerializableClassAttribute>(GetTypeAttributes(type)))
                {
                    context.TreeNodeEx($"{strId} [{type.Name}]", ImGuiTreeNodeFlags.None);
                    DrawFields(context, value);
                    context.TreePop();
                    return;
                }
                else if (value is IIdentity identity)
                {
                    context.Selectable($"(Reference: {type.Name})", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
                    return;
                }
            }

            context.Selectable($"{value} [{type.Name}]###{strId}", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
        }

        public static void DrawField(ImguiContext context, Type type, string strId, object value, bool isReadOnly, Action<object> callback)
        {
            DrawField(context, type, strId, value, isReadOnly, null, callback);
        }

        private delegate void DrawIListHandler(ImguiContext context, Type type, string name, IList values, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<IEnumerable> callback);
        private static readonly Dictionary<Type, DrawIListHandler> typeByIListHandler = new(ReferenceEqualityComparer.Instance)
        {
            [typeof(Array)] = (ctx, t, n, lt, rd, attr, cb) =>
            {
                int count = lt.Count;
                ctx.TreeNodeEx($"{n} [{t.Name}] ({count})###{n}", ImGuiTreeNodeFlags.None);
                DarwIList(ctx, n, lt, rd, attr, cb);
                if (rd == false)
                {
                    Type? elementType = t.GetElementType();
                    if (elementType != null)
                    {
                        DefaultConstructor? constructor = GetDefaultConstructor(elementType);
                        if (constructor != null)
                        {
                            ctx.Button("+", () =>
                            {
                                Array newArray = Array.CreateInstance(elementType, count + 1);
                                Array.Copy((Array)lt, newArray, count);
                                newArray.SetValue(constructor(), count);
                                cb(newArray);
                            });
                            if (0 < count)
                            {
                                ctx.SameLine();
                                ctx.Button("-", () =>
                                {
                                    Array newArray = Array.CreateInstance(elementType, count - 1);
                                    Array.Copy((Array)lt, newArray, count - 1);
                                    cb(newArray);
                                });
                            }
                        }
                    }
                }
                ctx.TreePop();
            },
            [typeof(List<>)] = (ctx, t, n, lt, rd, attr, cb) =>
            {
                int count = lt.Count;
                string typeName = GetGenericDisplayName(t);
                ctx.TreeNodeEx($"{n} [{typeName}] ({count})###{n}", ImGuiTreeNodeFlags.None);
                DarwIList(ctx, n, lt, rd, attr, cb);
                Type[] genericArgs = t.GetGenericArguments();
                if (genericArgs.Length > 0)
                {
                    Type elementType = genericArgs[0];
                    DefaultConstructor? constructor = GetDefaultConstructor(elementType);
                    if (constructor != null)
                    {
                        ctx.Button("+", () =>
                        {
                            lt.Add(constructor());
                        });
                        if (0 < count)
                        {
                            ctx.SameLine();
                            ctx.Button("-", () =>
                            {
                                lt.RemoveAt(count - 1);
                            });
                        }
                    }
                }
                ctx.TreePop();
            }
        };

        private static Type GetEnumerableType(Type type) => type switch
        {
            { IsArray: true } => typeof(Array),
            { IsGenericType: true } => type.GetGenericTypeDefinition(),
            _ => type
        };

        private static readonly Dictionary<Type, string> typeByGenericDisplayName = [];
        private static string GetGenericDisplayName(Type type)
        {
            if (typeByGenericDisplayName.TryGetValue(type, out string? displayName) == false)
            {
                displayName = BuildGenericDisplayName(type);
                typeByGenericDisplayName.Add(type, displayName);
            }
            return displayName;
        }

        private static string BuildGenericDisplayName(Type type)
        {
            if (type.IsGenericType == false)
                return type.Name;

            string baseName = type.Name;
            if (type.Name.Contains('`'))
                baseName = type.Name[..type.Name.IndexOf('`')];

            var genericArgs = type.GetGenericArguments().Select(t => BuildGenericDisplayName(t));
            return $"{baseName}<{string.Join(", ", genericArgs)}>";
        }

        public static void DrawEnumerable(ImguiContext context, Type type, string name, IEnumerable values, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<IEnumerable> callback)
        {
            Type enumerableType = GetEnumerableType(type);
            if (typeByIListHandler.TryGetValue(enumerableType, out DrawIListHandler? handler) == true)
            {
                if (isReadOnly)
                    context.PushStyleReadOnly();
                IList list = (IList)values;
                handler(context, type, name, list, isReadOnly, attributes, callback);
                if (isReadOnly)
                    context.PopStyleReadOnly();
            }
            else
            {
                int count = 0;
                if (values is ICollection collection)
                {
                    count = collection.Count;
                }
                else
                {
                    foreach (object? _ in values)
                    {
                        ++count;
                    }
                }
                context.PushStyleReadOnly();
                context.TreeNodeEx($"{name} [{type.Name}] ({count})###{name}", ImGuiTreeNodeFlags.None);
                uint index = 0;
                foreach (object? value in values)
                {
                    if (value == null)
                    {
                        context.Selectable($"({index}) null", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
                    }
                    else
                    {
                        object val = value;
                        if (val is IReflectionDrawer drawer)
                        {
                            if (drawer.UseCustomDrawing)
                            {
                                drawer.DrawFields(context, name, isReadOnly, attributes);
                            }
                        }
                        else
                        {
                            Type valueType = val.GetType();
                            context.TextUnformatted($"[{index}]".PadRight(5));
                            context.SameLine();
                            DrawField(context, valueType, $"[{valueType.Name}] ({index})", value, true, attributes, readonlyFieldSetter);
                        }
                    }
                    ++index;
                }
                context.TreePop();
                context.PopStyleReadOnly();
            }
        }
        public static void DrawEnumerable(ImguiContext context, Type type, string name, IEnumerable values, bool isReadOnly, Action<IEnumerable> callback)
        {
            DrawEnumerable(context, type, name, values, isReadOnly, null, callback);
        }

        public static void DrawStaticFields<TClass>(ImguiContext context) where TClass : class
        {
            IReadOnlyList<Field> fields = GetStaticFields<TClass>();
            DrawFields(context, null, fields);
        }

        private static void DarwIList(ImguiContext context, string name, IList list, bool isReadOnly, IReadOnlyDictionary<Type, Attribute>? attributes, Action<IEnumerable> callback)
        {
            if (isReadOnly)
                context.PushStyleReadOnly();
            int index = 0;
            foreach (object? value in list)
            {
                if (value == null)
                {
                    context.Selectable($"({index}) null", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
                }
                else
                {
                    if (value is IReflectionDrawer drawer)
                    {
                        if (drawer.UseCustomDrawing)
                        {
                            drawer.DrawFields(context, name, isReadOnly, attributes);
                        }
                    }
                    else
                    {
                        int i = index;
                        Type valueType = value.GetType();
                        context.TextUnformatted($"[{i}]".PadRight(5));
                        context.SameLine();
                        Action<object> setter = isReadOnly ? readonlyFieldSetter : (obj) =>
                        {
                            list[i] = obj;
                        };
                        context.PushID((uint)i);
                        DrawField(context, valueType, string.Empty, value, isReadOnly, attributes, setter);
                        context.PopID();
                    }
                }
                ++index;
            }
            if (isReadOnly)
                context.PopStyleReadOnly();
        }

        public static void DrawFields(ImguiContext context, object target)
        {
            Type targetType = target.GetType();
            if (targetType.IsClass == false)
                return;

            IReadOnlyList<Field> fields = GetFields(target);
            DrawFields(context, target, fields);
        }

        private static void DrawFields(ImguiContext context, object? target, IReadOnlyList<Field> fields)
        {
            if (fields.Count == 0)
                return;

            bool isTableOpen = false;
            void OpenTable()
            {
                if (isTableOpen == false)
                {
                    context.BeginTablePropertyType($"###{fields.GetHashCode()}");
                    isTableOpen = true;
                }
            }
            void CloseTable()
            {
                if (isTableOpen == true)
                {
                    context.EndTable();
                    isTableOpen = false;
                }
            }

            foreach (var field in fields)
            {
                object? value = field.Get(target);
                if (value == null)
                    continue;

                var attributes = field.FieldAttributes;
                if (Member.HasAttribute<HideInInspectorAttribute>(attributes))
                {
                    continue;
                }

                // 인터페이스 처리
                bool isReadOnly = field.Set == null;
                string name = field.Name;
                if (value is IReflectionDrawer drawer)
                {
                    if (drawer.UseCustomDrawing)
                    {
                        CloseTable();
                        drawer.DrawFields(context, name, isReadOnly, attributes);
                        continue;
                    }
                }

                // IEnumerable 타입 처리
                Type type = field.Type;
                bool hasDrawHandler = typeByDrawFieldHandler.TryGetValue(type, out var handle) == true;
                if (hasDrawHandler == false && value is IEnumerable enumerable)
                {
                    CloseTable();
                    Action<IEnumerable> setter = isReadOnly ? readonlyEnumerableSetter : (obj) =>
                    {
                        field.Set?.Invoke(target, obj);
                    };
                    DrawEnumerable(context, type, name, enumerable, isReadOnly, attributes, setter);
                    continue;
                }

                // 클래스 처리
                if (hasDrawHandler == false && type.IsClass && IsSystemNamespace(type) == false)
                {
                    CloseTable();
                    if (Member.HasAttribute<SerializableClassAttribute>(GetTypeAttributes(type)))
                    {
                        context.TreeNodeEx($"{field.Name} [{type.Name}]", ImGuiTreeNodeFlags.None);
                        DrawFields(context, value);
                        context.TreePop();
                    }
                    else if (value is IIdentity identity)
                    {
                        context.Selectable($"(Reference: {type.Name})", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
                    }
                    continue;
                }

                OpenTable();
                context.TableNextRow();
                if (isReadOnly)
                {
                    context.PushStyleReadOnly();
                }

                if (hasDrawHandler == true)
                {
                    context.TableNextColumn(); // 1
                    context.TextUnformatted(name);
                    context.TableNextColumn(); // 2
                    Action<object> setter = isReadOnly ? readonlyFieldSetter : (obj) =>
                    {
                        field.Set?.Invoke(target, obj);
                    };
                    handle!(context, $"[{type.Name}]###{name}", value, attributes, setter);
                }
                else
                {
                    context.TableNextColumn(); // 1
                    context.TextUnformatted(name);
                    context.TableNextColumn(); // 2
                    context.Selectable($"{value} [{type.Name}]###{name}", false, ImGuiSelectableFlags.None, readonlyButtonCallback);
                }

                if (isReadOnly)
                {
                    context.PopStyleReadOnly();
                }
            }
            CloseTable();
        }
        public static void DrawStaticMethods<TClass>(ImguiContext context) where TClass : class
        {
            IReadOnlyList<Method> methods = GetStaticMethods<TClass>();
            DrawMethods(context, null, methods);
        }

        public static void DrawMethods(ImguiContext context, object target)
        {
            if (target.GetType().IsClass == false)
                return;

            IReadOnlyList<Method> methods = GetMethods(target);
            DrawMethods(context, target, methods);
        }

        private static void DrawMethods(ImguiContext context, object? target, IReadOnlyList<Method> methods)
        {
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

                    object[] buffer;
                    if (target != null)
                        buffer = GetMethodParametersBuffer(target, method);
                    else
                        buffer = GetMethodParametersBuffer(methods, method);

                    context.BeginTablePropertyType(buffer.GetHashCode().ToString());
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        context.TableNextRow();
                        ParameterInfo info = parameters[i];
                        Type parameterType = info.ParameterType;
                        string parameterName = string.Empty;
                        if (info.Name != null)
                        {
                            parameterName = info.Name;
                        }
                        int index = i;

                        context.TableNextColumn(); // 1
                        context.TextUnformatted(parameterName);
                        context.TableNextColumn(); // 2
                        DrawField(context, parameterType, parameterName, buffer[index], false, (v) =>
                        {
                            buffer[index] = v;
                        });
                    }
                    context.EndTable();
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
                        DefaultConstructor constructor = GetDefaultConstructor(pType)!;
                        object? value = constructor();
                        if (value != null)
                            parametersBuffer[i] = value;
                    }
                    else if (pType == Types.String)
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