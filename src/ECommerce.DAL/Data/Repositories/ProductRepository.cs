using ECommerce.DAL.Common;
using ECommerce.DAL.Data.RepositoryContracts;
using ECommerce.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DAL.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PagedResult<Product>> SearchProductsAsync(ProductSearchParams searchParams)
    {
        IQueryable<Product> query = _dbContext.Products.Include(p => p.Category);

        query = ApplyFilters(searchParams, query);
        query = ApplySorting(searchParams, query);

        int totalCount = await query.CountAsync();
        var pagedItems = await query
            .Skip((searchParams.PageNumber - 1) * searchParams.PageSize)
            .Take(searchParams.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = searchParams.PageNumber,
            PageSize = searchParams.PageSize
        };
    }

    private static IQueryable<Product> ApplyFilters(ProductSearchParams searchParams, IQueryable<Product> query)
    {
        if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            string searchTerm = searchParams.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm));
        }

        if (searchParams.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == searchParams.CategoryId.Value);
        }

        if (searchParams.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= searchParams.MinPrice.Value);
        }

        if (searchParams.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= searchParams.MaxPrice.Value);
        }

        return query;
    }

    private static IQueryable<Product> ApplySorting(ProductSearchParams searchParams, IQueryable<Product> query)
    {
        if (!string.IsNullOrWhiteSpace(searchParams.SortBy))
        {
            switch (searchParams.SortBy.ToLower())
            {
                case "name":
                    query = searchParams.SortDescending
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                    break;
                case "price":
                    query = searchParams.SortDescending
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                    break;
                default:
                    query = query.OrderBy(p => p.Id);
                    break;
            }
        }
        else
        {
            query = query.OrderBy(p => p.Name);
        }

        return query;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await _dbContext.Categories.ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _dbContext.Categories.FindAsync(id);
    }
}