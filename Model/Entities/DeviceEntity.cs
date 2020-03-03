/*******************************************************
* DeviceEntity.cs -- Copyrights Moonlighters.          * 
*                                                      *
* Author:  Campos, Daniel                              *
*                                                      *
* Purpose:  Mapping of Device                          *
*                                                      *
* Usage: N/A                                           *
*                                                      *
********************************************************/


using System.Xml.Serialization;

namespace Model.Entities
{
    /// <summary>
    /// device entity
    /// </summary>
    public class DeviceEntity : Entity
    {
        /// <summary>
        /// device token id
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// device platform
        /// </summary>
        public int Platform { get; set; }
        /// <summary>
        /// customer id
        /// </summary>
        public int? CustomerId { get; set; }

        //Navigation properties
        [XmlIgnore]
        public virtual CustomerEntity Customer { get; set; }
    }
}
