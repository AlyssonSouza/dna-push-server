/*******************************************************
* PushNotificationEntity.cs -- Copyrights Moonlighters.* 
*                                                      *
* Author:  Campos, Daniel                              *
*                                                      *
* Purpose:  Mapping of PushNotification                *
*                                                      *
* Usage: N/A                                           *
*                                                      *
********************************************************/


using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Model.Entities
{
    /// <summary>
    /// push notification entity
    /// </summary>
    public class PushNotificationEntity : Entity
    {
        /// <summary>
        /// title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// push image
        /// </summary>
        public byte[] PushImage { get; set; }
        /// <summary>
        /// push image
        /// </summary>
        public byte[] ThumbImage { get; set; }
        /// <summary>
        /// offer start date
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// offer end date
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// send push only to logges customers
        /// </summary>
        public bool? SendToLogged { get; set; }
        /// <summary>
        /// push start age group year
        /// </summary>
        public DateTime? StartAgeYear { get; set; }
        /// <summary>
        /// push end age group year
        /// </summary>
        public DateTime? EndAgeYear { get; set; }
        /// <summary>
        /// push sent to specific customer gender
        /// </summary>
        public int? Gender { get; set; }
        /// <summary>
        /// defines if the push was sent
        /// </summary>
        public bool IsPushSent { get; set; }
        /// <summary>
        /// macro region id
        /// </summary>
        public int? MacroRegionId { get; set; }
        /// <summary>
        /// region id
        /// </summary>
        public int? RegionId { get; set; }
        /// <summary>
        /// city
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// state name
        /// </summary>
        [XmlIgnore]
        public string StateName { get; set; }
        [XmlIgnore]
        public int? OfferId { get; set; }
    }
}
