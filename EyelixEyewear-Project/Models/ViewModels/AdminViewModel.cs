namespace EyelixEyewear_Project.Models.ViewModels
{
    // Dashboard ViewModel
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public int TotalProducts { get; set; }
        public int PendingOrders { get; set; }
    }

    // Order ViewModel
    public class AdminOrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
    }

    // Product ViewModel
    public class AdminProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Stock { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }
    }

    // Customer ViewModel
    public class AdminCustomerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
    }
}