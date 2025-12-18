using EyelixEyewear_Project.Data;
using EyelixEyewear_Project.Models;
using EyelixEyewear_Project.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EyelixEyewear_Project.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        // Trang danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả sản phẩm đang active, bao gồm thông tin Category
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt) // Mới nhất lên đầu
                .ToListAsync();

            return View(products);
        }

        // GET: /Product/Detail/5
        // Trang chi tiết sản phẩm
        public async Task<IActionResult> Detail(int id)
        {
            if (id <= 0) return NotFound();

            // Lấy sản phẩm theo ID
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews) // Nếu muốn hiển thị đánh giá
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Lấy 4 sản phẩm liên quan (cùng Category, khác ID hiện tại)
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id && p.IsActive)
                .Take(4)
                .ToListAsync();

            // Đưa dữ liệu vào ViewModel
            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts
            };

            return View(viewModel);
        }
    }
}