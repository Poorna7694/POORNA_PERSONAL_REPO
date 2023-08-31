using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Contact_Pre_Valdiation

{
    public class Contact_Plugin : IPlugin
    {
        public void Execute(IServiceProvider sp)
        {
            ITracingService trace = (ITracingService)sp.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)sp.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)sp.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            trace.Trace("The Logged in USER ID is " + context.UserId);
            #region VALDIATIONS
            if (context.MessageName.ToLower().ToString() != "create")
                return;
            trace.Trace(context.MessageName.ToLower().ToString());
            if (context.Stage != 10)
                return;
            trace.Trace(context.Stage.ToString());
            if (context.PrimaryEntityName.ToLower().ToString() != "contact")
                return;
            trace.Trace(context.PrimaryEntityName.ToLower().ToString());
            if (!context.InputParameters.Contains("Target"))
                return;
            trace.Trace("Context Is Carrying Data From FE");
            if (!(context.InputParameters["Target"] is Entity))
                return;
            trace.Trace("Context is Carrying The Data in The Format Of Entity");

            trace.Trace("All Valdiations Success .... going to try block");
            #endregion
            try
            {
                Entity uiContact = (Entity)context.InputParameters["Target"] as Entity;
                if (uiContact != null)
                {
                    #region Reading FE Data

                    if (!uiContact.Contains("firstname") || uiContact["firstname"] == null)

                        throw new InvalidPluginExecutionException("FIRST NAME Is missing Please Provide");
                    if (!uiContact.Contains("lastname") || uiContact["lastname"] == null)
                        return;

                    if (!uiContact.Contains("emailaddress1") || uiContact["emailaddress1"] == null)

                        throw new InvalidPluginExecutionException("Email Id Is missing Please Provide");
                    string fname = uiContact.GetAttributeValue<string>("firstname");

                    string lname = (string)uiContact["lastname"];

                    string email = uiContact.GetAttributeValue<string>("emailaddress1");

                    trace.Trace("First Name is " + email);

                    #endregion

                    #region Business Logic Starts From Here

                    string query = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                        <entity name='contact'>
                     <attribute name='fullname' />
                      <attribute name='telephone1' />
                         <attribute name='contactid' />
                      <order attribute='fullname' descending='false' />
                          <filter type='and'>
                      <condition attribute='emailaddress1' operator='eq' value='{email}' />
                           </filter>
                         </entity>
                         </fetch>";

                    EntityCollection contactList = service.RetrieveMultiple(new FetchExpression(query));

                    trace.Trace("We Found These Many Contacts" + contactList.Entities.Count + " With This Email Id is " + email);
                    if (contactList.Entities.Count > 0)
                        throw new InvalidPluginExecutionException("Email Id Is Already Existed  Please Provide Another One");

                    #endregion

                }

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(" We got error and it is " + ex.Message.ToString());

            }

        }
    }
}
