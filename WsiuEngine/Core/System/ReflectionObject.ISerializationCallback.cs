namespace WsiuEngine.Core.System
{
    public static partial class ReflectionObject
    {
        public interface ISerializationCallback
        {
            void OnBeforeSerialize();
            void OnAfterDeserialize();
        }
    }
}
