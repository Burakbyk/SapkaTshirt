using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OzSapkaTShirt.Data;
using OzSapkaTShirt.Models;

namespace OzSapkaTShirt.Controllers
{
    [Authorize]
    public class OrderProductsController : Controller
    {
        private readonly ApplicationContext _context;

        public OrderProductsController(ApplicationContext context)
        {
            _context = context;
        }
      

        public IActionResult UpDateBasket(long id, short quantity)
        {
            Order? order;
            OrderProduct? orderProduct;
            Product? product = _context.Products.Find(id);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            order = _context.Orders.Where(o => o.UserId == userId && o.Status == 0).Include(o => o.OrderProducts).FirstOrDefault();
            if (order != null && order.OrderProducts != null)
            {
                foreach (var item in order.OrderProducts)
                {
                    if (item.Product == null)
                    {
                        item.Product = _context.Products.FirstOrDefault(q => q.Id == item.ProductId);
                    }
                }
            }

            if (order == null)
            {
                order = new Order();
                order.OrderDate = DateTime.Today;
                order.Status = 0;
                order.TotalPrice = 0;
                order.UserId = userId;
                order.OrderProducts = new List<OrderProduct>();
                _context.Add(order);
                _context.SaveChanges();
            }
            orderProduct = order.OrderProducts.Find(o => o.ProductId == id);
            if (orderProduct == null)
            {
                orderProduct = new OrderProduct();
                orderProduct.OrderId = order.Id;
                orderProduct.Price = product.Price;
                orderProduct.ProductId = id;
                orderProduct.Quantity = 1;
                orderProduct.Total = product.Price;
                order.OrderProducts.Add(orderProduct);
            }

            else
            {
                if (orderProduct.Quantity == quantity * -1)
                {
                    order.TotalPrice -= orderProduct.Price * orderProduct.Quantity;
                    order.OrderProducts.Remove(orderProduct);
                    _context.OrderProducts.Remove(orderProduct);
                    _context.SaveChanges();
                }
                orderProduct.Quantity += quantity;
                if (orderProduct.Quantity == 0)
                {

                    order.OrderProducts.Remove(orderProduct);
                    if (order.OrderProducts.Count == 0)
                    {
                        _context.Remove(order);
                        _context.SaveChanges();
                        HttpContext.Response.Cookies.Append("totalQuantity", order.OrderProducts.Sum(q => q.Quantity).ToString());
                        return PartialView("OrderDetailPartial", order);
                    }
                }
                else
                {
                    orderProduct.Total += product.Price * quantity;
                }
            }

            order.TotalPrice += product.Price * quantity;

            _context.Update(order);
            _context.SaveChanges();
            HttpContext.Response.Cookies.Append("totalQuantity", order.OrderProducts.Sum(q => q.Quantity).ToString());
            return PartialView("OrderDetailPartial", order);


        }




    }
}
