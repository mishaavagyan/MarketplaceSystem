namespace OrderService.BLL.Models.DTO
{
    public class OrderUpdatedEvent
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string BuyerEmail { get; set; } = string.Empty;
        public string SellerEmail { get; set; } = string.Empty;
    }
}
    