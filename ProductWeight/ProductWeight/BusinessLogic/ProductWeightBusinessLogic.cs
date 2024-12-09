using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductWeight.BusinessLogic
{
    // add comment
    // add comment 2
    // add comment 3
    public class ProductWeightBusinessLogic
    {
        private PluginContext<Entity> _context;
        private Entity _target;
        private Entity _preImage;

        public ProductWeightBusinessLogic(PluginContext<Entity> context)
        {
            _context = context;
            _target = context.Target;
            _preImage = _context.GetPreImage("PreImage");
        }

        public void CreateProductWeight()
        {
            try
            {
                if (_target.LogicalName == "salesorderdetail")
                {
                    EntityReference productrefenace = _target.GetAttributeValue<EntityReference>("productid");
                    Guid productid = productrefenace.Id;

                    if (productid != Guid.Empty)
                    {
                        Entity productentity = _context.Service.Retrieve(productrefenace.LogicalName, productid, new ColumnSet("dt_newweight"));

                        if(productentity.Contains("dt_newweight"))
                        {
                            decimal productWeight = productentity.GetValue<decimal>("dt_newweight",0);
                            decimal quantity = _target.GetValue<decimal>("quantity", 0);

                            _target.SetValue("dt_newquantityweight", productWeight);
                            _target.SetValue("dt_newtotalweight", productWeight * quantity);

                            _context.Trace("Quantity Weight: " + productWeight);
                            _context.Trace("Total Weight: " + productWeight * quantity);
                            _context.Service.Update(_target);
                            CreateNetWeightOnOrder();
                        }                       
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        public void UpdateProductWeight()
        {
            try
            {
                if (_target.LogicalName == "salesorderdetail")
                {

                    EntityReference productReference = _context.GetValue<EntityReference>(_target, _preImage, "productid");


                    Entity ProductEntity = _context.Service.Retrieve(productReference.LogicalName, productReference.Id, new ColumnSet("dt_newweight"));

                    decimal Product_weight = ProductEntity.GetAttributeValue<decimal>("dt_newweight");//100

                    decimal qty = _context.GetValue<decimal>(_target, _preImage, "quantity");//updated qty

                    decimal totalweight_orderline = _preImage.GetAttributeValue<decimal>("dt_newtotalweight");//qty*weight

                    decimal totalweight = qty * Product_weight;

                    _target.SetValue("dt_newquantityweight", Product_weight);

                    _target.SetValue("dt_newtotalweight", totalweight);

                    _context.Service.Update(_target);
                    CreateNetWeightOnOrder();



                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        public void CreateNetWeightOnOrder()
        {
            EntityCollection salesorderdetailentities = GetSalesOrderlineWithDiscount();
            decimal netWeight = 0;
            foreach (Entity salesorderdetail in salesorderdetailentities.Entities)
            {
                netWeight += salesorderdetail.GetValue<decimal>("dt_newtotalweight",0);
                _context.Trace("Net weight: {0}",netWeight);
            }
            Guid orderid = (_target.Contains("salesorderid")) ? _target.GetAttributeValue<EntityReference>("salesorderid").Id : _preImage.GetAttributeValue<EntityReference>("salesorderid").Id;
            Entity orderentity = new Entity("salesorder", orderid);

            orderentity.SetValue("dt_finalweight", netWeight);
            _context.Service.Update(orderentity);
        }

        public void UpdateNetWeightOnDelete()
        {
            try
            {
                decimal current_weight = _target.GetAttributeValue<decimal>("dt_newtotalweight");
                Guid salesOrderId = _target.GetAttributeValue<EntityReference>("salesorderid").Id;
                Entity order = _context.Service.Retrieve("salesorder", salesOrderId, new ColumnSet("dt_finalweight"));
                decimal tweight = order.GetAttributeValue<decimal>("dt_finalweight");
                _context.Trace("Total Weight: {0}", tweight);
                decimal final_weight = tweight - current_weight;
                _context.Trace("Net Weight: {0}", final_weight);
                order["dt_finalweight"] = final_weight;

                _context.Service.Update(order);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private EntityCollection GetSalesOrderlineWithDiscount()
        {
            Guid orderId = (_target.Contains("salesorderid")) ? _target.GetAttributeValue<EntityReference>("salesorderid").Id : _preImage.GetAttributeValue<EntityReference>("salesorderid").Id;
            QueryExpression query = new QueryExpression("salesorderdetail");
            query.ColumnSet = new ColumnSet(true); // retrieve all columns of salesorderdetail.
            query.Criteria.AddCondition("salesorderid", ConditionOperator.Equal, orderId);
            EntityCollection salesOrderDetails = _context.Service.RetrieveMultiple(query);
            _context.Trace("Sales Order Details: {0}", salesOrderDetails.Entities.Count);
            return salesOrderDetails;
        }

        private Guid GetOrderId(Guid id)
        {
            var query = new QueryExpression("salesorder");
            query.ColumnSet.AddColumn("salesorderid");
            var a = query.AddLink("salesorderdetail", "salesorderid", "salesorderid");
            a.EntityAlias = "a";
            a.LinkCriteria.AddCondition("salesorderdetailid", ConditionOperator.Equal, id);
            EntityCollection result = _context.Service.RetrieveMultiple(query);
            if (result.Entities.Count > 0)
            {
                return result.Entities[0].Id;
            }
            return Guid.Empty;

        }

    }
}
