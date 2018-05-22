#region using

using System.Text;

#endregion

namespace NLog.Layouts
{
    /// <summary>
    ///     A specialized layout that renders Log4j-compatible XML events.
    /// </summary>
    /// <remarks>
    ///     This layout is not meant to be used explicitly. Instead you can use ${log4jxmlevent} layout renderer.
    /// </remarks>
    [Layout("SimpleNetworkLayout")]
    public class SimpleNetworkLayout : Layout
    {
        private const char ElementSeperator = ',';
        private const char EntrySeperator = '\n';

        /// <summary>
        ///     Renders the layout for the specified logging event by invoking layout renderers.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        /// <returns>The rendered layout.</returns>
        protected override string GetFormattedMessage(LogEventInfo logEvent)
        {
            var sb = new StringBuilder();
            sb.Append(logEvent.TimeStamp.ToBinary());
            sb.Append(ElementSeperator);
            sb.Append(logEvent.LoggerName);
            sb.Append(ElementSeperator);
            sb.Append(logEvent.Level);
            sb.Append(ElementSeperator);
            sb.Append(logEvent.FormattedMessage.Replace(EntrySeperator, ' '));
            sb.Append(EntrySeperator);
            return sb.ToString();
        }
    }
}