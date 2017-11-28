
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using ResearchPortal.Crm.Fetch;

namespace ResearchPortal
{
    public static partial class Extensions
    {


        /// <summary>
        /// Given a fetchxml string, return a list of records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="fetchXML"></param>
        /// <returns></returns>
        public static HashSet<T> FetchCompleteList<T>(this IOrganizationService service, string fetchXML, bool stopAtFirstPage = false) where T : Entity
        {
            HashSet<T> entities = new HashSet<T>();
            FetchType fetch = Extensions.DeserializeXml<FetchType>(fetchXML);
            int pageNumber = 1;
            if (!fetch.aggregate)
            {
                fetch.returntotalrecordcount = true;
            }
            if (!string.IsNullOrEmpty(fetch.count))
            {
                stopAtFirstPage = true;
            }
            fetch.nolock = true;

            EntityCollection ec = null;

            FetchExpression fe = new FetchExpression(fetch.SerializeXML());
            ec = service.RetrieveMultiple(fe);

            entities.UnionWith(ec.Entities.Select(e => e.ToEntity<T>()));

            if (stopAtFirstPage)
            {
                return entities;
            }
            while (ec.MoreRecords)
            {
                pageNumber++;

                fetch.page = pageNumber.ToString();
                fetch.pagingcookie = ec.PagingCookie;

                fe = new FetchExpression(fetch.SerializeXML());

                ec = service.RetrieveMultiple(fe);
                entities.UnionWith(ec.Entities.Select(e => e.ToEntity<T>()));
            }
            return entities;
        }

        /// <summary>
        /// Fetch
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service"></param>
        /// <param name="fetchXML"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="pagingCookie"></param>
        /// <returns></returns>
        public static IEnumerable<T> FetchPage<T>(this IOrganizationService service, string fetchXML, int pageSize, int page, ref string pagingCookie) where T : Entity
        {
            HashSet<T> entities = new HashSet<T>();
            FetchType fetch = Extensions.DeserializeXml<FetchType>(fetchXML);
            if (!fetch.aggregate)
            {
                fetch.returntotalrecordcount = true;
            }
            fetch.count = pageSize.ToString();
            fetch.pagingcookie = pagingCookie;

            fetch.nolock = true;

            EntityCollection ec = null;

            FetchExpression fe = new FetchExpression(fetch.SerializeXML());
            ec = service.RetrieveMultiple(fe);

            pagingCookie = ec.PagingCookie;

            return ec.Entities.Select(e => e.ToEntity<T>());
        }

        /// <summary>
        /// Returns a dictionary list of all the linked entities.  Where the key is
        /// linked enitty alias, and the value is the Entity that represents that alias.
        /// </summary>
        /// <param name="baseEntity"></param>
        /// <returns></returns>
        public static Dictionary<string, Entity> ExtractLinkedEntities(this Entity baseEntity)
        {
            Dictionary<string, Entity> ret = new Dictionary<string, Entity>();

            var aliasedKeys = baseEntity.Attributes.Keys
              .Where(k => k.Contains("."));
            var aliasPrefixes = aliasedKeys
                .Select(k => k.Substring(0, k.IndexOf("."))
                .Distinct());

            foreach (string aliasPrefix in aliasPrefixes)
            {
                foreach (string aliasKey in aliasedKeys.Where(k => k.StartsWith(aliasPrefix + ".")))
                {
                    if (!ret.ContainsKey(aliasPrefix))
                    {
                        ret[aliasPrefix] = new Entity();
                    }
                    AliasedValue av = baseEntity[aliasKey] as AliasedValue;
                    ret[aliasPrefix].LogicalName = av.EntityLogicalName;
                    ret[aliasPrefix][av.AttributeLogicalName] = av.Value;
                }
            }
            return ret;
        }
        public static object GetLookupKeyAttribute(this Entity entity, string attributeLogicalName, string key)
        {
            if (!entity.Contains(attributeLogicalName))
            {
                return null;
            }
            var er = entity[attributeLogicalName] as EntityReference;
            if (er == null)
            {
                return null;
            }

            return er.KeyAttributes[key].ToString();
        }
        public static void SetLookupKeyAttribute(this Entity entity, string entityLogicalName, string attributeLogicalName, string key, object value)
        {
            if (!entity.Contains(attributeLogicalName))
            {
                entity[attributeLogicalName] = new EntityReference(entityLogicalName);
            }
            var er = entity[attributeLogicalName] as EntityReference;

            er.KeyAttributes[key] = value;
        }
    }
}
