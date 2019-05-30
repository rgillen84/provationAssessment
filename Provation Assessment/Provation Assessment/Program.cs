

//Rextester.Program.Main is the entry point for your code. Don't change it.
//Compiler version 4.0.30319.17929 for Microsoft (R) .NET Framework 4.5

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rextester
{
    public class Program
    {
        // public List<ExemptProduct> ExemptItems;
        public static double ImportRate = 0.05;
        public static double TaxRate = 0.10;
        public static Category book = new Category() { Name = "Book" };
        public static Category food = new Category() { Name = "Food" };
        public static Category medProd = new Category() { Name = "Medical Product" };
        public static Category media = new Category() { Name = "Media" };
        public static Category hygeine = new Category() { Name = "Hygeine" };
       public static List<Category> exemptCategories = new List<Category>() {
                book,
                food,
                medProd
            };

        public static IList<Product> productInventory = new List<Product>() {
                new Product(){ Name = "book", Category = book },
                new Product(){ Name = "music CD", Category = media },
                new Product(){ Name = "chocolate bar", Category = food },
                new Product(){ Name = "box of chocolates", Category = food },
                new Product(){ Name = "bottle of perfume", Category = hygeine },
                new Product(){ Name = "headache pills", Category = medProd },
                new Product(){ Name = "packet of headache pills", Category = medProd }
            };

        public static string input = @"Shopping Basket 1:
1 book at 12.49
1 music CD at 14.99
1 chocolate bar at 0.85
Shopping Basket 2:
1 imported box of chocolates at 10.00
1 imported bottle of perfume at 47.50
Shopping Basket 3:
1 imported bottle of perfume at 27.99
1 bottle of perfume at 18.99
1 packet of headache pills at 9.75
1 imported box of chocolates at 11.25";

        public static void Main(string[] args)
        {
            ShoppingBasket shoppingBasket = new ShoppingBasket();
            List<ShoppingBasket> shoppingBaskets = new List<ShoppingBasket>();
            //build shoppingBaskets
            using (StringReader reader = new StringReader(input))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (line.Contains("Shopping Basket"))
                        {
                            //add basket to the list
                            if (shoppingBasket.Name != null)
                            {
                                shoppingBaskets.Add(shoppingBasket);
                            }
                            //create new basket to build
                            shoppingBasket = new ShoppingBasket();
                            shoppingBasket.Name = line.ToString();
                        } else {
                            //add items to basket
                            CartItem cartItem = parseCartItemString(line);
                            shoppingBasket.CartItems.Add(cartItem);
                        }
                    }
                } while (line != null);
                shoppingBaskets.Add(shoppingBasket);
            }

            //build output
            List<Output> outputList = new List<Output>();
            int count = 0;
            foreach(ShoppingBasket shBasket in shoppingBaskets)
            {
                count += 1;
                Output newOutput = ShoppingBasketToOutput(shBasket, count);
                outputList.Add(newOutput);
            }

            //Write output
            foreach (Output outputEntry in outputList)
            {
                Console.WriteLine(outputEntry.Header);
                foreach(string item in outputEntry.Items)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine(outputEntry.SalesTax);
                Console.WriteLine(outputEntry.Total);
            }
           // Console.WriteLine("Hello, world!");
        }

        public static double SalesTax(CartItem cartItem, double taxRate)
        {
            double salesTax = 0.00;
            if (ItemIsExempt(cartItem.Product) == false) {
                salesTax = (cartItem.Price * taxRate);
            }
            return RoundTax(salesTax);
        }

        public static double ImportDuty(CartItem cartItem, double importRate)
        {
            double importDuty = 0.00;
            if (cartItem.Imported == true)
            {
                importDuty = (cartItem.Price * importRate);
            }
            return RoundTax(importDuty);
        }

        public static double RoundTax(double taxAmount)
        {
            double roundedTax = 0.00;
            if (taxAmount > 0.00)
            {
                double taxCents = (taxAmount * 100);
                if (taxCents % 5 != 0)
                {
                    taxCents = (taxCents - (taxCents % 5) + 5);
                }
                roundedTax = (taxCents / 100);
            }
            return roundedTax;
        }

        public static bool ItemIsExempt(Product product)
        {
            bool isExempt = false;
            Category productCategory = product.Category;
            if (exemptCategories.Contains(productCategory)) {
                isExempt = true;
            }
            return isExempt;
        }

        public static CartItem parseCartItemString(string cartItem)
        {
            CartItem newCartItem = new CartItem();
            //get quantity
            string[] cartItemSections = cartItem.Split(new[] { ' ' }, 2);
            newCartItem.Quantity = Convert.ToInt32(cartItemSections[0]);
            //get item name and price
            cartItemSections = cartItemSections[1].Split(" at ", StringSplitOptions.None);
            newCartItem.Price = Convert.ToDouble(cartItemSections[1]);
            //identify if import and parse name
            string productName = cartItemSections[0];
            if (productName.StartsWith("imported "))
            {
                newCartItem.Imported = true;
                productName = productName.Split(new[] { ' ' }, 2)[1];
            } else
            {
                newCartItem.Imported = false;
            }
            newCartItem.Product = parseCartItemProductString(productName);
            //calculate taxes
            double salesTax = SalesTax(newCartItem, TaxRate);
            double importTax = ImportDuty(newCartItem, ImportRate);
            newCartItem.SalesTax = salesTax + importTax;

            return newCartItem;
        }

        public static Product parseCartItemProductString(string productName)
        {
            //set product name
            Product newProduct = new Product();
            newProduct.Name = productName;
            //set default category
            Category defaultCategory = new Category();
            defaultCategory.Name = "unknown";
            newProduct.Category = defaultCategory;
            //loop through product inventory and find category for product
            foreach (Product product in productInventory)
            {
                //find product in inventory and set category
                if (product.Name == productName)
                {
                    newProduct.Category = product.Category;
                }
            }
            return newProduct;
        }

        public static Output ShoppingBasketToOutput(ShoppingBasket shoppingBasket, int count)
        {
            Output output = new Output();

            output.Header = "Output " + count + ":";

            double totalPrice = 0.00;
            double totalTax = 0.00;

            foreach(CartItem cartItem in shoppingBasket.CartItems)
            {
                string itemName = cartItem.Imported ? "imported " + cartItem.Product.Name : cartItem.Product.Name;
                double totalItemPrice = cartItem.Price + cartItem.SalesTax;
                totalPrice += totalItemPrice;
                totalTax += cartItem.SalesTax;
                string item = cartItem.Quantity + " " + itemName + ": " + string.Format("{0:N2}", totalItemPrice);
                output.Items.Add(item);
            }
            output.Total = "Total: " + string.Format("{0:N2}",totalPrice);
            output.SalesTax = "Sales Taxes: " + string.Format("{0:N2}", totalTax);

            return output;
        }

    }

    public class ShoppingBasket
    {
        public string Name { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }

    public class Output
    {
        public string Header { get; set; }
        public List<string> Items { get; set; } = new List<string>();
        public string SalesTax { get; set; }
        public string Total { get; set; }
    }

    public class CartItem
    {
        public int Quantity { get; set; }
        public bool Imported { get; set; }
        public Product Product { get; set; }
        public double Price { get; set; }
        public double SalesTax { get; set; }
    }

    public class Product
    {
        public string Name { get; set; }
        public Category Category { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
    }
}
