using WsiuRenderer;
namespace WsiuEngine.Core.System
{
    public static partial class ReflectedType<T>
    {
        public static void DragFields(ImguiContext context, T target)
        {
            foreach (var field in Fields)
            {
                if (field.Type == typeof(double))
                {
                    if (field.Get(target) is double f)
                    {
                        if (field.Set == null)
                        {
                            context.BeginDisabled();
                            context.DragDouble(field.Name, f, (val) => { });
                            context.EndDisabled();

                        }
                        else
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
                    if (field.Get(target) is float f)
                    {
                        if (field.Set == null)
                        {

                            context.BeginDisabled();
                            context.DragFloat(field.Name, f, (val) => { });
                            context.EndDisabled();

                        }
                        else
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