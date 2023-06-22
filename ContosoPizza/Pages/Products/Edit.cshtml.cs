using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoPizza.Data;
using ContosoPizza.Models.Generated;
using Microsoft.AspNetCore.Hosting;

namespace ContosoPizza.Pages.Products
{
    public class EditModel : PageModel
    {
        private readonly ContosoPizza.Data.ContosoPizzaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EditModel(ContosoPizza.Data.ContosoPizzaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public Product Product { get; set; } = default!;
        [BindProperty]
        public string ErrorMessage { get; set; } = default!;

        public SelectList CategoryList { get; set; }

        public int skippedcount = 0;
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product =  await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            skippedcount = await _context.Products.CountAsync(p => p.Id < id);
            CategoryList = new SelectList(_context.Categories, "Id", "CategoryName");
            Product = product;
            HttpContext.Session.SetInt32("SkippedCount", skippedcount);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile imageFile, int? id)
        {
            if (imageFile != null)
            {
                if (!IsValidImageFile(imageFile))
                {
                    ErrorMessage = "Your image is not in the correct format";
                    return await OnGetAsync(id);
                }

                var image_link = _context.Products.Where(p=>p.Id==id).Select(p=>p.Image).FirstOrDefault();
                Console.WriteLine("ImageLink: " + image_link);
                if (image_link != null) 
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,image_link.TrimStart('/'));
                    
                    if(System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }else
                    {
                        Console.WriteLine("No Path, just add");
                    }    
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
            }
            else
            {
                var existingProduct = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
                if (existingProduct != null)
                {
                    _context.Entry(existingProduct).State = EntityState.Detached;
                    Product.Image = existingProduct.Image;
                }
            }
            _context.Attach(Product).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return await OnGetAsync(Product.Id);
        }

        /*var skippedCount_else = HttpContext.Session.GetInt32("SkippedCount");
        var curentPage_else = skippedCount_else / 5 + 1;
                return Redirect($"./Index?Page={curentPage_else}");*/

        private bool ProductExists(int id)
        {
          return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private bool IsValidImageFile(IFormFile imageFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}
