using System;
namespace WsiuEditor.Editor
{
    public interface IEditor
    {
        UInt64 ID { get; }
        string Name { get; set; }
        bool Active { get; set; }
        public void Draw();

        public void SetDisableCallback(Action callback);
    }
}
