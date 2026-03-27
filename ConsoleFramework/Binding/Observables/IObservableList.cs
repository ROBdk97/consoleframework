namespace ConsoleFramework.Binding.Observables;

/// <summary>
/// Marks IList / IList&lt;T&gt; with notifications support.
/// Not derived from IList/IList&lt;T&gt; to allow both generic and non-generic implementations.
/// </summary>
public interface IObservableList
{
    event ListChangedHandler ListChanged;
}
