using System;

namespace McbeevCommerceBot.Models
{
    /// <summary>
    /// Auto-gen class from Visual Studio's Paste JSON as Class feature from the Kentico JSON
    /// Just remember to rename RootObject to what you want, thanks @themarkschmidt!
    /// </summary>
    public class SKUResponse
    {
        public Ecommerce_Skus[] ecommerce_skus { get; set; }
    }

    public class Ecommerce_Skus
    {
        public COM_SKU[] COM_SKU { get; set; }
        public Totalrecord[] TotalRecords { get; set; }
    }

    public class COM_SKU
    {
        public object SKUCollectionID { get; set; }
        public object SKURetailPrice { get; set; }
        public int SKUManufacturerID { get; set; }
        public string SKUDescription { get; set; }
        public string SKUShortDescription { get; set; }
        public float SKUWeight { get; set; }
        public object SKUSupplierID { get; set; }
        public object SKUBundleInventoryType { get; set; }
        public object SKUWidth { get; set; }
        public int SKUPublicStatusID { get; set; }
        public string SKUProductType { get; set; }
        public int SKUTaxClassID { get; set; }
        public object SKUAvailableInDays { get; set; }
        public bool SKUNeedsShipping { get; set; }
        public DateTime SKUCreated { get; set; }
        public object SKUCustomData { get; set; }
        public object SKUValidity { get; set; }
        public string SKUGUID { get; set; }
        public object SKUMaxItemsInOrder { get; set; }
        public bool SKUSellOnlyAvailable { get; set; }
        public bool SKUEnabled { get; set; }
        public string SKUTrackInventory { get; set; }
        public object SKUOptionCategoryID { get; set; }
        public object SKUHeight { get; set; }
        public int SKUAvailableItems { get; set; }
        public object SKUEproductFilesCount { get; set; }
        public object SKUMembershipGUID { get; set; }
        public string SKUImagePath { get; set; }
        public object SKUMinItemsInOrder { get; set; }
        public DateTime SKUInStoreFrom { get; set; }
        public string SKUNumber { get; set; }
        public object SKUValidFor { get; set; }
        public object SKUBrandID { get; set; }
        public int SKUID { get; set; }
        public object SKUConversionName { get; set; }
        public object SKUDepth { get; set; }
        public object SKUOrder { get; set; }
        public object SKUBundleItemsCount { get; set; }
        public object SKUReorderAt { get; set; }
        public object SKUInternalStatusID { get; set; }
        public int SKUSiteID { get; set; }
        public float SKUPrice { get; set; }
        public string SKUConversionValue { get; set; }
        public object SKUParentSKUID { get; set; }
        public object SKUValidUntil { get; set; }
        public int SKUDepartmentID { get; set; }
        public string SKUName { get; set; }
        public DateTime SKULastModified { get; set; }
    }
}
