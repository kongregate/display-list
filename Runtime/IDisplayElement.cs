/// <summary>
/// Common interface for all UI elements that can be displayed in a <see cref="DisplayList{T, U}"/>.
/// </summary>
///
/// <typeparam name="D">The data type used to populate this display element.</typeparam>
public interface IDisplayElement<D>
{
    void Populate(D data);
}
