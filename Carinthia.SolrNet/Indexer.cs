using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using Carinthia.Domain;

namespace Carinthia.SolrNet
{
    public class Indexer
    {
        public bool IndexContact(Contact c)
        {
            new SolrBaseRepository.Instance<Contact>().Start();
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Contact>>();

            if (c.AttachmentPath != "")     // Commit with the ExtractingRequestHandler (Tika)
            {
                using (var fileStream = System.IO.File.OpenRead(c.AttachmentPath))
                {
                    var response = solr.Extract(
                            new ExtractParameters(fileStream, c.ContStub)
                            {
                                ExtractFormat = ExtractFormat.Text,
                                ExtractOnly = false,
                                AutoCommit = true,
                                Fields = new[] { 
                                                    new ExtractField("acct_id", c.AccountId.ToString()),
                                                    new ExtractField("cont_stub", c.ContStub),
                                                    new ExtractField("cont_type_name", c.ContType),
                                                    new ExtractField("email_addr", c.EmailAddr),
                                                    new ExtractField("cont_first_name", c.FirstName),
                                                    new ExtractField("cont_last_name", c.LastName),
                                                    new ExtractField("cont_note_text", c.NoteText),
                                                    new ExtractField("cont_source_id", c.SourceId),
                                                    new ExtractField("registration_date", c.ContRegDate),
                                                }
                            });
                }
            }

            else       // Use the normal UpdateHandler
            {
                try
                {
                    solr.Add(c);
                    solr.Commit();
                }

                catch (Exception ex)
                {
                    //HttpContext.Current.Response.Write("<b>Error committing to Solr - </b><br/>"
                    //    + ex.Message);
                    return false;
                }
            }
            return true;
        }
    }
}