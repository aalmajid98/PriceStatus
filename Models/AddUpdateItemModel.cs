namespace PriceStatus_Items.Models
{
    public class AddUpdateItemModel
    {
        public int UpdateID { get; set; }
        public string UpdateName { get; set; }
        public int UpdateCost { get; set; }

        public string AddName { get; set; }
        public int AddCost {get; set; }
    }
}