using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CreateModel(ContosoPizza.Data.ContosoPizzaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult OnGet()
        {
            CategoryList = new SelectList(_context.Categories, "Id", "CategoryName");
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; } = default!;
        public SelectList CategoryList { get; set; }
        public string ErrorMessage { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(IFormFile imageFile)
        {
                bool IsValidImageFile(IFormFile imageFile)
                {
                    if (imageFile == null || imageFile.Length == 0)
                    {
                        return false;
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    return allowedExtensions.Contains(extension);
                }

                if (!IsValidImageFile(imageFile))
                {
                    ErrorMessage = "Can't add your image because it is not in the correct format .jpg, .jpeg, .png";
                    return OnGet();
                }


                // Lưu trữ tệp tin ảnh trong thư mục uploads
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "sample-files");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn tới file ảnh vào trường ImagePath của model Image
                Product.Image = "/sample-files/" + uniqueFileName;

            if (_context.Products == null || Product == null)
            {
                return Page();
            }
            var existProductName = await _context.Products.Where(c => c.ProductName == Product.ProductName).FirstOrDefaultAsync();
            if (existProductName != null)
            {
                ErrorMessage = "This Product Name: " + Product.ProductName + " has already existed";
                return OnGet();
            }

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
