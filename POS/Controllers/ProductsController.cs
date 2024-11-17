using POS.Class;
using POS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace POS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Requires a valid JWT token to access
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest("Product data is null");
            }

            product.LastModified = DateTime.UtcNow; 
            _context.Products.Add(product);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromBody] Product updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.LastModified = DateTime.UtcNow; 

            _context.SaveChanges();
            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
            return NoContent();
        }

        // SYNC: api/products/download
        [HttpGet("download")]
        public IActionResult DownloadChanges([FromQuery] DateTime lastSyncTime)
        {
            var changes = _context.Products
                .Where(p => p.LastModified > lastSyncTime)
                .ToList();

            return Ok(changes); // Return products that have been modified since the last sync
        }

        // SYNC: api/products/upload
        [HttpPost("upload")]
        public IActionResult UploadChanges([FromBody] List<Product> clientProducts)
        {
            foreach (var clientProduct in clientProducts)
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == clientProduct.Id);

                if (existingProduct == null)
                {
                    // Add new product
                    clientProduct.LastModified = DateTime.UtcNow; // Set server timestamp
                    _context.Products.Add(clientProduct);
                }
                else if (clientProduct.LastModified > existingProduct.LastModified)
                {
                    // Update existing product if client has a newer version
                    existingProduct.Name = clientProduct.Name;
                    existingProduct.Price = clientProduct.Price;
                    existingProduct.LastModified = DateTime.UtcNow; // Set server timestamp
                }
            }

            _context.SaveChanges();
            return Ok("Sync completed");
        }
    }
}
