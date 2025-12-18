using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EyelixEyewear_Project.Data;
using EyelixEyewear_Project.Models;
using EyelixEyewear_Project.Models.ViewModels;

namespace EyelixEyewear_Project.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // HELPER: Kiểm tra Admin đã đăng nhập chưa
        // ==========================================
        private bool IsAdminLoggedIn()
        {
            var isLoggedIn = HttpContext.Session.GetString("AdminLoggedIn");
            return isLoggedIn == "true";
        }

        // ==========================================
        // 0. LOGIN PAGE (Trang đăng nhập Admin)
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi → Redirect vào Dashboard
            if (IsAdminLoggedIn())
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Kiểm tra credentials (demo đơn giản)
            // TODO: Thay bằng check database với Role = "Admin"
            if (username == "admin" && password == "admin123")
            {
                // Lưu session
                HttpContext.Session.SetString("AdminLoggedIn", "true");
                HttpContext.Session.SetString("AdminUsername", username);

                return Json(new { success = true, message = "Login successful" });
            }

            return Json(new { success = false, message = "Invalid credentials" });
        }

        // ==========================================
        // LOGOUT
        // ==========================================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        // ==========================================
        // 1. DASHBOARD (Trang chủ Admin)
        // ==========================================
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())  // ← Check Session thay vì [Authorize]
            {
                return RedirectToAction("Login");
            }
            // Tính toán statistics
            var totalOrders = _context.Orders.Count();
            var totalSales = _context.Orders.Sum(o => o.TotalAmount);
            var totalProducts = _context.Products.Count();
            var pendingOrders = _context.Orders.Count(o => o.Status == "Pending");

            var model = new AdminDashboardViewModel
            {
                TotalOrders = totalOrders,
                TotalSales = totalSales,
                TotalProducts = totalProducts,
                PendingOrders = pendingOrders
            };

            return View(model);
        }

        // ==========================================
        // 2. ORDERS MANAGEMENT
        // ==========================================
        public IActionResult Orders()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.ShippingName,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    PaymentMethod = o.PaymentMethod
                })
                .ToList();

            return View(orders);
        }

        // ==========================================
        // 3. ORDER DETAIL
        // ==========================================
        [HttpGet]
        public IActionResult OrderDetail(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = order.Id,
                orderNumber = order.OrderNumber,
                customer = order.ShippingName,
                date = order.OrderDate.ToString("yyyy-MM-dd"),
                address = $"{order.ShippingAddress}, {order.ShippingWard}, {order.ShippingCity}",
                payment = order.PaymentMethod,
                status = order.Status,
                total = order.TotalAmount,
                items = order.OrderDetails.Select(od => new
                {
                    name = od.Product.Name,
                    qty = od.Quantity,
                    price = od.Price,
                    total = od.Price * od.Quantity
                }).ToList()
            });
        }

        // ==========================================
        // 4. UPDATE ORDER STATUS
        // ==========================================
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            order.Status = status;
            _context.SaveChanges();

            return Json(new { success = true, message = "Order status updated successfully" });
        }

        // ==========================================
        // 5. PRODUCTS MANAGEMENT
        // ==========================================
        public IActionResult Products()
        {
            var products = _context.Products
                .Select(p => new AdminProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Price = p.Price,
                    Category = p.Category.Name,
                    Stock = p.Stock,
                    Status = p.IsActive ? "Active" : "Inactive",
                    ImageUrl = p.ImageUrl
                })
                .ToList();

            return View(products);
        }

        // ==========================================
        // 6. CUSTOMERS MANAGEMENT
        // ==========================================
        public IActionResult Customers()
        {
            var customers = _context.Users
                .Select(u => new AdminCustomerViewModel
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Email = u.Email,
                    Location = u.Address,
                    TotalOrders = _context.Orders.Count(o => o.UserId == u.Id),
                    TotalSpent = _context.Orders.Where(o => o.UserId == u.Id).Sum(o => o.TotalAmount)
                })
                .ToList();

            return View(customers);
        }
    }
}