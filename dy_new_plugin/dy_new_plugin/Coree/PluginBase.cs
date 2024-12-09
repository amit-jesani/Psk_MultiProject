using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace dy_new_plugin
{
    /// <summary>
    /// Implements a late-bound plugin.
    /// </summary>
    public abstract class PluginBase : PluginBase<Entity>, IPlugin
    {
        public PluginBase() : base() { }
        public PluginBase(string unsecure, string secure) : base(unsecure, secure) { }

        // This fixes a weird issue with registrations
        new public void Execute(IServiceProvider serviceProvider) { base.Execute(serviceProvider); }
    }

    /// <summary>
    /// Implements a strongly typed plugin.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class PluginBase<TEntity> : IPlugin
         where TEntity : Entity, new()
    {
        #region Private Members

        private string _unsecure = "";
        private string _secure = "";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the unsecure configuration.
        /// </summary>
        /// <value>
        /// The unsecure configuration.
        /// </value>
        public string UnsecureConfiguration
        {
            get { return _unsecure; }
        }

        /// <summary>
        /// Gets the secure configuration.
        /// </summary>
        /// <value>
        /// The secure configuration.
        /// </value>
        public string SecureConfiguration
        {
            get { return _secure; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase{TEntity}"/> class.
        /// </summary>
        public PluginBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase{TEntity}"/> class.
        /// </summary>
        /// <param name="unsecure">The unsecure configuration.</param>
        /// <param name="secure">The secure configuration.</param>
        public PluginBase(string unsecure, string secure)
        {
            _unsecure = unsecure;
            _secure = secure;
        }

        /// <summary>
        /// An implementation of the IPlugin Execute method.  OnExecute should be overridden instead of replacing Execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <exception cref="Microsoft.Xrm.Sdk.InvalidPluginExecutionException"></exception>
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = new PluginContext<TEntity>(serviceProvider);

            try
            {
                OnExecute(context);
            }
            catch (Exception e)
            {
                context.Trace("Received Exception: {0}", e.ToString());

                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        public abstract void OnExecute(PluginContext<TEntity> context);
    }
}
