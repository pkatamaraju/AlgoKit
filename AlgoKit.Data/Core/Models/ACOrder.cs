using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoKit.Data.Core.Models
{
    public class ACOrder
    {
        [JsonProperty(PropertyName = "orderID")]
        public int OrderID { get; set; }

        [JsonProperty(PropertyName = "userID")]
        public int UserID { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "createdDate")]
        public string CreatedDate { get; set; }

        [JsonProperty(PropertyName = "pincode")]
        public string Pincode { get; set; }

        [JsonProperty(PropertyName = "deliveryAddress")]
        public string DeliveryAddress { get; set; }

        [JsonProperty(PropertyName = "phoneNumber")]
        public Int64 PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "stateID")]
        public Int64 StateID { get; set; }

        [JsonProperty(PropertyName = "stateName")]
        public string StateName { get; set; }


        [JsonProperty(PropertyName = "orderValue")]
        public int OrderValue { get; set; }

        [JsonProperty(PropertyName = "ShippingAmount")]
        public int ShippingAmount { get; set; }

        [JsonProperty(PropertyName = "statusID")]
        public Int64 StatusID { get; set; }

        [JsonProperty(PropertyName = "statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty(PropertyName = "totalPageCount")]
        public string TotalPageCount { get; set; }

        [JsonProperty(PropertyName = "referralAmountUsed")]
        public int ReferralAmountUsed { get; set; }

        [JsonProperty(PropertyName = "selectedShippingType")]
        public string SelectedShippingType { get; set; }

        [JsonProperty(PropertyName = "fromAddressReseller")]
        public string FromAddressReseller { get; set; }


        [JsonProperty(PropertyName = "comboOfferAmount")]
        public int ComboOfferAmount { get; set; }
        

    }
}
