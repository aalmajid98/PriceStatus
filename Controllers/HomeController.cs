using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.IO;
using PriceStatus_Items.Models;
using Microsoft.Ajax.Utilities;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace PriceStatus_Items.Controllers
{
    public class HomeController : Controller
    {
        //no longer in use
        private static string connStr =
            "server=PriceStatus-project.ccgkrwo73nam.us-east-2.rds.amazonaws.com;user=admin;database=items;port=3306;password=";
        private readonly MySqlConnection conn = new MySqlConnection(connStr);
        
        // API requests to UI

        [HttpGet]
        public ActionResult Index()
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">ID ---- Item Name ---- Cost</h3><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Check if there is nothing in the database first
                string sql = "SELECT COUNT(*) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object count = cmd.ExecuteScalar();
                if (Convert.ToInt32(count) == 0)
                {
                    // If there is nothing, let's read from the file
                    Console.WriteLine("Entering readFile function...");
                    readFile();
                    Console.WriteLine("Done with readFile function...");
                }
                else
                {
                    // Get out all the values from the database
                    sql = "SELECT ID,ITEM_NAME,COST FROM items";
                    cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader  rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString()+"\t ---- "+rdr[1].ToString()+ "\t ---- " + rdr[2].ToString() + "</span><br>");
                    }
                    htmlOrig += "</div>";

                    rdr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");
            
            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }

        [HttpGet]
        public ActionResult itemPrice()
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h2 style=\"text-align: center\">Item Name</h2><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                // Get all the item names, no repeats
                string sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader  rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">"+rdr[0].ToString()+"</span><br>");
                }
                htmlOrig += "</div>";

                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            
            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }

        [HttpPost]
        public ActionResult itemPrice(string itemName)
        {
            // Force all item name's passed through to be upper case
            string name = itemName.ToUpper();
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h2 style=\"text-align: center\">Item Name</h2><br>";
            
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                // Get the names of the items and check if what the user gave a valid item
                string sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                // Used to see if their is an item with that name
                Boolean isFound = false;
                while (rdr.Read())
                {
                    htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString()+"</span><br>");
                    if (rdr[0].Equals(name))
                    {
                        isFound = true;
                        break;
                    }
                }
                htmlOrig += "</div>";

                rdr.Close();
                // If the user did not give a valid item name
                if (!isFound)
                {
                    ViewBag.cost = "Not A Valid Item";
                }
                else
                {
                    // SQL cquery to insert the values from the text file to the DB
                    sql = "SELECT MAX(COST) FROM items WHERE ITEM_NAME = @item";
                    cmd = new MySqlCommand(sql, conn);

                    // Give each parameter a value
                    cmd.Parameters.AddWithValue("@item", name);
                    Object maxCost = cmd.ExecuteScalar();

                    ViewBag.cost = maxCost;
                    
                }
                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            return View();
        }
        
        [HttpGet]
        public ActionResult itemsPrice()
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">ID ---- Item Name ---- Cost</h3><br>";
            try
            {
                conn.Open();
                // Get the max cost for each item
                string sql = "SELECT ITEM_NAME, MAX(COST) FROM items GROUP BY ITEM_NAME";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString() + " ---- " + rdr[1].ToString() + "</span><br>");
                }
                htmlOrig += "</div>";
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            
            // Render through the html generated based on what the route is
            ViewBag.html = htmlOrig;
            
            return View();
        }

        [HttpGet]
        public ActionResult updateAddItem()
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">ID ---- Item Name ---- Cost</h3><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Check if there is nothing in the database first
                string sql = "SELECT ID,ITEM_NAME,COST FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString() + "\t ---- " + rdr[1].ToString() + "\t ---- " + rdr[2].ToString() + "</span><br>");
                    }
                    htmlOrig += "</div>";

                    rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");

            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            AddUpdateItemModel model = new AddUpdateItemModel();
            return View(model);
        }

        // Post request to update items
        [HttpPost]
        public ActionResult UpdateItem(AddUpdateItemModel model)
        {
            // Get the updated name and cost of the item based on the id from the model
            string itemName = model.UpdateName;
            int itemID = model.UpdateID;
            int itemCost = model.UpdateCost;
            
            // Force all item name's passed through to be upper case
            itemName = itemName.ToUpper();
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                
                // Update the name and/or cost of item based on id
                string sql = "UPDATE items SET ITEM_NAME = @item, COST = @cost WHERE ID = @id";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
               
                // Give parameters in query there value
                cmd.Parameters.AddWithValue("@item", itemName);
                cmd.Parameters.AddWithValue("@cost", itemCost);
                cmd.Parameters.AddWithValue("@id", itemID);

                // Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");
            
            // Direct POST request to updateAddItem page
            return RedirectToAction("updateAddItem");
        }

        [HttpPost]
        public ActionResult AddItem(AddUpdateItemModel model)
        {
            // Get the name and cost of the item from the model
            string itemName = model.AddName;
            int itemCost = model.AddCost;
            
            // Force all item name's passed through to be upper case
            itemName = itemName.ToUpper();
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                
                // Insert item based on the user's item name and cost of item
                // ID will be auto generated
                string sql = "INSERT INTO items (ITEM_NAME, COST) VALUES (@item, @cost)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
               
                // Give parameters in query there value
                cmd.Parameters.AddWithValue("@item", itemName);
                cmd.Parameters.AddWithValue("@cost", itemCost);

                // Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");

            // Direct POST request to updateAddItem page
            return RedirectToAction("updateAddItem");
        }

        [HttpGet]
        public ActionResult DeleteItem()
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">ID ---- Item Name ---- Cost</h3><br>";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Get out all the data fields from the database
                string sql = "SELECT ID,ITEM_NAME,COST FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    htmlOrig += ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" + rdr[0].ToString() + "\t ---- " + rdr[1].ToString() + "\t ---- " + rdr[2].ToString() + "</span><br>");
                }
                htmlOrig += "</div>";

                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");

            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            // Render an empty delte statement
            ViewBag.delete = "";
            return View();
        }
        
        [HttpPost]
        public ActionResult DeleteItem(int itemID)
        {
            // Used to display the items in the front end
            string htmlOrig = "<div style=\"text-align: center; line-heght: 8px;\"><h3 style=\"text-align: center\">ID ---- Item Name ---- Cost</h3><br>";
            // Used for error messages
            string message = "";
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Count how many items are there
                string sql = "SELECT COUNT(*) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object count = cmd.ExecuteScalar();
                MySqlDataReader rdr;
                // If the ID given from the user is not valid
                if (itemID > Convert.ToInt32(count))
                {
                    message = "Not a Valid ID";
                }
                else // If it is valid
                {
                    // Delete the item based off of the id
                    sql = "DELETE FROM items WHERE ID = @id";
                    cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", itemID);
                    cmd.ExecuteNonQuery();
                    
                    // Get out all the data fields
                    sql = "SELECT ID,ITEM_NAME,COST FROM items";
                    cmd = new MySqlCommand(sql, conn);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        // Format html
                        htmlOrig +=
                            ("<span style=\"display: inline-block; vetical-align: middle; line-height: normal; font-size: 15px;\">" +
                             rdr[0].ToString() + "\t ---- " + rdr[1].ToString() + "\t ---- " + rdr[2].ToString() +
                             "</span><br>");
                    }

                    htmlOrig += "</div>";

                    rdr.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");

            // Render through the html generated based on what the route is
            ViewBag.htmlOrig = htmlOrig;
            // Render through an error message if there is one
            ViewBag.delete = message;
            return View();
        }

        // ---------------------- URI endpoints -------------------
        public Object GetItemsPricesURIEndpoint()
        {
            // ArrayList that will hold all the maxPrices objects
            ArrayList mPL = new ArrayList();
            
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                // Get the max cost for each item
                string sql = "SELECT ID,ITEM_NAME, MAX(COST) FROM items GROUP BY ITEM_NAME";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                
                while (rdr.Read())
                {
                    // Create an object of the ID, ITEM_NAME, and COST returned from the 
                    // query and store it in an array
                    maxPrices t = new maxPrices();
                    t.itemID = (int) rdr[0];
                    t.itemName = rdr[1].ToString();
                    t.itemCost = (int) rdr[2];

                    mPL.Add(t);
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            conn.Close();
            Console.WriteLine("Done.");
            // Return an object instance of the ArrayList of maxPrices
            return JsonConvert.SerializeObject(mPL);
        }

        // Will return a random item with its max price
        public Object GetItemMaxURIEndpoint()
        {
            maxPrices mP = new maxPrices();
            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();
                // Get only the item names, no repetition
                string sql = "SELECT DISTINCT(ITEM_NAME) FROM items";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                string name;
                // Store all the names into the arraylist
                ArrayList names = new ArrayList();

                while (rdr.Read())
                {
                    names.Add(rdr[0]);
                }

                rdr.Close();
                // Randomly get a name out
                Random rand = new Random();
                name = names[rand.Next(names.Capacity)].ToString();
                
                // Get the max cost based off of the name
                // SQL cquery to insert the values from the text file to the DB
                sql = "SELECT ID, ITEM_NAME, MAX(COST) FROM items WHERE ITEM_NAME = @item";
                cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@item", name);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    mP.itemID = (int) rdr[0];
                    mP.itemName = rdr[1].ToString();
                    mP.itemCost = (int) rdr[2];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            // Return an object instance of the maxPrice
            return JsonConvert.SerializeObject(mP);
        }

        // ------------- Functions used for the API requests ----------------

        // Read from the items file
        public void readFile()
        {
            string filePath = (string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "\\App_Data\\items.txt"));
            
            if (System.IO.File.Exists(filePath))
            {
                int count = 1;
                string[] lines = System.IO.File.ReadAllLines(filePath); // Store each line in the text file in the array
                foreach (string line in lines)
                {
                    if (count == 1) // Ignore the header
                    {
                        count++;
                        continue;
                    }
                    else
                    {
                        // Split each line at the commas
                        string[] parts = line.Split(',');
                        
                        // SQL cquery to insert the values from the text file to the DB
                        string sql = "INSERT INTO items (ID,ITEM_NAME,COST) VALUES (@ID, @ITEM_NAME, @COST)";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        
                        // Give each parameter a value
                        cmd.Parameters.AddWithValue("@ID", Int32.Parse(parts[0]));
                        cmd.Parameters.AddWithValue("@ITEM_NAME", parts[1]);
                        cmd.Parameters.AddWithValue("@COST", Int32.Parse(parts[2]));
                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}