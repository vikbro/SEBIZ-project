using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SEBIZ.Data;
using SEBIZ.Domain.Contracts.MogoDbProductAPI.Domain.Contracts;
using SEBIZ.Domain.Models;

namespace SEBIZ.Service
{
    public class ProductService : IProductService
    {
        public ProductService(AppDbContext context) {
            _context = context;
        }
        private readonly AppDbContext _context;
        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            ValidateCreateDto(createProductDto);

            var product = new Product
            {
                //Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price
            };

            try
            {
                await _context.ProductCollection.InsertOneAsync(product);
                return MapToDto(product);
            }
            catch (Exception ex)
            {
                throw new MongoException($"Failed to create product: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            ValidateObjectId(id);

            try
            {

                var result = await _context.ProductCollection.DeleteOneAsync(
                    Builders<Product>.Filter.Eq(p => p.Id, id)
                );
                if (result.DeletedCount == 0)
                {
                    throw new MongoException($"Product with id {id} not found.");
                }

                return true;

            }
            catch (Exception ex) when (!(ex is MongoException))
            {
                throw new MongoException($"Failed to delete product: {ex.Message}");
            }

            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                var products = await _context.ProductCollection.Find(Builders<Product>.Filter.Empty).ToListAsync();
                return products.Select(MapToDto);
            }
            catch (Exception ex)
            {
                throw new MongoException($"Failed to retrieve products: {ex.Message}");
            }

            throw new NotImplementedException();
        }

        public async Task<ProductDto> GetProductByIdAsync(string id)
        {
            ValidateObjectId(id);

            try
            {
                var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
                var products = await _context.ProductCollection.Find(filter).FirstOrDefaultAsync()
                    ?? throw new MongoException($"Product with id {id} not found.");
                return MapToDto(products);
            }
            catch (Exception ex) when (!(ex is MongoException))
            {
                throw new MongoException($"Failed to retrieve product: {ex.Message}");
            }
        }

        public async Task<ProductDto> UpdateProductAsync(string id, UpdateProductDto updateProductDto)
        {
            ValidateObjectId(id);
            ValidateUpdateDto(updateProductDto);

            try
            {
                var update = Builders<Product>.Update.Set(p => p.Name, updateProductDto.Name)
                    .Set(p => p.Description, updateProductDto.Description)
                    .Set(p => p.Price, updateProductDto.Price);

                var product = await _context.ProductCollection.FindOneAndUpdateAsync(
                    Builders<Product>.Filter.Eq(p => p.Id, id),
                    update,
                    new FindOneAndUpdateOptions<Product>
                    {
                        ReturnDocument = ReturnDocument.After
                    }
                    ) ?? throw new MongoException($"Product with id {id} not found.");
                return MapToDto(product);

            }
            catch (Exception ex)
            {
                throw new MongoException($"Failed to update product: {ex.Message}");
            }
        }

        private static void ValidateObjectId(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length != 24 || !System.Text.RegularExpressions.Regex.IsMatch(id, "^[0-9a-fA-F]{24}$"))
            {
                throw new MongoException("Invalid ObjectId format.");
                if(!MongoDB.Bson.ObjectId.TryParse(id, out var objectId))
                {
                    throw new MongoException("Invalid ObjectId format.");
                }
            }
        }

        //validate create dto
        private static void ValidateCreateDto(CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                throw new ArgumentNullException(nameof(createProductDto), "CreateProductDto cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(createProductDto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(createProductDto.Name));
            }
        }
        //ValidateAntiForgeryTokenAttribute updateto Dto
        private static void ValidateUpdateDto(UpdateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                throw new ArgumentNullException(nameof(createProductDto), "CreateProductDto cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(createProductDto.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(createProductDto.Name));
            }
        }

        //map everything to dto
        private static ProductDto MapToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        };


    }
}
