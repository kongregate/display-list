/// <summary>
/// Common interface for all UI elements that can be displayed in a <see cref="DisplayList{T, U}"/>.
/// </summary>
///
/// <typeparam name="T">The data type used to populate this display element.</typeparam>
public interface IDisplayElement<T>
{
    void Populate(T data);
}
