using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedType<T>
    {
        public static void DragFields(ImguiContext context, T target)
        {
            foreach (var field in Fields)
            {
                bool isReadOnly = field.Set == null;
                if (isReadOnly)
                {
                    context.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                }

                if (field.Type == typeof(double))
                {
                    if (field.Get(target) is double f)
                    {
                        context.DragDouble(field.Name, f, (val) =>
                        {
                            field.Set?.Invoke(target, val);
                        });
                    }
                }
                else if (field.Type == typeof(float))
                {
                    if (field.Get(target) is float f)
                    {
                        context.DragFloat(field.Name, f, (val) =>
                        {
                            field.Set?.Invoke(target, val);
                        });
                    }
                }
                else
                {
                    context.Text($"{field.Name}: {field.Get(target)}");
                }

                if (isReadOnly)
                {
                    context.PopStyleVar();
                }
            }
        }
    }
}