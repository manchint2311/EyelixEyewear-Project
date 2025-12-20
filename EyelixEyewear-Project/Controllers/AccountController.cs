using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EyelixEyewear_Project.Data;
using Microsoft.AspNetCore.Authorization;
using AppUser = EyelixEyewear_Project.Models.User;
using EyelixEyewear_Project.Models.ViewModels;
using EyelixEyewear_Project.Helpers;

namespace EyelixEyewear_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= ĐĂNG KÝ =================
        [HttpGet]
        public IActionResult Signup()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username hoặc email đã tồn tại chưa
                if (await _context.Users.AnyAsync(u => u.Username == model.Username || u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc Email đã tồn tại.");
                    return View(model);
                }

                // 2. Tạo user mới (Dùng AppUser thay vì User)
                var newUser = new AppUser
                {
                    FullName = model.FullName,
                    Username = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Role = "Customer", // Mặc định là khách hàng

                    // Gọi PasswordHelper bạn đã có
                    PasswordHash = PasswordHelper.HashPassword(model.Password),

                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // ================= ĐĂNG NHẬP (LOGIN) =================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm user trong DB
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                // 2. Kiểm tra password (Dùng VerifyPassword từ Helper của bạn)
                if (user != null && PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError("", "Tài khoản đã bị khóa.");
                        return View(model);
                    }

                    // 3. Tạo Claims (Thông tin phiên đăng nhập)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("FullName", user.FullName)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu.");
            }
            return View(model);
        }

        // ================= ĐĂNG XUẤT (LOGOUT) =================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // ================= PROFILE DASHBOARD =================
        [Authorize] // Bắt buộc đăng nhập
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // 1. Lấy ID user đang đăng nhập
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);

            // 2. Lấy thông tin User + Đơn hàng từ DB
            var user = await _context.Users
                .Include(u => u.Orders)
                    .ThenInclude(o => o.OrderDetails) // Load chi tiết đơn để tính toán nếu cần
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login");

            // 3. Đổ dữ liệu vào ViewModel
            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.PhoneNumber ?? "",
                Address = user.Address ?? "",
                Orders = user.Orders.OrderByDescending(o => o.OrderDate).ToList() // Đơn mới nhất lên đầu
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return RedirectToAction("Login");

            // Load lại danh sách đơn hàng để hiển thị nếu có lỗi (vì Orders không được submit qua form)
            model.Orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (ModelState.IsValid)
            {
                // 1. Cập nhật thông tin cơ bản
                user.FullName = model.FullName;
                user.PhoneNumber = model.Phone;
                user.Address = model.Address;
                // Email và Username thường không cho đổi tùy tiện để tránh lỗi hệ thống

                // 2. Xử lý đổi mật khẩu (Nếu có nhập)
                if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    // Kiểm tra mật khẩu cũ
                    if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                    {
                        ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
                        TempData["ErrorMessage"] = "Cập nhật thất bại: Sai mật khẩu cũ.";
                        return View("Profile", model);
                    }

                    // Đổi sang mật khẩu mới
                    user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
            return View("Profile", model);
        }

        // ================= FORGOT PASSWORD =================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Please enter your email address.");
                return View();
            }

            // Tìm user theo email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError("", "No account found with this email address.");
                return View();
            }

            // Generate password tạm thời (6 ký tự random)
            var tempPassword = GenerateRandomPassword(8);

            // Hash và lưu vào DB
            user.PasswordHash = PasswordHelper.HashPassword(tempPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Hiển thị password tạm cho user (trong thực tế nên gửi email)
            ViewBag.TempPassword = tempPassword;
            ViewBag.UserEmail = email;
            ViewBag.Success = true;

            return View("ForgotPasswordSuccess");
        }

        // Helper method để generate random password
        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // Order Detail
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetOrderDetail(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy order với đầy đủ thông tin
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Order not found" });
            }

            // Chuẩn bị dữ liệu trả về
            var orderDetail = new
            {
                success = true,
                order = new
                {
                    orderNumber = order.OrderNumber,
                    orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    status = order.Status,
                    paymentMethod = order.PaymentMethod,
                    note = order.Note ?? "",

                    // Thông tin giao hàng
                    shippingName = order.ShippingName,
                    shippingPhone = order.ShippingPhone,
                    shippingAddress = order.ShippingAddress,
                    shippingWard = order.ShippingWard,
                    shippingDistrict = order.ShippingDistrict,
                    shippingCity = order.ShippingCity,

                    // Chi tiết sản phẩm
                    items = order.OrderDetails.Select(od => new
                    {
                        productName = od.Product.Name,
                        productImage = od.Product.ImageUrl ?? "/images/default-product.jpg",
                        quantity = od.Quantity,
                        price = od.Price,
                        subtotal = od.Quantity * od.Price
                    }).ToList(),

                    // Tổng tiền
                    subtotal = order.OrderDetails.Sum(od => od.Quantity * od.Price),
                    shippingFee = order.ShippingFee,
                    discount = order.Discount,
                    totalAmount = order.TotalAmount
                }
            };

            return Json(orderDetail);
        }
    }
}