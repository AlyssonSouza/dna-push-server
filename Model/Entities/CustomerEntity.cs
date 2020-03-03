/*******************************************************
* CustomerEntity.cs -- Copyrights Moonlighters.        * 
*                                                      *
* Author:  Campos, Daniel                              *
*                                                      *
* Purpose:  Customer Mapping                           *
*                                                      *
* Usage: N/A                                           *
*                                                      *
********************************************************/

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Model.Entities
{
    /// <summary>
    /// customer entity
    /// </summary>
    public class CustomerEntity : Entity
    {
        /// <summary>
        /// Customer name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Customer email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// IsCustomerDev
        /// </summary>
        public bool? IsCustomerDev { get; set; } = false;
        /// <summary>
        /// customer gender
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Customer birthdate
        /// </summary>
        public string Birthdate { get; set; }
        /// <summary>
        /// Customer phone
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// Customer HomePhone
        /// </summary>
        public string HomePhone { get; set; }
        /// <summary>
        /// customer's password
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public string Password { get; set; }
        /// <summary>
        /// customer's document
        /// </summary>        
        public string Document { get; set; }
        [JsonIgnore]
        /// <summary>
        /// region id
        /// </summary>
        public int? RegionId { get; set; }
        /// <summary>
        /// city
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// password salt
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        public string Salt { get; set; }
        [JsonIgnore]
        /// <summary>
        /// has changed password
        /// </summary>
        [XmlIgnore]
        public bool HasChangedPassword { get; set; }
        [JsonIgnore]
        /// <summary>
        /// creation date
        /// </summary>
        [XmlIgnore]
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// zip code
        /// </summary>
        public string ZipCode { get; set; }
        /// <summary>
        /// address
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// number
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// complement
        /// </summary>
        public string Complement { get; set; }
        /// <summary>
        /// system id
        /// </summary>
        public int? SystemId { get; set; }
        [JsonIgnore]
        /// <summary>
        /// payment id
        /// </summary>
        public string PaymentId { get; set; }
        [JsonIgnore]
        /// <summary>
        /// district
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// placeOfBirth
        /// </summary>
        public string placeOfBirth { get; set; }
        [JsonIgnore]
        /// <summary>
        /// facebook id
        /// </summary>
        public string FacebookId { get; set; }
        [JsonIgnore]
        /// <summary>
        /// Image
        /// </summary>
        public byte[] Image { get; set; }
        [JsonIgnore]
        /// <summary>
        /// regional document
        /// </summary>
        public string RegionalDocument { get; set; }
        [JsonIgnore]
        /// <summary>
        /// regional document state
        /// </summary>
        public string RegionalDocumentState { get; set; }
        [JsonIgnore]
        /// <summary>
        /// address id
        /// </summary>
        public string AddressId { get; set; }
        /// <summary>
        /// phone id
        /// </summary>
        public string PhoneId { get; set; }
        [JsonIgnore]
        /// <summary>
        /// Appkey
        /// </summary>
        public string AppKey { get; set; }
        [JsonIgnore]
        /// <summary>
        /// idControl
        /// </summary>
        public string idControl { get; set; }
        [JsonIgnore]
        /// <summary>
        /// status
        /// </summary>
        public string status { get; set; }
        [JsonIgnore]
        /// <summary>
        /// idControl
        /// </summary>
        public string processMessage { get; set; }
        [JsonIgnore]
        public string State { get; set; }

        //Navigation properties
        [XmlIgnore]
        [JsonIgnore]
        public virtual List<DeviceEntity> Device { get; set; }
    }

}
