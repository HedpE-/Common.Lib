namespace Common.Lib.Interfaces
{
    public interface ISelectableObject
    {
        bool IsSelected { get; set; }
        bool CanSelect { get; set; }
    }
}
