using POS.Class;
using POS.Models;
using Microsoft.EntityFrameworkCore;

namespace POS.Services
{
    public class ProductSyncService
    {
        private readonly AppDbContext _context;

        public ProductSyncService(AppDbContext context)
        {
            _context = context;
        }

        // Check if there is an internet connection
        private bool HasInternet()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = client.GetAsync("https://www.google.com").Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        // Sync offline products to the main database and delete them from SQLite
        public async Task SyncOfflineProducts()
        {
            if (!HasInternet()) return;

            // Fetch unsynced offline products
            var offlineProducts = await _context.OfflineProducts
                                                .Where(p => !p.IsSynced)
                                                .ToListAsync();

            foreach (var offlineProduct in offlineProducts)
            {
                // Add to main Product table
                var product = new Product
                {
                    Name = offlineProduct.Name,
                    Price = offlineProduct.Price,
                };

                _context.Products.Add(product);

                // Mark as synced in SQLite
                offlineProduct.IsSynced = true;
            }

            // Save changes to the main database
            await _context.SaveChangesAsync();

            // Remove synced records from SQLite
            _context.OfflineProducts.RemoveRange(offlineProducts);
            await _context.SaveChangesAsync(); // Save changes to SQLite
        }

        // Add a product (offline or online based on connectivity)
        public async Task AddProductAsync(Product product)
        {
            if (HasInternet())
            {
                // Save directly to the main database
                _context.Products.Add(product);
            }
            else
            {
                // Save to the offline database
                var offlineProduct = new OfflineProduct
                {
                    Name = product.Name,
                    Price = product.Price,
                };
                _context.OfflineProducts.Add(offlineProduct);
            }

            await _context.SaveChangesAsync();
        }
    }
}
