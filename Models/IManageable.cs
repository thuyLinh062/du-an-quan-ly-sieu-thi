namespace SupermarketManagementSystem
{
    public interface IManageable
    {
        bool IsActive { get; }
        void Disable();
    }
}