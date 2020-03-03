/*******************************************************
* PushNotificationBusiness.cs -- Copyrights Moon       * 
*                                                      *
* Author:  Campos, Daniel                              *
*                                                      *
* Purpose:  PushNotification business                  *
*                                                      *
* Usage: N/A                                           *
*                                                      *
********************************************************/


using Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Business
{
    /// <summary>
    /// push notification business
    /// </summary>
    public class PushNotificationBusiness
    {
        Logger log = new Logger();
        CultureInfo _culture = CultureInfo.CreateSpecificCulture("pt-BR");
        DateTimeStyles _styles = DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeLocal;

        /// <summary>
        /// creates a new instance of dao
        /// </summary>
        /// <returns>pushnotification dao</returns>
        protected override IDao<PushNotificationEntity> CreateDao()
        {
            return (new PushNotificationDao());
        }

        /// <summary>
        /// get all non logged push notification by shopping
        /// </summary>        
        /// <returns>push list</returns>
        public List<PushNotificationEntity> GetNonLogged()
        {
            DateTime currentDate = CalculateLocalTime();
            List<PushNotificationEntity> pushLst = ((PushNotificationDao)_dao).Get(currentDate);
            pushLst = pushLst.Where(p => p.SendToLogged == null || p.SendToLogged.Value == false).ToList();
            return (pushLst);
        }

        /// <summary>
        /// get all pushs to specific user
        /// </summary>        
        /// <param name="customer">customer id</param>
        /// <returns></returns>
        public List<PushNotificationEntity> Get(CustomerEntity customer)
        {
            CustomerBusiness customerBusiness = new CustomerBusiness();
            CustomerEntity customerEntity = customerBusiness.Get(customer.Id);
            List<PushNotificationEntity> generalPushLst = new List<PushNotificationEntity>();
            DateTime currentDate = CalculateLocalTime();
            DeviceBusiness deviceBusiness = new DeviceBusiness();
            List<PushNotificationEntity> removedLst = new List<PushNotificationEntity>();

            List<PushNotificationEntity> pushLst = ((PushNotificationDao)_dao).Get(currentDate);


            if (pushLst.Count > 0)
            {
                generalPushLst = pushLst.Where(p => p.SendToLogged == null || p.SendToLogged.Value == false).ToList();
                if (null != customerEntity)
                {
                    DateTime birthday;
                    int gender = 0;

                    if (!string.IsNullOrWhiteSpace(customerEntity.Gender))
                    {
                        switch (customerEntity.Gender.ToLower())
                        {
                            case "masculino":
                                gender = 1;
                                break;
                            case "feminino":
                                gender = 2;
                                break;
                        }
                    }
                    //if (null != customerEntity && customerEntity.Birthdate.Length < 10)
                    //    customerEntity.Birthdate = customerEntity.Birthdate.Insert(3, "0");                                                  

                    foreach (PushNotificationEntity push in pushLst)
                    {
                        int? startAge = null;
                        int? endAge = null;
                        int? customerAge = null;
                        if (push.StartAgeYear.HasValue && push.EndAgeYear.HasValue)
                        {
                            if (!string.IsNullOrWhiteSpace(customerEntity.Birthdate))
                            {
                                if (DateTime.TryParseExact(customerEntity.Birthdate, "dd/MM/yyyy", _culture, _styles, out birthday))
                                {
                                    startAge = DateTime.Now.Year - push.StartAgeYear.Value.Year;
                                    endAge = DateTime.Now.Year - push.EndAgeYear.Value.Year;
                                    customerAge = DateTime.Now.Year - birthday.Year;
                                }
                            }
                            else
                                continue;
                        }

                        if (push.SendToLogged.HasValue && push.SendToLogged.Value)
                        {
                            if (!push.StartAgeYear.HasValue && !push.EndAgeYear.HasValue && !push.Gender.HasValue &&
                                !push.MacroRegionId.HasValue && !push.RegionId.HasValue && string.IsNullOrWhiteSpace(push.City))
                            {
                                generalPushLst.Add(push);
                            }
                            else
                            {
                                int customerMacroRegion = 0;
                                if (customerEntity.RegionId.HasValue)
                                    customerMacroRegion = GetMacroRegion(customerEntity.RegionId.Value);

                                //All filters enabled

                                if (customerAge.HasValue && push.StartAgeYear.HasValue && push.EndAgeYear.HasValue && push.Gender.HasValue && push.MacroRegionId.HasValue)
                                {
                                    if ((startAge <= customerAge && customerAge <= endAge) &&
                                        push.Gender.Value == gender)
                                    {
                                        if (customerMacroRegion > 0 && push.MacroRegionId.Value == customerMacroRegion)
                                        {
                                            if (push.RegionId.HasValue)
                                            {
                                                if (push.RegionId.Value == customerEntity.RegionId.Value)
                                                {
                                                    if (!string.IsNullOrEmpty(push.City))
                                                    {
                                                        if (push.City.ToLower().Equals(customerEntity.City.ToLower()))
                                                            generalPushLst.Add(push);
                                                    }
                                                    else
                                                        generalPushLst.Add(push);
                                                }
                                            }
                                            else
                                                generalPushLst.Add(push);
                                        }
                                    }
                                }
                                else
                                {
                                    bool added = false;
                                    //only age enabled
                                    if ((push.StartAgeYear.HasValue && push.EndAgeYear.HasValue) && customerAge.HasValue && !push.Gender.HasValue && !push.MacroRegionId.HasValue)
                                    {
                                        if (startAge <= customerAge && customerAge <= endAge)
                                            generalPushLst.Add(push);

                                        added = true;
                                    }

                                    //only gender enabled
                                    if ((!push.StartAgeYear.HasValue && !push.EndAgeYear.HasValue) && push.Gender.HasValue && !push.MacroRegionId.HasValue)
                                    {
                                        if (push.Gender.Value == gender)
                                            generalPushLst.Add(push);

                                        added = true;
                                    }

                                    //only region enabled                                        
                                    if ((!push.StartAgeYear.HasValue && !push.EndAgeYear.HasValue) && !push.Gender.HasValue && push.MacroRegionId.HasValue)
                                    {
                                        if (customerMacroRegion > 0)
                                        {
                                            if (push.MacroRegionId.Value == customerMacroRegion)
                                            {
                                                if (push.RegionId.HasValue)
                                                {
                                                    if (push.RegionId.Value == customerEntity.RegionId.Value)
                                                    {
                                                        if (!string.IsNullOrEmpty(push.City))
                                                        {
                                                            if (push.City.ToLower().Equals(customerEntity.City.ToLower()))
                                                                generalPushLst.Add(push);
                                                        }
                                                        else
                                                            generalPushLst.Add(push);
                                                    }
                                                }
                                                else
                                                    generalPushLst.Add(push);
                                            }
                                        }
                                        added = true;
                                    }

                                    //only age and gender
                                    if ((push.StartAgeYear.HasValue && push.EndAgeYear.HasValue) && customerAge.HasValue && push.Gender.HasValue && !push.MacroRegionId.HasValue)
                                    {
                                        if (startAge <= customerAge && customerAge <= endAge &&
                                            push.Gender.Value == gender)
                                            generalPushLst.Add(push);

                                        added = true;
                                    }

                                    //only age and region
                                    if ((push.StartAgeYear.HasValue && push.EndAgeYear.HasValue) && customerAge.HasValue && !push.Gender.HasValue && push.MacroRegionId.HasValue)
                                    {
                                        if (startAge <= customerAge &&
                                            customerAge <= endAge &&
                                            customerMacroRegion > 0)
                                        {
                                            if (push.MacroRegionId.Value == customerMacroRegion)
                                            {
                                                if (push.RegionId.HasValue)
                                                {
                                                    if (push.RegionId.Value == customerEntity.RegionId.Value)
                                                    {
                                                        if (!string.IsNullOrEmpty(push.City))
                                                        {
                                                            if (push.City.ToLower().Equals(customerEntity.City.ToLower()))
                                                                generalPushLst.Add(push);
                                                        }
                                                        else
                                                            generalPushLst.Add(push);
                                                    }
                                                }
                                                else
                                                    generalPushLst.Add(push);

                                            }
                                        }
                                        added = true;
                                    }

                                    //only gender and region
                                    if ((!push.StartAgeYear.HasValue && !push.EndAgeYear.HasValue) && push.Gender.HasValue && push.MacroRegionId.HasValue)
                                    {
                                        if (push.Gender.Value == gender && customerMacroRegion > 0)
                                        {
                                            if (push.MacroRegionId.Value == customerMacroRegion)
                                            {
                                                if (push.RegionId.HasValue)
                                                {
                                                    if (push.RegionId.Value == customerEntity.RegionId.Value)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(push.City))
                                                        {
                                                            if (push.City.ToLower().Equals(customerEntity.City.ToLower()))
                                                                generalPushLst.Add(push);
                                                        }
                                                        else
                                                            generalPushLst.Add(push);
                                                    }
                                                }
                                                else
                                                    generalPushLst.Add(push);

                                            }
                                        }
                                        added = true;
                                    }

                                    if (!added)
                                        generalPushLst.Add(push);
                                }
                            }
                        }
                    }
                }
            }

            return (generalPushLst);

        }

        /// <summary>
        /// send push notification
        /// </summary>
        /// <param name="push">push to be sent</param>
        /// <returns>sent</returns>
        public bool SendPush(PushNotificationEntity push)
        {
            bool sent = false;

            Infrastructure.Push.PushHelper.CheckIosDeviceTokenAvailability();

            DeviceBusiness deviceBusiness = new DeviceBusiness();
            List<DeviceEntity> deviceLst = null;

            if (!push.OfferId.HasValue)
            {

                if (push.SendToLogged.HasValue && push.SendToLogged.Value)
                {
                    deviceLst = deviceBusiness.GetLogged();

                    CustomerBusiness customerBusiness = new CustomerBusiness();
                    List<CustomerEntity> customerLst = null;

                    if (push.StartAgeYear.HasValue && push.EndAgeYear.HasValue)
                    {
                        customerLst = customerBusiness.Get(push.StartAgeYear.Value, push.EndAgeYear.Value);
                    }
                    else
                        customerLst = customerBusiness.Get();

                    if (push.Gender.HasValue)
                    {
                        customerLst.RemoveAll(c => string.IsNullOrWhiteSpace(c.Gender));
                        switch (push.Gender.Value)
                        {
                            case 1:
                                customerLst = customerLst.Where(c => c.Gender.ToLower().Equals("m")).ToList();
                                break;
                            case 2:
                                customerLst = customerLst.Where(c => c.Gender.ToLower().Equals("f")).ToList();
                                break;
                        }
                    }

                    //filter customers by region
                    if (push.MacroRegionId.HasValue)
                    {
                        List<CustomerEntity> removedCustomerLst = new List<CustomerEntity>();
                        foreach (CustomerEntity customer in customerLst)
                        {
                            if (customer.RegionId.HasValue)
                            {
                                int customerMacroRegion = GetMacroRegion(customer.RegionId.Value);

                                if (customerMacroRegion != push.MacroRegionId.Value)
                                    removedCustomerLst.Add(customer);
                                else
                                {
                                    if (push.RegionId.HasValue && push.RegionId.Value != customer.RegionId.Value)
                                        removedCustomerLst.Add(customer);
                                    else if (!string.IsNullOrEmpty(push.City) && !push.City.ToLower().Equals(customer.City.ToLower()))
                                        removedCustomerLst.Add(customer);
                                }
                            }
                            else
                                removedCustomerLst.Add(customer);
                        }

                        foreach (CustomerEntity removedCustomer in removedCustomerLst)
                        {
                            customerLst.Remove(removedCustomer);
                        }
                    }

                    if (null != customerLst && customerLst.Count > 0)
                    {
                        List<int> customerIdList = new List<int>();
                        foreach (CustomerEntity item in customerLst)
                        {
                            customerIdList.Add(item.Id);
                        }

                        deviceLst = deviceBusiness.Get(customerIdList);
                        sent = Infrastructure.Push.PushHelper.SendPush(push, deviceLst);
                    }
                    else
                        sent = true;

                }
                else
                {
                    //deviceLst = deviceBusiness.Get();
                    deviceLst = deviceBusiness.Get().Where(o => o.CustomerId == 30).ToList();

                    sent = Infrastructure.Push.PushHelper.SendPush(push, deviceLst);
                }
            }
            else
            {
                List<int> customerIds = new List<int>();
                OfferBusiness offerBusiness = new OfferBusiness();
                OfferEntity pushAudience = offerBusiness.Get(push.OfferId.Value);

                if (null != pushAudience)
                {

                    if (pushAudience.AudienceType == (int)AudienceType.GENERAL)
                        deviceLst = deviceBusiness.Get();
                    else
                    {
                        if (pushAudience.CustomerId.HasValue)
                        {
                            customerIds.Add(pushAudience.CustomerId.Value);
                            deviceLst = deviceBusiness.Get(customerIds);
                        }
                    }

                    if (null != deviceLst && deviceLst.Count > 0)
                        sent = Infrastructure.Push.PushHelper.SendPush(push, deviceLst);
                }
            }



            if (sent)
            {
                PushNotificationEntity updtPush = Get(push.Id);
                updtPush.IsPushSent = sent;
                updtPush.ThumbImage = GetThumImage(push.Id);
                updtPush.PushImage = GetPushImage(push.Id);
                UpdatePushStatus(updtPush);
            }

            return (sent);
        }

        /// <summary>
        /// get push image
        /// </summary>
        /// <param name="pushId">push id</param>
        /// <returns>image bytes</returns>
        public byte[] GetPushImage(int pushId)
        {
            return (((PushNotificationDao)_dao).GetPushImage(pushId));
        }

        /// <summary>
        /// get push image
        /// </summary>
        /// <param name="pushId">push id</param>
        /// <returns>image bytes</returns>
        public byte[] GetThumImage(int pushId)
        {
            return (((PushNotificationDao)_dao).GetThumbImage(pushId));
        }

        /// <summary>
        /// mark push as read for specific user
        /// </summary>
        /// <param name="readedPush">readed push</param>
        public void InsertReaded(ReadedPushEntity readedPush)
        {
            readedPush.ReadDate = CalculateLocalTime();
            ((PushNotificationDao)_dao).InsertReaded(readedPush);
        }

        /// <summary>
        /// deleted readed push by push id
        /// </summary>
        /// <param name="pushId">push id</param>
        public void DeleteReaded(int pushId)
        {
            ((PushNotificationDao)_dao).DeleteReaded(pushId);
        }

        /// <summary>
        /// uodate push status
        /// </summary>
        /// <param name="push">push</param>
        public void UpdatePushStatus(PushNotificationEntity push)
        {
            ((PushNotificationDao)_dao).UpdatePushStatus(push);
        }

        /// <summary>
        /// get push by offer id
        /// </summary>
        /// <param name="offer">offer id</param>
        /// <returns>push list</returns>
        public List<PushNotificationEntity> Get(OfferEntity offer)
        {
            return (((PushNotificationDao)_dao).Get(offer));
        }

        /// <summary>
        /// deleted push by offer id
        /// </summary>
        /// <param name="offer">offer id</param>
        public void Delete(OfferEntity offer)
        {
            ((PushNotificationDao)_dao).Delete(offer);
        }

        /// <summary>
        /// deleted queued push by push id
        /// </summary>
        /// <param name="pushId">push id</param>
        public void DeleteQueue(int pushId)
        {
            ((PushNotificationDao)_dao).DeleteQueue(pushId);
        }

        /// <summary>
        /// het macro region base on region id
        /// </summary>
        /// <param name="regionId">region id</param>
        /// <returns>macro region</returns>
        private int GetMacroRegion(int regionId)
        {
            int customerMacroRegion = 0;
            switch (regionId)
            {
                case 1:
                case 3:
                case 4:
                case 14:
                case 22:
                case 23:
                case 27:
                    //North
                    customerMacroRegion = 1;
                    break;
                case 2:
                case 5:
                case 6:
                case 10:
                case 15:
                case 16:
                case 17:
                case 20:
                case 26:
                    //Northeast
                    customerMacroRegion = 2;
                    break;
                case 8:
                case 13:
                case 19:
                case 25:
                    //Southeast
                    customerMacroRegion = 3;
                    break;
                case 7:
                case 9:
                case 11:
                case 12:
                    //Midwest
                    customerMacroRegion = 4;
                    break;
                case 18:
                case 21:
                case 24:
                    //South
                    customerMacroRegion = 5;
                    break;
            }

            return (customerMacroRegion);
        }
    }
}
