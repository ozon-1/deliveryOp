﻿using System;
using System.Collections.Generic;
using System.Globalization;
using static BackgroundLieferandoApiAsyncRequests.ConsumeAPIs;

namespace BackgroundLieferandoApiAsyncRequests
{
    public static class OrdersExtension
    {
        // Extension method for converting Lieferando's json string format for their orders
        // to a format for our server.
        public static List<OwnOrders> ToOwnOrder(this LieferandoOrders lieferandoOrders)
        {
            List<OwnOrders> listOfOwnOrders = new List<OwnOrders>();
            foreach (var order in lieferandoOrders.orders)
            {
                var listOfproducts = new OwnProductOrders();
                int productIdx = 1;
                foreach (var product in order.products)
                {
                    // ids cannot be null
                    int.TryParse(product.id, out int LieferandoProductId);
                    int.TryParse(order.id, out int LieferandoOrderId);
                    var newProduct = new OwnProduct
                    {
                        status = "INPROGRESS", // what are other status in our API? TODO: status updates to Lieferando API
                                               // to send it to Lieferando its status are either
                                               // printed, kitchen, in_delivery, confirmed_change_delivery_time, delivered or error
                                               // see documentation (page 8): https://tcapp.takeaway.com/manuals/external_order_api_manual.pdf
                        categoryDesc = product.category,
                        categoryName = product.category,
                        secondPrintAt = Properties.Settings.Default.SecondPrintAt, // TODO: depending on category (example?)
                        productType = "PRODUCT", // always "PRODUCT"
                        productName = product.name,
                        productId = LieferandoProductId,
                        groupName = "Küche", // always "Küche"
                        productArticalNumber = product.id, // same as product id
                        printAt = Properties.Settings.Default.PrintAt, // TODO: from config depending on category 
                        tableId = 0, // always 0
                        signatureCount = 0, // always 0
                        orderId = LieferandoOrderId,
                        categoryId = Properties.Settings.Default.CategoryId, // TODO: from config depending on category
                        groupId = 1, // always 1
                        index = productIdx++, // suffix +1, incremental from 1 to n, each order
                        isLine = false, // always false
                        editIndex = 0, // always 0
                        productPrice = product.price,
                        discountRate = 0.0, // always 0.0 until we have some
                        quantity = product.count,
                        discount = 0.0, // always 0.0 until we have some
                        makedKitchen = 0, // always 0
                        numCus = 1, // always 1
                        border = 0, // always 0
                        money = product.price,
                        tablePos = 0, // always 0
                        tax = Properties.Settings.Default.Tax, // TODO: from config depending on product id
                                                               // if product id in database == product.id
                        taxRate = Properties.Settings.Default.TaxRate, // TODO: from config file depend of product id
                        totalDiscount = 0.0, // always 0.0 until we have some
                        totalMoney = product.price,
                        totalTax = product.count * Properties.Settings.Default.Tax, // =quantity*tax (tax, TODO: from config depending on product id)
                                                                                    // if product id in database == product.id 
                        transferToKitchen = 0, // always 0
                        allowChangeTax = Properties.Settings.Default.AllowChangeTax, // TODO: from config file depend of product id
                        // missing information provided by Lieferando API that is not used in our API:
                        // - (not mandatory) sideDishes (and its id), remark
                    };
                    listOfproducts.listOwnProductOrders.Add(newProduct);
                }

                var newOrder = new OwnOrders
                {
                    action = "automaticOrder", // not provided
                    signature = order.orderKey,
                    time = ConvertToOwnDateTime(order.requestedDeliveryTime.ToString()), // format example "26/10/2021 21:15:40",
                    user = "1",
                    zdata = new Zdata
                    {
                        customer = new ownCustomer
                        {
                            customerName = order.customer.name,
                            customerCompany = order.customer.companyName,
                            customerPhone = order.customer.phoneNumber,
                            customerCode = order.customer.postalCode,
                            customerCity = order.customer.city,
                            customerAddress = order.customer.street,
                            customerMail = "tungnm.ptit@gmail.com", // not provided by Lieferando API, where should I get it from the customer?
                            customerType = "ECONOMIC", // not provided
                            id = 1
                            // missing information provided by Lieferando API that is not used in our API:
                            // - (not mandatory) extraAddressInfo f.e. floor 2 or sometimes it's another Building Complex on the same adress
                        },
                        listProductOrders = listOfproducts,
                        waiter = "1" // not provided, probably always some kind of arbitrary number like 0?
                    },
                    // missing information provided by Lieferando API that is not used in our API:
                    // - (not mandatory) remark f.e. "Nicht klingeln bitte"
                    // (Spätschicht Lieferung Baby schlafen, Bote meldet sich telefonisch bei Ankunft)
                };
                listOfOwnOrders.Add(newOrder);
            }


            return listOfOwnOrders;
        }

        // Function to convert various string representations of dates and times to
        // DateTime values with format "dd/MM/YYYY HH:mm:ss".
        private static string ConvertToOwnDateTime(string value)
        {
            try
            {
                return Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString();
            }
            catch (FormatException)
            {
                Console.WriteLine("'{0}' is not in the proper format.", value);
                return value + " is not in the proper format.";
            }
        }
    }
}
