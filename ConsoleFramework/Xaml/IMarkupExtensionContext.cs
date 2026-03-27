using System.Collections.Generic;

namespace ConsoleFramework.Xaml
{
    /// <summary>
    /// Context accessible to a markup extension.
    /// </summary>
    public interface IMarkupExtensionContext
    {
        /// <summary>
        /// The name of the property being set through the markup extension.
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Reference to the object being configured.
        /// </summary>
        object Object { get; }

        /// <summary>
        /// Returns the active DataContext for the configured object.
        /// If the currently configured object has no its own DataContext,
        /// the context of the object above in the control hierarchy will be taken, and so on to the main element
        /// of the control tree.
        /// </summary>
        object DataContext { get; }

        /// <summary>
        /// Returns already created object with specified x:Id attribute value or null if object with
        /// this x:Id is not constructed yet. To resolve forward references use fixup tokens mechanism.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        object GetObjectById(string id);

        /// <summary>
        /// Gets a value that determines whether calling GetFixupToken is available
        /// in order to resolve a name into a token for forward resolution.
        /// </summary>
        bool IsFixupTokenAvailable { get; }

        /// <summary>
        /// Returns an object that can correct for certain markup patterns that produce forward references.
        /// </summary>
        /// <param name="ids">A collection of ids that are possible forward references.</param>
        /// <returns>An object that provides a token for lookup behavior to be evaluated later.</returns>
        IFixupToken GetFixupToken(IEnumerable<string> ids);
    }
}

