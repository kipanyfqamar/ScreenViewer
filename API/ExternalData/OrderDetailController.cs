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

namespace ScreenViewer.API.ExternalData
{
    public class OrderDetailController : ApiController
    {
        private ScreenPlayCRMEntities db = new ScreenPlayCRMEntities();

        // GET api/OrderDetail
        public IQueryable<OrderDetail> GetOrderDetails()
        {
            return db.OrderDetails;
        }

        // GET api/OrderDetail/5
        [ResponseType(typeof(OrderDetail))]
        public IHttpActionResult GetOrderDetail(decimal id)
        {
            OrderDetail orderdetail = db.OrderDetails.Find(id);
            if (orderdetail == null)
            {
                return NotFound();
            }

            return Ok(orderdetail);
        }

        // PUT api/OrderDetail/5
        public IHttpActionResult PutOrderDetail(decimal id, OrderDetail orderdetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != orderdetail.OrderId)
            {
                return BadRequest();
            }

            db.Entry(orderdetail).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
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

        // POST api/OrderDetail
        [ResponseType(typeof(OrderDetail))]
        public IHttpActionResult PostOrderDetail(OrderDetail orderdetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.OrderDetails.Add(orderdetail);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (OrderDetailExists(orderdetail.OrderId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = orderdetail.OrderId }, orderdetail);
        }

        // DELETE api/OrderDetail/5
        [ResponseType(typeof(OrderDetail))]
        public IHttpActionResult DeleteOrderDetail(decimal id)
        {
            OrderDetail orderdetail = db.OrderDetails.Find(id);
            if (orderdetail == null)
            {
                return NotFound();
            }

            db.OrderDetails.Remove(orderdetail);
            db.SaveChanges();

            return Ok(orderdetail);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderDetailExists(decimal id)
        {
            return db.OrderDetails.Count(e => e.OrderId == id) > 0;
        }
    }
}