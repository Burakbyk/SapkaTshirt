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
    public class OrdersController : Controller
    {
        private readonly ApplicationContext _context;

        public OrdersController(ApplicationContext context)
        {
            _context = context;
        }

        // Siparişler (sepet durumu hariç)
        public async Task<IActionResult> Index()
        {
            List<Order>? order;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order =  _context.Orders.Where(o => o.UserId == userId && o.Status > 0).ToList();
            
        
            if (order==null)
            {
                return View();
            }
          
            return View(order);
        }

       
        // Basket Sepet durumunda ki sipariler
        public async Task<IActionResult> Details()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Order? order = _context.Orders.Where(o => o.UserId == userId && o.Status == 0).Include(o => o.OrderProducts).ThenInclude(o => o.Product).FirstOrDefault();

            if (order == null)
            {
                return View();
            }
            return View(order);
        }

      
        //Sepeti siparişe dönüştürne
        public async Task<IActionResult> Approve(long? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            var orderProducts = _context.Orders.Where(o => o.Id == id).Include(o => o.OrderProducts).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }
            order.Status = 1;
            order.OrderDate = DateTime.Now;
            _context.Update(order);
            _context.SaveChanges();
            if (order.OrderProducts!=null)
            {
                HttpContext.Response.Cookies.Append("totalQuantity", orderProducts.OrderProducts.Sum(q => q.Quantity*-1).ToString());
            }
            return RedirectToAction("Index","Orders");
        }





        //Siparişi sil (Orders ındex içinden)
        public IActionResult RemoveOrder(long? id)
        {
            List<Order>? orderList;
            Order? order;
          
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id != null || _context.Orders != null)
            {
                order = _context.Orders.Where(o => o.Id == id && o.UserId == userId).Include(o => o.OrderProducts).FirstOrDefault();
                if (order != null)
                {

                    _context.Remove(order);
                    _context.SaveChanges();
                    orderList = _context.Orders.Where(o=>o.Status > 0).ToList();                   
                    HttpContext.Response.Cookies.Append("totalQuantity", order.OrderProducts.Sum(q => q.Quantity * -1).ToString());
                    return PartialView("OrderListPartial",orderList);
                }

                return PartialView("OrderListPartial", null);


            }



            return PartialView("OrderListPartial", null);

        }



        //Detaylı sipariş bilgilerin olduğu Sipariş listesi
        public async Task<IActionResult> OrderProductsList(long? id)
        {
            Order? order;
            
            
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            order = _context.Orders.Where(o => o.Id == id && o.UserId==userId).Include(o => o.OrderProducts).ThenInclude(op=>op.Product).First();

            if (order != null && order.OrderProducts != null)
            {
                return View(order);
            }
           

                return View();
            


          
        }

       
   
        //sipariş sil sipariş detay kısmından   
        public IActionResult DeleteOrder(long? id)
        {
           Order? order;

           string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
            {
                return NotFound();
            }


        order = _context.Orders.Where(o => o.Id == id && o.UserId == userId).Include(o => o.OrderProducts).FirstOrDefault();
        if(order != null )
         {
                if (order.Status < 3)
                {
                    _context.Remove(order);

                    _context.SaveChanges();

                    return RedirectToAction("Index", "Orders");
                }

                return NotFound();

         }
       
        

             return NotFound();
            
            
           
        }

       
      
    }
}
