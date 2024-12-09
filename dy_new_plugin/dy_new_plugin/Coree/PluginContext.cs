using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace dy_new_plugin
{
    public enum ParameterType
    {
        IN, OUT
    }

    /// <summary>
    /// A class that contains the context for a plugin
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class PluginContext<TEntity>
        where TEntity : Entity, new()
    {
        #region Private Members

        private IServiceProvider _serviceProvider;
        private IPluginExecutionContext _context;
        private ITracingService _tracer;
        private Lazy<IOrganizationService> _organizationService;
        private Lazy<TEntity> _entity;
        private Lazy<EntityReference> _reference;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the local plugin execution context.
        /// </summary>
        /// <value>
        /// The local plugin execution context.
        /// </value>
        public IPluginExecutionContext LocalContext
        {
            get { return _context; }
        }

        /// <summary>
        /// Gets an instance of the IOrganizationService.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        public IOrganizationService Service
        {
            get { return _organizationService.Value; }
        }

        /// <summary>
        /// Gets the target of this message.
        /// </summary>
        /// <value>
        /// The Entity targeted by this message.
        /// </value>
        public TEntity Target
        {
            get { return _entity.Value; }
        }

        /// <summary>
        /// Gets the entity reference of the entity affected by this message.
        /// </summary>
        /// <value>
        /// The entity reference of the Entity targeted by this message.
        /// </value>
        public EntityReference Reference
        {
            get
            {
                return _reference.Value;
            }
        }

        /// <summary>
        /// Gets the name of the message.
        /// </summary>
        /// <value>
        /// The name of the message.
        /// </value>
        public string MessageName
        {
            get
            {
                return LocalContext.MessageName;
            }
        }

        #endregion

        public Entity GetEntity(Entity targetEntity)
        {
            Entity entity = new Entity(targetEntity.LogicalName);
            entity.Id = targetEntity.Id;

            return entity;
        }

        public Entity GetEntity(EntityReference entityReference)
        {
            Entity entity = new Entity(entityReference.LogicalName);
            entity.Id = entityReference.Id;

            return entity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginContext{TEntity}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public PluginContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // Obtain the execution context from the service provider.
            _context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            _tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            _organizationService =
                new Lazy<IOrganizationService>(GetOrganizationService);

            _entity =
                new Lazy<TEntity>(GetTarget);

            _reference =
                new Lazy<EntityReference>(GetReference);
        }

        /// <summary>
        /// Gets the post image by name.
        /// </summary>
        /// <param name="name">The name of the post image.</param>
        /// <returns></returns>
        public Entity GetPostImage(string name)
        {
            return GetImage(name, _context.PostEntityImages);
        }

        /// <summary>
        /// Tries to get the post image by name.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="etn">The entity in the image.</param>
        /// <returns>If there is an entity image with that name or not</returns>
        public bool TryGetPostImage(string name, out Entity etn)
        {
            return TryGetImage(name, _context.PostEntityImages, out etn);
        }

        /// <summary>
        /// Gets the pre image by name.
        /// </summary>
        /// <param name="name">The name of the pre image.</param>
        /// <returns></returns>
        public Entity GetPreImage(string name)
        {
            return GetImage(name, _context.PreEntityImages);
        }

        /// <summary>
        /// Tries to get the pre image by name.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="etn">The entity in the image.</param>
        /// <returns>If there is an entity image with that name or not</returns>
        public bool TryGetPreImage(string name, out Entity etn)
        {
            return TryGetImage(name, _context.PreEntityImages, out etn);
        }

        /// <summary>
        /// Determines whether either the specified target entity has value or the pre-image entity.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public bool HasValue(Entity entity, string attribute)
        {
            return HasValue(entity, null, attribute);
        }

        /// <summary>
        /// Determines whether either the specified target entity has value or the pre-image entity.
        /// </summary>
        /// <param name="entity">The target entity.</param>
        /// <param name="preEntity">The pre-image entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public bool HasValue(Entity entity, Entity preEntity, string attribute)
        {
            return
                entity.Attributes.ContainsKey(attribute) ||
                    (preEntity != null && preEntity.Attributes.ContainsKey(attribute));
        }

        /// <summary>
        /// Get the value of an attribute from the target entity and the pre-image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The target entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>the attribute value</returns>
        public T GetValue<T>(Entity entity, string attribute) //
        {
            return GetValue<T>(entity, null, attribute);
        }

        /// <summary>
        /// Get the value of an attribute from the target entity and the pre-image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The target entity.</param>
        /// <param name="preEntity">The pre-image entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <returns>the attribute value</returns>
        public T GetValue<T>(Entity entity, Entity preEntity, string attribute)
        {
            T result = default(T);

            if (entity.Attributes.ContainsKey(attribute))
            {
                if (entity[attribute] is AliasedValue && typeof(T) != typeof(AliasedValue))
                {
                    result = (T)entity.GetAttributeValue<AliasedValue>(attribute).Value;
                }
                else
                {
                    result = (T)entity[attribute];
                }
            }
            else if (preEntity != null && preEntity.Attributes.ContainsKey(attribute))
            {
                if (preEntity[attribute] is AliasedValue && typeof(T) != typeof(AliasedValue))
                {
                    result = (T)preEntity.GetAttributeValue<AliasedValue>(attribute).Value;
                }
                else
                {
                    result = (T)preEntity[attribute];
                }
            }

            return result;
        }

       
        /// <summary>
        /// Tries the get the value of an attribute from the target entity and the pre-image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The target entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns>True if the attribute is in either the target or the pre-image</returns>
        public bool TryGetValue<T>(Entity entity, string attribute, out T result)
        {
            return TryGetValue<T>(entity, null, attribute, out result);
        }

        /// <summary>
        /// Tries the get the value of an attribute from the target entity and the pre-image.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The target entity.</param>
        /// <param name="preEntity">The pre-image entity.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="result">The resulting value.</param>
        /// <returns>True if the attribute is in either the target or the pre-image</returns>
        public bool TryGetValue<T>(Entity entity, Entity preEntity, string attribute, out T result)
        {
            result = default(T);

            if (HasValue(entity, preEntity, attribute))
            {
                result = GetValue<T>(entity, preEntity, attribute);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the specified parameter (input or output) exists.
        /// </summary>
        /// <param name="paramType">Collection to look in to get the parameter (input or output).</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns></returns>
        public bool HasParameter(ParameterType paramType, string name)
        {
            return (paramType == ParameterType.IN &&
                    _context.InputParameters.ContainsKey(name) &&
                    _context.InputParameters[name] != null) ||
                    (paramType == ParameterType.OUT &&
                    _context.OutputParameters.ContainsKey(name) &&
                    _context.OutputParameters[name] != null);
        }

        /// <summary>
        /// Gets the specified parameter (input or output).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramType">Collection to look in to get the parameter (input or output).</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="result">The resulting value or the default value of the type specified.</param>
        /// <returns></returns>
        public T GetParameter<T>(ParameterType paramType, string name)
        {
            T result = default(T);

            if (HasParameter(paramType, name))
            {
                result =
                    (T)(paramType == ParameterType.IN ?
                            _context.InputParameters[name] :
                            _context.OutputParameters[name]);
            }

            return result;
        }

        /// <summary>
        /// Tries to get the specified parameter (input or output).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramType">Collection to look in to get the parameter (input or output).</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="result">The resulting value or the default value of the type specified.</param>
        /// <returns></returns>
        public bool TryGetParameter<T>(ParameterType paramType, string name, out T result)
        {
            result = default(T);

            if (HasParameter(paramType, name) &&
                (paramType == ParameterType.IN ?
                            _context.InputParameters[name] :
                            _context.OutputParameters[name]) is T)
            {
                result =
                    (T)(paramType == ParameterType.IN ?
                            _context.InputParameters[name] :
                            _context.OutputParameters[name]);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the specified entity by refrence.
        /// </summary>
        /// <param name="er">The refrence of the entity to retrieve.</param>
        /// <param name="columns">The attributes to retrieve.</param>
        /// <returns>An Entity</returns>
        public Entity Retrieve(EntityReference er, params string[] columns)
        {
            ColumnSet cols = null;

            if (columns == null || columns.Length == 0)
            {
                cols = new ColumnSet(true);
            }
            else
            {
                cols = new ColumnSet(columns);
            }

            return Service.Retrieve(er.LogicalName, er.Id, cols);
        }

        /// <summary>
        /// Retrieves the first entity returned by the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>An Entity</returns>
        public Entity RetrieveByAttribute(QueryByAttribute query)
        {
            Entity result = new Entity();

            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 1;
            query.PageInfo.PageNumber = 1;

            EntityCollection col = Service.RetrieveMultiple(query);

            if (col != null && col.Entities != null && col.Entities.Count > 0)
            {
                result = col.Entities[0];
            }

            return result;
        }

        /// <summary>
        /// Retrieves the first entity returned by the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>An Entity</returns>
        public Entity RetrieveByExpression(QueryExpression query)
        {
            Entity result = new Entity();

            query.PageInfo = new PagingInfo();
            query.PageInfo.Count = 1;
            query.PageInfo.PageNumber = 1;

            EntityCollection col = Service.RetrieveMultiple(query);

            if (col != null && col.Entities != null && col.Entities.Count > 0)
            {
                result = col.Entities[0];
            }

            return result;
        }

        /// <summary>
        /// Retrieves all records returned by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A list of Entities</returns>
        public List<Entity> RetrieveAll(QueryExpression query)
        {
            List<Entity> results = new List<Entity>();
            int page = 1;
            string pageCookie = "";
            bool moreRecords = false;

            query.PageInfo = new PagingInfo();

            do
            {
                moreRecords = false;

                query.PageInfo.PageNumber = page;
                query.PageInfo.Count = 5000;
                query.PageInfo.PagingCookie = pageCookie;

                EntityCollection col = Service.RetrieveMultiple(query);

                if (col != null && col.Entities != null && col.Entities.Count > 0)
                {
                    moreRecords = col.MoreRecords;
                    pageCookie = col.PagingCookie;

                    results.AddRange(col.Entities);

                    page++;
                }

            } while (moreRecords);

            return results;
        }

        /// <summary>
        /// Retrieves all records returned by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A list of Entities</returns>
        public List<Entity> RetrieveAll(QueryByAttribute query)
        {
            List<Entity> results = new List<Entity>();
            int page = 1;
            string pageCookie = "";
            bool moreRecords = false;

            query.PageInfo = new PagingInfo();

            do
            {
                moreRecords = false;

                query.PageInfo.PageNumber = page;
                query.PageInfo.Count = 5000;
                query.PageInfo.PagingCookie = pageCookie;

                EntityCollection col = Service.RetrieveMultiple(query);

                if (col != null && col.Entities != null && col.Entities.Count > 0)
                {
                    moreRecords = col.MoreRecords;
                    pageCookie = col.PagingCookie;

                    results.AddRange(col.Entities);

                    page++;
                }

            } while (moreRecords);

            return results;
        }

        /// <summary>
        /// Retrieves all records returned by a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A list of Entities</returns>
        public List<Entity> RetrieveAll(string fetchXml, int count = 5000)
        {
            List<Entity> results = new List<Entity>();

            // Inject page, paging-cookie, count if they are missing
            XDocument xdoc = XDocument.Parse(fetchXml);
            string fetchXmlCore = "";

            using (StringWriter sw = new StringWriter())
            {
                if (xdoc.Root.Attribute("page") == null)
                {
                    xdoc.Root.SetAttributeValue("page", "{0}");
                }

                if (xdoc.Root.Attribute("paging-cookie") == null)
                {
                    xdoc.Root.SetAttributeValue("paging-cookie", "{1}");
                }

                if (xdoc.Root.Attribute("count") == null)
                {
                    xdoc.Root.SetAttributeValue("count", count.ToString());
                }

                xdoc.Save(sw);

                fetchXmlCore = sw.ToString();
            }

            bool moreRecords = false;
            int page = 0;
            string pageCookie = "";

            do
            {
                page++;
                moreRecords = false;

                FetchExpression query =
                    new FetchExpression(
                        String.Format(fetchXmlCore, page, System.Security.SecurityElement.Escape(pageCookie)));

                EntityCollection col = Service.RetrieveMultiple(query);

                if (col != null && col.Entities != null && col.Entities.Count > 0)
                {
                    moreRecords = col.MoreRecords;
                    pageCookie = col.PagingCookie;

                    foreach (Entity etn in col.Entities)
                    {
                        results.Add(etn);
                    }
                }

            } while (moreRecords);

            return results;
        }

        public void Associate(EntityReference erParent, string relationship, params EntityReference[] related)
        {
            Service.Associate(
                erParent.LogicalName,
                erParent.Id,
                new Relationship(relationship),
                new EntityReferenceCollection(related));
        }

        public void Disassociate(EntityReference erParent, string relationship, params EntityReference[] related)
        {
            Service.Disassociate(
                erParent.LogicalName,
                erParent.Id,
                new Relationship(relationship),
                new EntityReferenceCollection(related));
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            return Execute<OrganizationRequest, OrganizationResponse>(request);
        }

        public TResult Execute<T, TResult>(T request)
            where T : OrganizationRequest
            where TResult : OrganizationResponse
        {
            return (TResult)Service.Execute(request);
        }

        /// <summary>
        /// Traces the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public void Trace(string message, params object[] args)
        {
            _tracer.Trace(message, args);
        }

        /// <summary>
        /// Gets the organization service.
        /// </summary>
        /// <returns>An instance of the service</returns>
        private IOrganizationService GetOrganizationService()
        {
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)_serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            return factory.CreateOrganizationService(_context.UserId);
        }

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <returns></returns>
        private TEntity GetTarget()
        {
            TEntity result = null;

            if (_context.InputParameters.Contains("Target"))
            {
                Entity etn = null;

                if (_context.InputParameters["Target"] is Entity)
                {
                    etn = ((Entity)_context.InputParameters["Target"]);
                }
                else if (_context.InputParameters["Target"] is EntityReference)
                {
                    etn = Retrieve((EntityReference)_context.InputParameters["Target"]);
                }

                if (etn != null && typeof(TEntity) != typeof(Entity))
                {
                    result = etn.ToEntity<TEntity>();
                }
                else
                {
                    result = (TEntity)etn;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the reference.
        /// </summary>
        /// <returns></returns>
        private EntityReference GetReference()
        {
            EntityReference result = null;

            if (_context.InputParameters.Contains("Target"))
            {
                if (_context.InputParameters["Target"] is EntityReference)
                {
                    result = (EntityReference)_context.InputParameters["Target"];
                }
                else if (Target != null)
                {
                    TEntity target = Target;
                    EntityReference erResult = null;

                    if (_context.OutputParameters.Contains("id"))
                    {
                        Guid id = (Guid)_context.OutputParameters["id"];

                        erResult = new EntityReference(target.LogicalName, id);
                    }
                    else
                    {
                        erResult = target.ToEntityReference();
                    }

                    return erResult;
                }
            }
            else if (_context.InputParameters.Contains("EntityMoniker") &&
                _context.InputParameters["EntityMoniker"] is EntityReference)
            {
                result = (EntityReference)_context.InputParameters["EntityMoniker"];
            }

            return result;
        }

        /// <summary>
        /// Gets an entity image.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="imageCollection">The image collection.</param>
        /// <returns></returns>
        private Entity GetImage(string name, EntityImageCollection imageCollection)
        {
            Entity result = new Entity();

            if (imageCollection.Contains(name) &&
                imageCollection[name] is Entity)
            {
                result = (Entity)imageCollection[name];
            }

            return result;
        }

        /// <summary>
        /// Tries to get an entity image.
        /// </summary>
        /// <param name="name">The name of the image.</param>
        /// <param name="imageCollection">The image collection.</param>
        /// <param name="etn">The entity image.</param>
        /// <returns></returns>
        private bool TryGetImage(string name, EntityImageCollection imageCollection, out Entity etn)
        {
            bool result = false;
            etn = new Entity();

            if (imageCollection.Contains(name) &&
                            imageCollection[name] is Entity)
            {
                etn = (Entity)imageCollection[name];
                result = true;
            }

            return result;
        }

        public List<Entity> RetrieveRecordsByLookupView(
            string entityName,
            IEnumerable<KeyValuePair<string, object>> recordFilter = null
        )
        {
            return RetrieveRecordsByView(
                entityName,
                new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("querytype", 64)
                }, recordFilter);
        }

        public List<Entity> RetrieveRecordsByViewId(
            string entityName,
            Guid id,
            IEnumerable<KeyValuePair<string, object>> recordFilter = null
        )
        {
            return RetrieveRecordsByView(
                entityName,
                new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("savedqueryid", id)
                }, recordFilter);
        }

        public List<Entity> RetrieveRecordsByView(
            string entityName,
            string name,
            IEnumerable<KeyValuePair<string, object>> recordFilter = null
        )
        {
            return RetrieveRecordsByView(
                entityName,
                new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("name", name)
                }, recordFilter);
        }

        public List<Entity> RetrieveRecordsByView(
            string entityName,
            IEnumerable<KeyValuePair<string, object>> filter,
            IEnumerable<KeyValuePair<string, object>> recordFilter = null
        )
        {
            Entity query = GetViewDefinition(entityName, filter);

            return RetrieveRecordsByView(query, recordFilter);
        }

        public Entity GetViewDefinition(string entityName, string viewName)
        {
            return GetViewDefinition(
                entityName,
                new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("name", viewName)
                });
        }

        public Entity GetViewDefinition(string entityName, IEnumerable<KeyValuePair<string, object>> filter)
        {
            var query = new Entity();
            var metadata = GetEntityMetadata(entityName);

            if (metadata != null)
            {
                var fullFilter =
                    new List<KeyValuePair<string, object>>();

                fullFilter.AddRange(
                    new KeyValuePair<string, object>[] {
                    new KeyValuePair<string, object>("returnedtypecode", metadata.ObjectTypeCode.Value),
                    new KeyValuePair<string, object>("statecode", 0)
                    });

                fullFilter.AddRange(filter);

                query =
                    RetrieveByAttribute(
                        "savedquery",
                        new string[] { "fetchxml", "layoutxml" },
                        fullFilter.ToArray()
                    );
            }

            return query;
        }

        private EntityMetadata GetEntityMetadata(string entityName)
        {
            RetrieveEntityRequest req = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = false,
                LogicalName = entityName
            };

            RetrieveEntityResponse resp = Execute(req) as RetrieveEntityResponse;

            if (resp != null && resp.EntityMetadata != null)
            {
                return resp.EntityMetadata;
            }
            else return null;
        }

        public List<Entity> RetrieveRecordsByView(
            Entity query,
            IEnumerable<KeyValuePair<string, object>> filter
        )
        {
            var result = new List<Entity>();

            if (query != null && query.Contains("fetchxml"))
            {
                string fetchXml = query.GetAttributeValue<string>("fetchxml");

                if (!String.IsNullOrEmpty(fetchXml))
                {
                    if (filter != null)
                    {
                        XDocument xdoc = XDocument.Parse(fetchXml);

                        using (StringWriter sw = new StringWriter())
                        {
                            // Merge in record filters
                            var elemEtn = xdoc.Root.Element("entity");
                            var elemFilter = new XElement("filter");

                            foreach (var pair in filter)
                            {
                                var elemCondition = new XElement("condition");
                                elemCondition.SetAttributeValue("attribute", pair.Key);
                                elemCondition.SetAttributeValue("operator", "eq");
                                elemCondition.SetAttributeValue("value", pair.Value);

                                elemFilter.Add(elemCondition);
                            }

                            elemEtn.Add(elemFilter);

                            xdoc.Save(sw);

                            fetchXml = sw.ToString();
                        }
                    }

                    result = RetrieveAll(fetchXml);
                }
            }

            return result;
        }

        internal Entity RetrieveByAttribute(string logicalName, string[] columns, IEnumerable<KeyValuePair<string, object>> values)
        {
            Entity result = new Entity();

            StringBuilder sbAttributes = new StringBuilder();
            StringBuilder sbConditions = new StringBuilder();

            if (columns == null)
                sbAttributes.Append("<all-attributes />");
            else
            {
                foreach (string column in columns)
                    sbAttributes.AppendFormat("<attribute name='{0}' />", CleanBrackets(column));
            }

            foreach (KeyValuePair<string, object> pair in values)
                sbConditions.Append(
                    CleanBrackets(
                        String.Format("<condition attribute='{0}' operator='eq' value='{1}' />", pair.Key, pair.Value)));

            List<Entity> records =
                RetrieveAll(
                    String.Format(
                        @"<fetch>
                            <entity name='{0}'>
                                {1}
                                
                                <filter>
                                    {2}
                                </filter>
                            </entity>
                          </fetch>", logicalName, sbAttributes.ToString(), sbConditions.ToString()
                    )
                );

            if (records != null && records.Count > 0)
                result = records.First();

            return result;
        }

        private string CleanBrackets(string input)
        {
            return String.IsNullOrEmpty(input) ? input : input.Replace("{", "{{").Replace("}", "}}");
        }
    }
}