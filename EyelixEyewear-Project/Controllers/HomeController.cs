// Controllers/HomeController.cs
using EyelixEyewear_Project.Data;
using EyelixEyewear_Project.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EyelixEyewear.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                // Lấy 6 sản phẩm mới nhất
                NewArrivals = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(6)
                    .Select(p => new ProductCardViewModel
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        DiscountPrice = p.DiscountPrice,
                        ImageUrl = p.ImageUrl,
                        IsNew = true,
                        IsSale = p.DiscountPrice.HasValue
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: Home/OurStory
        public IActionResult OurStory()
        {
            return View();
        }

        // GET: Home/OurMission
        public IActionResult OurMission()
        {
            return View();
        }

        // GET: Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Xử lý gửi email hoặc lưu vào database
                // Ví dụ: await _emailService.SendContactEmailAsync(model);

                TempData["Message"] = "Thanks for contacting us! We will respond as soon as possible.";
                return RedirectToAction(nameof(Contact));
            }

            return View(model);
        }

        // POST: Home/Subscribe (Newsletter)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(NewsletterSubscribeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Lưu email vào database newsletter
                TempData["Message"] = "Subscribtion Completed! Thank you for your support.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Home/FAQ
        public IActionResult FAQ()
        {
            return View();
        }

        // GET: Home/Terms
        public IActionResult Terms()
        {
            return View();
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Collection/fall-winter-2025
        public IActionResult Collection(string slug)
        {
            // TODO: Lấy thông tin collection từ database
            ViewData["CollectionSlug"] = slug;
            return View();
        }

        // GET: Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}