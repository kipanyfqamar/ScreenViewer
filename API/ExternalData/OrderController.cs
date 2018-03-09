using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ScreenViewer.Data;
using System.Data.Entity.Validation;

namespace ScreenViewer.API.ExternalData
{
    public class OrderController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET api/Order
        public IQueryable<Order> GetOrders()
        {
            return db.Orders;
        }

        // GET api/Order/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(decimal id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // PUT api/Order/5
        public IHttpActionResult PutOrder(decimal id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.OrderId)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        //[Route("UpdateOrderDetails")]
        public IHttpActionResult PutOrderDetails(ICollection<OrderDetail> OrderDetails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            decimal OrderId = OrderDetails.FirstOrDefault().OrderId;


            var orderdel = db.OrderDetails.Where(x => x.OrderId == OrderId);
            if (orderdel.Count() > 0)
            {
                db.OrderDetails.RemoveRange(orderdel);
            }


            foreach (var item in OrderDetails)
            {
                db.OrderDetails.Add(item);

            }

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Order
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                db.Orders.Add(order);
                db.SaveChanges();


                return Ok(order);
            }
            catch (DbEntityValidationException e)
            {
                return null;
            }
        }

        // DELETE api/Order/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(decimal id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(decimal id)
        {
            return db.Orders.Count(e => e.OrderId == id) > 0;
        }


    }
}