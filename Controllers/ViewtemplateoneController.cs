﻿using Ecommerce_NetCore_API.Models;
using Ecommerce_NetCore_API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce_NetCore_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewtemplateoneController : Controller
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly Context context;
        public ViewtemplateoneController(IWebHostEnvironment _webHostEnvironment, Context _context)
        {
            webHostEnvironment = _webHostEnvironment;
            context = _context;
        }

        [HttpGet("gethistory")]
        public ActionResult<string> GetAllData()
        {
            return Ok("test");
        }

        [HttpPost("prodaddhistory")]
        public ActionResult<List<ProdAddHistoryReportModel>> GetProductRegisteredHistory([FromForm] ViewTemplateModelOne data, string groupvalue)
        {
            bool IsFromDateValid = false;
            bool IsEndDateValid = false;
            DateTime FromDate;
            string CompleteFromDateString = "";
            DateTime EndDate;
            string CompleteEndDateString = "";
            DateTime dt;
            bool SelectOne = false;
            bool SelectTwo = false;
            bool SelectThree = false;
            List<ProdAddHistoryTE> products = new List<ProdAddHistoryTE>();
            List<ProdAddHistoryReportModel> ReportModelList = new List<ProdAddHistoryReportModel>();
            //List<List<ProdAddHistoryTE>> TotalCollection = new List<List<ProdAddHistoryTE>>();
            string Result = "";
            bool NotFound = false;
            bool IsGroup = groupvalue == "GROUP" ? true : false;

            if (data.FromDate != null && data.EndDate != null)
            {

                DateService DateService1 = new DateService();
                CompleteFromDateString = DateService1.GetCompleteFromDate(data.FromDate);
                IsFromDateValid = DateTime.TryParse(CompleteFromDateString, out dt);

                DateService DateService2 = new DateService();
                CompleteEndDateString = DateService2.GetCompleteEndDate(data.EndDate);
                IsEndDateValid = DateTime.TryParse(CompleteEndDateString, out dt);
            }
            if (IsFromDateValid && IsEndDateValid)
            {
                FromDate = Convert.ToDateTime(CompleteFromDateString);
                EndDate = Convert.ToDateTime(CompleteEndDateString);
                if (data.CategoryValue != null && data.ProductValue == null && data.SizeValue == null)
                {
                    SelectOne = true;
                    var ProductsByCategoryId = context.products.Where(x => x.CategoryId.ToString() == data.CategoryValue).ToList();
                    if (ProductsByCategoryId.Count() != 0)
                    {

                        foreach (var ProductsById in ProductsByCategoryId)
                        {
                            var collection = context.prodAddHistoryData.Where(x => x.ProductId == ProductsById.Id && x.Date >= FromDate && x.Date <= EndDate).OrderBy(x => x.Size).ToList();
                            foreach (var prod in collection)
                            {
                                products.Add(prod);
                            }
                        }
                        // return Ok(products);
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }
                }
                else if (data.ProductValue != null && data.SizeValue == null)
                {
                    SelectTwo = true;
                    var DataCollection = context
                           .prodAddHistoryData
                           .Where(x => x.ProductId.ToString() == data.ProductValue && x.Date >= FromDate && x.Date <= EndDate).OrderBy(x => x.Size).ToList();
                    if (DataCollection.Count() != 0)
                    {

                        products = DataCollection;
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }

                }
                else if (data.ProductValue != null && data.SizeValue != null)
                {
                    SelectThree = true;
                    var DataBySize = context.prodAddHistoryData
                        .Where(x => x.ProductId.ToString() == data.ProductValue && x.Size == data.SizeValue && x.Date >= FromDate && x.Date <= EndDate).ToList();
                    if (DataBySize.Count() > 0)
                    {
                        products = DataBySize;

                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;

                    }

                }
                if (products.Count() > 0 && SelectOne && IsGroup)
                {
                    var GroupBySizeSelectOne = products.GroupBy(x => new { x.ProductId, x.Size }).Select(z => new ProdAddHistoryTE()
                    {
                        ProductId = z.Key.ProductId,
                        Size = z.Key.Size,
                        Quantity = z.Sum(x => x.Quantity),
                        Cost = z.Sum(x => x.Cost) / z.Count(),
                    });
                    products = GroupBySizeSelectOne.ToList();

                }
                else if (products.Count() > 0 && SelectTwo && IsGroup)
                {
                    var GroupBySelectTwo = products.GroupBy(x => x.Size).Select(z => new ProdAddHistoryTE()
                    {
                        ProductId = products[0].ProductId,
                        Size = z.Key,
                        Quantity = z.Sum(x => x.Quantity),
                        Cost = z.Sum(x => x.Cost) / z.Count(),
                    });
                    products = GroupBySelectTwo.ToList();
                }
                else if (products.Count() > 0 && SelectThree && IsGroup)
                {
                    ProdAddHistoryTE SelectThreeData = new ProdAddHistoryTE()
                    {
                        ProductId = products[0].ProductId,
                        Size = products[0].Size,
                        Quantity = products.Sum(x => x.Quantity),
                        Cost = products.Sum(x => x.Cost) / products.Count()

                    };
                    products.Clear();
                    products.Add(SelectThreeData);
                }
                //Produt Name not avilable in products thats why converting into new Model with productName!!
                if (products.Count() > 0)
                {
                    foreach (var prod in products)
                    {
                        ProductNameService NameService = new ProductNameService(context);
                        ProdAddHistoryReportModel model = new ProdAddHistoryReportModel()
                        {
                            Productname = NameService.GetProductName(prod.ProductId),
                            Size = prod.Size,
                            Quantity = prod.Quantity,
                            Cost = prod.Cost,
                            Date = prod.Date,
                            Productid = prod.ProductId

                        };
                        ReportModelList.Add(model);
                    }

                }
                else
                {
                    NotFound = true;
                }
            }
            if (NotFound)
            {
                return Ok(Result);
            }
            return Ok(ReportModelList);

        }


        [HttpPost("prodsalehistory")]
        public ActionResult<List<ProdSaleHistoryReportModel>> GetProductSaleHistory([FromForm] ViewTemplateModelOne data, string groupvalue)
        {
            bool IsFromDateValid = false;
            bool IsEndDateValid = false;
            DateTime FromDate;
            string CompleteFromDateString = "";
            DateTime EndDate;
            string CompleteEndDateString = "";
            DateTime dt;
            bool SelectOne = false;
            bool SelectTwo = false;
            bool SelectThree = false;
            List<SalewithCustIdTE> products = new List<SalewithCustIdTE>();
            List<ProdSaleHistoryReportModel> ReportModelList = new List<ProdSaleHistoryReportModel>();

            string Result = "";
            bool NotFound = false;
            bool IsGroup = groupvalue == "GROUP" ? true : false;

            if (data.FromDate != null && data.EndDate != null)
            {

                DateService DateService1 = new DateService();
                CompleteFromDateString = DateService1.GetCompleteFromDate(data.FromDate);
                IsFromDateValid = DateTime.TryParse(CompleteFromDateString, out dt);

                DateService DateService2 = new DateService();
                CompleteEndDateString = DateService2.GetCompleteEndDate(data.EndDate);
                IsEndDateValid = DateTime.TryParse(CompleteEndDateString, out dt);
            }
            if (IsFromDateValid && IsEndDateValid)
            {
                FromDate = Convert.ToDateTime(CompleteFromDateString);
                EndDate = Convert.ToDateTime(CompleteEndDateString);
                if (data.CategoryValue != null && data.ProductValue == null && data.SizeValue == null)
                {
                    SelectOne = true;
                    var ProductsByCategoryId = context.products.Where(x => x.CategoryId.ToString() == data.CategoryValue).ToList();
                    if (ProductsByCategoryId.Count() != 0)
                    {

                        foreach (var ProductsById in ProductsByCategoryId)
                        {
                            var collection = context.saleswithCustomerIds.Where(x => x.Productid == ProductsById.Id && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).OrderBy(x => x.Prodsize).ToList();
                            foreach (var prod in collection)
                            {
                                products.Add(prod);
                            }
                        }
                        // return Ok(products);
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }

                }
                else if (data.ProductValue != null && data.SizeValue == null)
                {
                    SelectTwo = true;
                    var DataCollection = context
                           .saleswithCustomerIds
                           .Where(x => x.Productid.ToString() == data.ProductValue && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).OrderBy(x => x.Prodsize).ToList();
                    if (DataCollection.Count() != 0)
                    {

                        products = DataCollection;
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }

                }
                else if (data.ProductValue != null && data.SizeValue != null)
                {
                    SelectThree = true;
                    var DataBySize = context.saleswithCustomerIds
                        .Where(x => x.Productid.ToString() == data.ProductValue && x.Prodsize == data.SizeValue && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).ToList();
                    if (DataBySize.Count() > 0)
                    {
                        products = DataBySize;

                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;

                    }

                }

                if (products.Count() > 0 && SelectOne && IsGroup)
                {
                    var GroupBySizeSelectOne = products.GroupBy(x => new { x.Productid, x.Prodsize }).Select(z => new SalewithCustIdTE()
                    {
                        Productid = z.Key.Productid,
                        Prodsize = z.Key.Prodsize,
                        Quantity = z.Sum(x => x.Quantity),
                        Unitprice = z.Sum(x => x.Unitprice) / z.Count(),
                    });
                    products = GroupBySizeSelectOne.ToList();

                }

                else if (products.Count() > 0 && SelectTwo && IsGroup)
                {
                    var GroupBySelectTwo = products.GroupBy(x => x.Prodsize).Select(z => new SalewithCustIdTE()
                    {
                        Productid = products[0].Productid,
                        Prodsize = z.Key,
                        Quantity = z.Sum(x => x.Quantity),
                        Unitprice = z.Sum(x => x.Unitprice) / z.Count(),
                    });
                    products = GroupBySelectTwo.ToList();
                }

                else if (products.Count() > 0 && SelectThree && IsGroup)
                {
                    SalewithCustIdTE SelectThreeData = new SalewithCustIdTE()
                    {
                        Productid = products[0].Productid,
                        Prodsize = products[0].Prodsize,
                        Quantity = products.Sum(x => x.Quantity),
                        Unitprice = products.Sum(x => x.Unitprice) / products.Count()

                    };
                    products.Clear();
                    products.Add(SelectThreeData);
                }

                //Produt Name not avilable in products thats why converting into new Model with productName!!
                if (products.Count() > 0)
                {
                    foreach (var prod in products)
                    {
                        ProductNameService NameService = new ProductNameService(context);
                        ProdSaleHistoryReportModel model = new ProdSaleHistoryReportModel()
                        {
                            Productname = NameService.GetProductName(prod.Productid),
                            Size = prod.Prodsize,
                            Quantity = prod.Quantity,
                            Cost = prod.Unitprice,
                            Date = prod.Purchasedate,
                            Productid = prod.Productid

                        };
                        ReportModelList.Add(model);
                    }

                }

                else
                {
                    NotFound = true;
                }

            }
            if (NotFound)
            {
                return Ok(Result);
            }

            return Ok(ReportModelList);

        }

        [HttpPost("prodsaleprofit")]
        public ActionResult<List<ProdProfitReportModel>> GetProductSaleProfit([FromForm] ViewTemplateModelOne data)
        {
            bool IsFromDateValid = false;
            bool IsEndDateValid = false;
            DateTime FromDate;
            string CompleteFromDateString = "";
            DateTime EndDate;
            string CompleteEndDateString = "";
            DateTime dt;
            bool SelectOne = false;
            bool SelectTwo = false;
            bool SelectThree = false;
            List<SalewithCustIdTE> products = new List<SalewithCustIdTE>();
            List<ProdProfitReportModel> ReportModel = new List<ProdProfitReportModel>();
            string Result = "";
            bool NotFound = false;
            if (data.FromDate != null && data.EndDate != null)
            {

                DateService DateService1 = new DateService();
                CompleteFromDateString = DateService1.GetCompleteFromDate(data.FromDate);
                IsFromDateValid = DateTime.TryParse(CompleteFromDateString, out dt);

                DateService DateService2 = new DateService();
                CompleteEndDateString = DateService2.GetCompleteEndDate(data.EndDate);
                IsEndDateValid = DateTime.TryParse(CompleteEndDateString, out dt);

            }
            if (IsFromDateValid && IsEndDateValid)
            {
                FromDate = Convert.ToDateTime(CompleteFromDateString);
                EndDate = Convert.ToDateTime(CompleteEndDateString);

                if (data.CategoryValue != null && data.ProductValue == null && data.SizeValue == null)
                {
                    SelectOne = true;
                    var ProductsByCategoryId = context.products.Where(x => x.CategoryId.ToString() == data.CategoryValue).ToList();
                    if (ProductsByCategoryId.Count() != 0)
                    {

                        foreach (var ProductsById in ProductsByCategoryId)
                        {
                            var collection = context.saleswithCustomerIds.Where(x => x.Productid == ProductsById.Id && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).OrderBy(x => x.Prodsize).ToList();
                            foreach (var prod in collection)
                            {
                                products.Add(prod);
                            }
                        }
                        // return Ok(products);
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }

                }
                else if (data.ProductValue != null && data.SizeValue == null)
                {
                    SelectTwo = true;
                    var DataCollection = context
                           .saleswithCustomerIds
                           .Where(x => x.Productid.ToString() == data.ProductValue && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).OrderBy(x => x.Prodsize).ToList();
                    if (DataCollection.Count() != 0)
                    {

                        products = DataCollection;
                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;
                    }

                }
                else if (data.ProductValue != null && data.SizeValue != null)
                {
                    SelectThree = true;
                    var DataBySize = context.saleswithCustomerIds
                        .Where(x => x.Productid.ToString() == data.ProductValue && x.Prodsize == data.SizeValue && x.Purchasedate >= FromDate && x.Purchasedate <= EndDate).ToList();
                    if (DataBySize.Count() > 0)
                    {
                        products = DataBySize;

                    }
                    else
                    {
                        Result = "NotFound";
                        NotFound = true;

                    }

                }
                List<SalewithCustIdTE> ProdSaleGropued = new List<SalewithCustIdTE>();
                if (products.Count() > 0 && SelectOne)
                {
                    var GroupBySizeSelectOne = products.GroupBy(x => new { x.Productid, x.Prodsize }).Select(z => new SalewithCustIdTE()
                    {
                        Productid = z.Key.Productid,
                        Prodsize = z.Key.Prodsize,
                        Quantity = z.Sum(x => x.Quantity),
                        Unitprice = z.Sum(x => x.Unitprice) / z.Count(),
                    });
                    ProdSaleGropued = GroupBySizeSelectOne.ToList();

                }

                else if (products.Count() > 0 && SelectTwo)
                {
                    var GroupBySelectTwo = products.GroupBy(x => x.Prodsize).Select(z => new SalewithCustIdTE()
                    {
                        Productid = products[0].Productid,
                        Prodsize = z.Key,
                        Quantity = z.Sum(x => x.Quantity),
                        Unitprice = z.Sum(x => x.Unitprice) / z.Count(),
                    });
                    ProdSaleGropued = GroupBySelectTwo.ToList();
                }

                else if (products.Count() > 0 && SelectThree)
                {
                    SalewithCustIdTE SelectThreeData = new SalewithCustIdTE()
                    {
                        Productid = products[0].Productid,
                        Prodsize = products[0].Prodsize,
                        Quantity = products.Sum(x => x.Quantity),
                        Unitprice = products.Sum(x => x.Unitprice) / products.Count()

                    };
                    ProdSaleGropued.Clear();
                    ProdSaleGropued.Add(SelectThreeData);
                }

                // Creating Profit Model for Result
                if (ProdSaleGropued.Count() > 0)
                {
                    List<ProdAddHistoryTE> ProductsAll = new List<ProdAddHistoryTE>();
                    foreach (SalewithCustIdTE prod in ProdSaleGropued)
                    {
                        List<ProdAddHistoryTE> ProdRegistered = context
                                 .prodAddHistoryData
                                 .Where(x => x.ProductId == prod.Productid && x.Size == prod.Prodsize).ToList();
                        foreach (var prodSingle in ProdRegistered)
                        {
                            ProductsAll.Add(prodSingle);
                        }
                    }

                    if (ProductsAll.Count() > 0)
                    {
                        List<ProdAddHistoryTE> ProdRegGrouped = ProductsAll.GroupBy(x => new { x.ProductId, x.Size }).Select(z => new ProdAddHistoryTE()
                        {
                            ProductId = z.Key.ProductId,
                            Size = z.Key.Size,
                            Cost = z.Sum(x => x.Cost) / z.Count(),
                            Quantity = z.Sum(x => x.Quantity),

                        }).ToList<ProdAddHistoryTE>();
                        foreach (var ProductSold in ProdSaleGropued)
                        {
                            var ProductRegistered = ProdRegGrouped.Single(x => x.ProductId == ProductSold.Productid && x.Size == ProductSold.Prodsize);
                            ProductNameService NameService = new ProductNameService(context);
                            ProductProfitService profitService = new ProductProfitService(context);
                            ProdProfitReportModel model = new ProdProfitReportModel();
                            model.Productname = NameService.GetProductName(ProductSold.Productid);
                            model.Size = ProductSold.Prodsize;
                            model.Purchasecostaverage = ProductRegistered.Cost;
                            model.Salecostaverage = ProductSold.Unitprice;
                            model.Quantitysold = ProductSold.Quantity;
                            model.Profit = profitService.CalcuatingProductProfitBasedDateValue(ProductSold.Productid, ProductSold.Prodsize, FromDate, EndDate);

                            ReportModel.Add(model);
                        }

                    }

                }
            }

            //
            if (NotFound)
            {
                return Ok(Result);
            }
            return Ok(ReportModel);
        }

        [HttpPost("prodstockreport")]
        public ActionResult<List<ProdStockReportModel>> GetProductStockReport([FromForm] ViewTemplateModelOne data)
        {
         
            string Result = "";
           
           List<StockTE> stocks = new List<StockTE>();
           List<ProdStockReportModel> modelList = new List<ProdStockReportModel>();
           if (data.CategoryValue != null && data.ProductValue == null && data.SizeValue == null)
                {
                   
                    var ProductsByCategoryId = context.products.Where(x => x.CategoryId.ToString() == data.CategoryValue).ToList();
                    if (ProductsByCategoryId.Count() != 0)
                    {

                        foreach (var ProductsById in ProductsByCategoryId)
                        {
                        var collection = context.stocks.Where(x => x.ProductId == ProductsById.Id).ToList();
                            foreach (var prod in collection)
                            {
                            stocks.Add(prod);
                            }
                        }
                        
                    }
                    else
                    {
                        Result = "NotFound";
                        
                    }
                }
                else if (data.ProductValue != null && data.SizeValue == null)
                {
                  
                    var DataCollection = context
                           .stocks
                           .Where(x => x.ProductId.ToString() == data.ProductValue).ToList();
                    if (DataCollection.Count() != 0)
                    {

                        stocks = DataCollection;
                    }
                    else
                    {
                        Result = "NotFound";
                       
                    }

                }
                else if (data.ProductValue != null && data.SizeValue != null)
                {
                    
                    var DataBySize = context.stocks
                        .Where(x => x.ProductId.ToString() == data.ProductValue && x.Size == data.SizeValue).ToList();
                    if (DataBySize.Count() > 0)
                    {
                     stocks = DataBySize;

                    }
                    else
                    {
                        Result = "NotFound";
                       

                    }

                }

           if(stocks.Count() > 0)
            {
                ProductNameService nameService = new ProductNameService(context);
                foreach(StockTE stock in stocks)
                {
                    ProdStockReportModel model = new ProdStockReportModel();
                    model.Productname = nameService.GetProductName(stock.ProductId);
                    model.Size = stock.Size;
                    model.Quantity = stock.Quantity;
                    model.Saleprice = stock.Cost;
                    modelList.Add(model);
                }
                return Ok(modelList);
            }
           else
            {
                return Ok(Result);
            }
            

        }
    
        [HttpPost("prodcostcomparisonreport")]
        public ActionResult<List<ProdCostComparisonReportModel>> GetProdcutCostDifference([FromForm] ViewTemplateModelOne  data)
        {
            string Result = "";

            List<StockTE> stocks = new List<StockTE>();
            List<ProdCostComparisonReportModel> modelList = new List<ProdCostComparisonReportModel>();
            if (data.CategoryValue != null && data.ProductValue == null && data.SizeValue == null)
            {

                var ProductsByCategoryId = context.products.Where(x => x.CategoryId.ToString() == data.CategoryValue).ToList();
                if (ProductsByCategoryId.Count() != 0)
                {

                    foreach (var ProductsById in ProductsByCategoryId)
                    {
                        var collection = context.stocks.Where(x => x.ProductId == ProductsById.Id).ToList();
                        foreach (var prod in collection)
                        {
                            stocks.Add(prod);
                        }
                    }

                }
                else
                {
                    Result = "NotFound";

                }
            }
            else if (data.ProductValue != null && data.SizeValue == null)
            {

                var DataCollection = context
                       .stocks
                       .Where(x => x.ProductId.ToString() == data.ProductValue).ToList();
                if (DataCollection.Count() != 0)
                {

                    stocks = DataCollection;
                }
                else
                {
                    Result = "NotFound";

                }

            }
            else if (data.ProductValue != null && data.SizeValue != null)
            {

                var DataBySize = context.stocks
                    .Where(x => x.ProductId.ToString() == data.ProductValue && x.Size == data.SizeValue).ToList();
                if (DataBySize.Count() > 0)
                {
                    stocks = DataBySize;

                }
                else
                {
                    Result = "NotFound";


                }

            }

            if (stocks.Count() > 0)
            {
                ProductNameService nameService = new ProductNameService(context);
                ProductProfitService profitService = new ProductProfitService(context);
                foreach (StockTE stock in stocks)
                {
                    ProdCostComparisonReportModel model = new ProdCostComparisonReportModel();
                    model.Productname = nameService.GetProductName(stock.ProductId);
                    model.Size = stock.Size;
                    model.Latestpurchaseprice = profitService.GetProductLatestPurcashingPrice(stock.ProductId, stock.Size);
                    model.Saleprice = stock.Cost;
                    modelList.Add(model);
                }
                return Ok(modelList);
            }
            else
            {
                return Ok(Result);
            }


        }





    }
}
