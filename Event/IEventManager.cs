using COL.UnityGameWheels.Core;

namespace COL.UnityGameWheels.Unity
{
    /// <summary>
    /// Interface of an event manager.
    /// </summary>
    public interface IEventManager : IManager
    {
        /// <summary>
        /// Event module.
        /// </summary>
        IEventModule Module { get; }

        /// <summary>
        /// How to release consumed <see cref="BaseEventArgs"/> objects.
        /// </summary>
        IEventArgsReleaser EventArgsReleaser { get; set; }

        /// <summary>
        /// Add a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        void AddEventListener(int eventId, OnHearEvent onHearEvent);

        /// <summary>
        /// Remove a event listener callback.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="onHearEvent">Callback when hearing the event.</param>
        void RemoveEventListener(int eventId, OnHearEvent onHearEvent);

        /// <summary>
        /// Send an event to listeners.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event.</param>
        void SendEvent(object sender, BaseEventArgs e);

        /// <summary>
        /// Send an event to listeners without any delay.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event itself.</param>
        void SendEventNow(object sender, BaseEventArgs eventArgs);
    }
}