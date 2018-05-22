#region using

using NLog.Layouts;

#endregion

namespace NLog.Targets
{
    /// <summary>
    ///     Sends log messages to the remote instance of NLog Viewer.
    /// </summary>
    /// <seealso href="https://github.com/nlog/nlog/wiki/NLogViewer-target">Documentation on NLog Wiki</seealso>
    /// <example>
    ///     <p>
    ///         To set up the target in the <a href="config.html">configuration file</a>,
    ///         use the following syntax:
    ///     </p>
    ///     <code lang="XML" source="examples/targets/Configuration File/NLogViewer/NLog.config" />
    ///     <p>
    ///         This assumes just one target and a single rule. More configuration
    ///         options are described <a href="config.html">here</a>.
    ///     </p>
    ///     <p>
    ///         To set up the log target programmatically use code like this:
    ///     </p>
    ///     <code lang="C#" source="examples/targets/Configuration API/NLogViewer/Simple/Example.cs" />
    ///     <p>
    ///         NOTE: If your receiver application is ever likely to be off-line, don't use TCP protocol
    ///         or you'll get TCP timeouts and your application will crawl.
    ///         Either switch to UDP transport or use <a href="target.AsyncWrapper.html">AsyncWrapper</a> target
    ///         so that your application threads will not be blocked by the timing-out connection attempts.
    ///     </p>
    /// </example>
    [Target("SimpleNetwork")]
    public class SimpleNetworkTarget : NetworkTarget
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NLogViewerTarget" /> class.
        /// </summary>
        /// <remarks>
        ///     The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        public SimpleNetworkTarget()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NLogViewerTarget" /> class.
        /// </summary>
        /// <remarks>
        ///     The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public SimpleNetworkTarget(string name)
            : this()
        {
            Name = name;
        }

        private readonly SimpleNetworkLayout layout = new SimpleNetworkLayout();

        /// <summary>
        ///     Gets or sets the instance of <see cref="Log4JXmlEventLayout" /> that is used to format log messages.
        /// </summary>
        /// <docgen category='Layout Options' order='10' />
        public override Layout Layout
        {
            get { return layout; }

            set { }
        }
    }
}