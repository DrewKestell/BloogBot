namespace WoWSharpClient.Models
{
    public interface IDeepCloneable<T>
    {
        T Clone();
        void CopyFrom(T source);
    }
}
