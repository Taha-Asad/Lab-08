using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PMS_CRUD_Razor.Data;
using PMS_CRUD_Razor.Models;

namespace PMS_CRUD_Razor.Pages.Products;

public class EditModel : PageModel
{
    private readonly ProductRepository _repository;
    private readonly ProductImageService _images;

    public EditModel(ProductRepository repository, ProductImageService images)
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
        {
            return NotFound();
        }

        Product = product;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Image update is optional during edit
        if (ImageFile != null && ImageFile.Length > 0)
        {
            string? error = _images.Validate(ImageFile);
            if (error != null)
            {
                ModelState.AddModelError(nameof(ImageFile), error);
            }
        }

        ModelState.Remove("Product.image");
        ModelState.Remove("Product.CreatedAt");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // If a new image was uploaded, save it and remove the old one
        if (ImageFile != null && ImageFile.Length > 0)
        {
            string oldImage = Product.image;
            string newImagePath = await _images.SaveAsync(ImageFile);
            Product.image = newImagePath;

            if (!string.IsNullOrEmpty(oldImage))
            {
                _images.Delete(oldImage);
            }
        }

        bool updated = await _repository.UpdateAsync(Product);

        if (!updated)
        {
            ModelState.AddModelError(string.Empty, "Failed to update the product. Please try again.");
            return Page();
        }

        TempData["Success"] = "Product updated successfully.";
        return RedirectToPage("Index");
    }
}