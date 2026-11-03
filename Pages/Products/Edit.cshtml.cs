using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PMS_CRUD_Razor.Data;
using PMS_CRUD_Razor.Models;
namespace PMS_CRUD_Razor.Pages.Products.Pages;

public class EditModel : PageModel
{
        private readonly ProductRepository _repository;
        private readonly ProductImageService _images;

        public EditModel(
            ProductRepository repository,
            ProductImageService images)
        {
            _repository = repository;
            _images = images;
        }

        [BindProperty]
        public Product Product { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return NotFound();

            Product = product;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (id != Product.Id)
                return BadRequest();

            var existing = await _repository.GetByIdAsync(id);

            if (existing == null)
                return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                string? error = _images.Validate(ImageFile);

                if (error != null)
                    ModelState.AddModelError(nameof(ImageFile), error);
            }

            if (!ModelState.IsValid)
            {
                Product.image = existing.image;
                return Page();
            }

            string oldImage = existing.image;
            string newImage = oldImage;

            if (ImageFile != null && ImageFile.Length > 0)
                newImage = await _images.SaveAsync(ImageFile);

            Product.image = newImage;

            try
            {
                bool updated = await _repository.UpdateAsync(Product);

                if (!updated)
                {
                    if (newImage != oldImage)
                        _images.Delete(newImage);

                    return NotFound();
                }
            }
            catch
            {
                if (newImage != oldImage)
                    _images.Delete(newImage);

                throw;
            }

            if (newImage != oldImage)
                _images.Delete(oldImage);

            TempData["Success"] = "Product updated successfully.";

            return RedirectToPage("Index");
        }
}
