using System;
namespace WsiuEditor.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SingletonEditorAttribute : Attribute {}

    public interface IEditor
    {
        UInt64 ID { get; }
        string Name { get; set; }
        bool Active { get; set; }
        public void Draw();

        public void SetDisableCallback(Action callback);
    }
}
