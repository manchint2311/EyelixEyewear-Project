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
        // GET: /Product?category=glasses
        // GET: /Product?category=sunglasses
        public async Task<IActionResult> Index(string category)
        {
            // Lấy tất cả sản phẩm đang active
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .Include(p => p.ProductVariants)
                .Where(p => p.IsActive);

            // Filter theo category nếu có
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Name.ToLower() == category.ToLower());
            }

            var products = await query
                .OrderByDescending(p => p.CreatedAt) // Mới nhất lên đầu
                .ToListAsync();

            // Truyền category vào ViewBag để hiển thị title
            ViewBag.CurrentCategory = category;

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