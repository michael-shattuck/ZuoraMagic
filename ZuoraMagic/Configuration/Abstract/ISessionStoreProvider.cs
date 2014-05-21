namespace ZuoraMagic.Configuration.Abstract
{
    /// <summary>
    ///     Interface for session storage and re-use.
    ///     This class can be implemented and injected 
    ///     to provide easy access and extensibility
    ///     for session storage.
    /// </summary>
    public interface ISessionStoreProvider
    {
        /// <summary>
        ///     Retrieve the session
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        ZuoraSession RetrieveSession(string environment);

        /// <summary>
        ///     Store the session
        /// </summary>
        /// <param name="session">A generated salesforce session</param>
        /// <returns></returns>
        bool StoreSession(ZuoraSession session);

        /// <summary>
        ///     Checks the store to see if an active session
        ///     is available.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        bool FindAvailableSession(string environment);
    }
}