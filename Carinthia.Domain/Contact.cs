using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SolrNet.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Carinthia.Domain
{
    /// <summary>
    /// Contact Entity
    /// </summary>
    public class Contact
    {
        [SolrField("acct_id")]
        public int AccountId { get; set; }

        public int SerialNo { get; set; }

        [SolrUniqueKey("cont_stub")]
        public string ContStub { get; set; }

        [DisplayName("Email Address")]
        [SolrField("email_addr")]
        public string EmailAddr { get; set; }

        [DisplayName("First Name")]
        [SolrField("cont_first_name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [SolrField("cont_last_name")]
        public string LastName { get; set; }

        [DisplayName("Source Id")]
        [SolrField("cont_source_id")]
        public string SourceId { get; set; }

        [DisplayName("Contact Type")]
        [SolrField("cont_type_name")]
        public string ContType { get; set; }

        [DisplayName("Note")]
        [DataType(DataType.MultilineText)]
        [SolrField("cont_note_text")]
        public string NoteText { get; set; }

        [DisplayName("Registration Date")]
        [SolrField("registration_date")]
        public string ContRegDate { get; set; }

        [SolrField("gender_name")]
        public string ContGender { get; set; }

        [SolrField("home_state_name")]
        public string ContState { get; set; }

        [SolrField("home_city")]
        public string ContCity { get; set; }

        [SolrField("score")]
        public decimal? ResultScore { get; set; }

        public string AttachmentPath { get; set; }
    }
}