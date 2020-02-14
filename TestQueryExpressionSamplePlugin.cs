using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
namespace TestQueryExpressionSamplePlugin
{
    public class Class1:IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext pluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory organizationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService organizationService = organizationServiceFactory.CreateOrganizationService(pluginContext.UserId);

            string fetchxml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='bupa_testentity'>
    <attribute name='bupa_testentityid' />
    <attribute name='bupa_name' />
    <attribute name='createdon' />
  </entity>
</fetch>";

            if (pluginContext.MessageName=="Create" && pluginContext.Stage==20)
            {
                string fetch = fetchxml;
                tracingService.Trace("PrimaryEntityName: " + pluginContext.PrimaryEntityName);
                //throw new InvalidPluginExecutionException("Reached");
                if (pluginContext.InputParameters.Contains("Target"))
                {
                    //throw new InvalidPluginExecutionException("Reached1");
                    //Entity e = (Entity)pluginContext.InputParameters["Target"];
                    //QueryExpression q = new QueryExpression(e.LogicalName);
                    //q.ColumnSet=new ColumnSet(true);
                    //EntityCollection ec= organizationService.RetrieveMultiple(q);
                    List<Entity> a = GetTotalRecordsFetchXML(organizationService, fetchxml);
                    throw new InvalidPluginExecutionException("Count: " + a.Count);
                }

            }

            

        }
        public List<Entity> RetrieveAllRecords(IOrganizationService organizationService, string fetch)
        {
            var moreRecords = false;
            int page = 1;
            var cookie = string.Empty;
            List<Entity> Entities = new List<Entity>();
            //EntityCollection Entities = new EntityCollection();
            do
            {
                var xml = string.Format(fetch, cookie);
                var collection = organizationService.RetrieveMultiple(new FetchExpression(xml));

                if (collection.Entities.Count >= 0) Entities.AddRange(collection.Entities);

                moreRecords = collection.MoreRecords;
                if (moreRecords)
                {
                    page++;
                    cookie = string.Format("paging-cookie='{0}' page='{1}'", System.Security.SecurityElement.Escape(collection.PagingCookie), page);
                }
            } while (moreRecords);

            return Entities;
        }
        private static List<Entity> GetTotalRecordsFetchXML(IOrganizationService organizationService, string fetchXML)
        {
            XDocument xDocument = XDocument.Parse(fetchXML);
            var fetchXmlEntity = xDocument.Root.Element("entity").ToString();

            EntityCollection entityColl = new EntityCollection();
            List<Entity> lstEntity = new List<Entity>();
            int page = 1;
            do
            {

                entityColl = organizationService.RetrieveMultiple(new FetchExpression(
                string.Format("<fetch version='1.0' page='{1}' paging-cookie='{0}'>" + fetchXmlEntity + "</fetch>",
                SecurityElement.Escape(entityColl.PagingCookie), page++)));

                lstEntity.AddRange(entityColl.Entities);
            }
            while (entityColl.MoreRecords);

            return lstEntity;
        }
    }
}
