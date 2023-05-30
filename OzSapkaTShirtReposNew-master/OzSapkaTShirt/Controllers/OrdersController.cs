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

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            List<Order>? order;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            order =  _context.Orders.Where(o => o.UserId == userId && o.Status > 0).Include(o=>o.OrderProducts).ThenInclude(op=>op.Product).ToList();
            
        
            if (order==null)
            {
                return NotFound();
            }
          
            return View(order);
        }

        // GET: Orders/Details/5
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

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,TotalPrice")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        public async Task<IActionResult> Approve(long? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            order.Status = 1;
            order.OrderDate = DateTime.Now;
            _context.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Index","Orders");
        }

        public async Task<IActionResult> RemoveOrder(long? id)
        {
            Order? order;
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            order =  _context.Orders.Where(o=>o.Id==id && o.UserId==userId).Include(o=>o.OrderProducts).FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }
       
            _context.Remove(order);
            _context.SaveChanges();
            return View(order);
        }





        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            Order? order;
            OrderProduct? orderProduct;
            Product? product = _context.Products.Find(id);
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            order = _context.Orders.Where(o => o.UserId == userId && o.Status > 0).Include(o => o.OrderProducts).FirstOrDefault();
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
            else
            {
                return NotFound();
            }



            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,OrderDate,TotalPrice")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                  
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ApplicationContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(long id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
