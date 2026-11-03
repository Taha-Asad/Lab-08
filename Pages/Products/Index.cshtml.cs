using Microsoft.AspNetCore.Mvc.RazorPages;
using PMS_CRUD_Razor.Data;
using PMS_CRUD_Razor.Models;

namespace PMS_CRUD_Razor.Pages.Products;

public class IndexModel : PageModel
{
    private readonly ProductRepository _repository;

    // Constructor: No 'void' return type, name matches the class
    public IndexModel(ProductRepository repository)
    {
        _repository = repository;
    }

    public List<Product> Products { get; set; } = new();

    public async Task OnGetAsync()
    {
        Products = await _repository.GetAllAsync();
    }
}