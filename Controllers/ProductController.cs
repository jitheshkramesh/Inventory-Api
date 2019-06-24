using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DempApiApp.Data;
using DempApiApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DempApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("[action]")]
        [Authorize(Policy = "RequiedLoggedIn")]
        public IActionResult GetProducts()
        {
            return Ok(_db.ProductModels.ToList());
        }

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratoralRole")]
        public async Task<IActionResult> AddProduct([FromBody]ProductModel model)
        {
            var newProduct = new ProductModel {
                Name = model.Name,
                ImageUrl = model.ImageUrl,
                Description = model.Description,
                OutOfStock = model.OutOfStock,
                Price = model.Price
            };

            await _db.ProductModels.AddAsync(newProduct);
            await _db.SaveChangesAsync();
            return Ok();

        }

        [HttpPost("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratoralRole")]
        public async Task<IActionResult> UpdateProduct([FromRoute]int id, [FromBody]ProductModel model)
        {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var findProduct = _db.ProductModels.FirstOrDefault(p => p.ProductId == id);
            if (findProduct == null) {
                return NotFound();
            }

            // if the product was found
            findProduct.Name = model.Name;
            findProduct.ImageUrl = model.ImageUrl;
            findProduct.Description = model.Description;
            findProduct.OutOfStock = model.OutOfStock;
            findProduct.Price = model.Price;


            _db.Entry(findProduct).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return Ok(new JsonResult("The product with id " + id.ToString() + " is updated."));

        }

        [HttpPost("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratoralRole")]
        public async Task<IActionResult> DeleteProduct([FromRoute]int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findProduct = await _db.ProductModels.FindAsync(id);
            if (findProduct == null)
            {
                return NotFound();
            }

            _db.ProductModels.Remove(findProduct);
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The product with id " + id.ToString() + " is deleted."));
        }
    }
}