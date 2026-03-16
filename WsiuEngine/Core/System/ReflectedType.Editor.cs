using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedType<T>
    {
        public static void DragFields(ImguiContext context, T target)
        {
            foreach (var field in ReflectedType<T>.Fields)
            {
                if (field.Type == typeof(double))
                {
                    if (field.Set == null)
                    {
                        if (field.Get(target) is double f)
                        {
                            context.BeginDisabled();
                            context.DragDouble(field.Name, f, (val) => { });
                            context.EndDisabled();
                        }
                    }
                    else
                    {
                        if (field.Get(target) is double f)
                        {
                            context.DragDouble(field.Name, f, (val) =>
                            {
                                field.Set(target, val);
                            });
                        }
                    }
                }
                else if (field.Type == typeof(float))
                {
                    if (field.Set == null)
                    {
                        if (field.Get(target) is float f)
                        {
                            context.BeginDisabled();
                            context.DragFloat(field.Name, f, (val) => { });
                            context.EndDisabled();
                        }
                    }
                    else
                    {
                        if (field.Get(target) is float f)
                        {
                            context.DragFloat(field.Name, f, (val) =>
                            {
                                field.Set(target, val);
                            });
                        }
                    }
                }
                else
                {
                    context.Text($"{field.Name}: {field.Get(target)}");
                }
            }
        }
    }
}