namespace presenting.ecslite
{
    public interface IUpdatable<TData>
        where TData : struct
    {
        public void Update(TData? data);
    }
}