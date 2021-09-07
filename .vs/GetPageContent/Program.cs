using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GetPageContent
{
    class Program
    {

        public class Item
        {

            public Item()
            {

            }

            public Item(string Name, double Price, double DeliveryIncluded)
            {
                this.Name = Name;
                this.Price = Price;
                DeliveryCostIncluded = DeliveryIncluded;
            }

            public string Name { get; set; }
            public double Price { get; set; }
            public double DeliveryCostIncluded { get; set; }
        }
        public class ConfigData
        {
            public string Directory { get; set; }
            public string Url { get; set; }
        }

        public class GetContentFromAllegro : GetContent
        {

            public GetContentFromAllegro()
            {

            }
            public ConfigData GetConfiguration()
            {
                ConfigData data = new ConfigData();
                try
                {
                    IConfiguration Config = new ConfigurationBuilder()
               .AddJsonFile("appSettings.json")
               .Build();

                    data.Url = Config.GetSection("Url").Value;
                    data.Directory = Config.GetSection("Destination").Value;
                    return data;
                }
                catch (Exception ex)
                {
                    return data;
                }

            }
            public List<Item> GetItemListFromPage(string Url)
            {
                List<Item> itemList = new List<Item>();               
                var Webget = new HtmlWeb();
                var doc = Webget.Load(Url);
                HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//div//section");
                HtmlNodeCollection collection2 = collection[1].ChildNodes;
                List<HtmlNode> collection3 = collection2.Where(x => x.Name == "article").ToList();
                foreach (HtmlNode node in collection2)
                {
                    Item item = new Item();
                    try
                    {

                        HtmlNode xxx = node.FirstChild.ChildNodes[1];
                        HtmlNode x1 = xxx.ChildNodes[0];
                        item.Name = x1.FirstChild.InnerText.ToString();
                        HtmlNode x2 = xxx.ChildNodes[1];
                        string Price = x2.FirstChild.InnerText.ToString().Split("zł")[0];
                        item.Price = Convert.ToDouble(Price);
                        HtmlNode x3 = xxx.ChildNodes[2];

                        HtmlNode x4 = xxx.ChildNodes[3];
                        string delivery = x4.InnerText.ToString().Split("zł")[0];
                        item.DeliveryCostIncluded = Convert.ToDouble(delivery);

                    }
                    catch (Exception ex)
                    {

                    }
                    itemList.Add(item);
                }
                return itemList;
            }
            public DataSet MakeTable(string tableName,List<Item> itemList)
            {
                DataTable dt = new DataTable() { TableName = tableName };
                DataSet set = new DataSet();
                if (itemList.Count > 0)
                {
                    dt.Clear();
                    dt.Columns.Add("Nazwa");
                    dt.Columns.Add("Cena");
                    dt.Columns.Add("Cena_z_dostawą");
                    foreach (var item in itemList)
                    {
                        DataRow _ravi = dt.NewRow();
                        _ravi["Nazwa"] = item.Name;
                        _ravi["Cena"] = item.Price;
                        _ravi["Cena_z_dostawą"] = item.DeliveryCostIncluded;
                        dt.Rows.Add(_ravi);
                    }

                    set.Tables.Add(dt);

                }
                return set;
            }


            string GetDesktopPath()
            {
                string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);               
                return filePath;
            }

            public bool SaveFileAsJson(string directory, List<Item> List)
            {
                try
                {
                    if(directory=="")
                    {
                        directory = GetDesktopPath();
                    }

                    directory = directory + "\\Items";
                    System.IO.Directory.CreateDirectory(directory);
                    string datetime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
                    var json = JsonConvert.SerializeObject(List);               
                    File.WriteAllText(directory + "\\ItemsJson" + datetime, json);         
                                                         
                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            public bool SaveFileAsXml(string directory,DataSet set)
            {
                try
                {
                    if (directory == "")
                    {
                        directory = GetDesktopPath();
                    }
                    string datetime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
                    directory = directory + "\\Items";
                    System.IO.Directory.CreateDirectory(directory);
                    set.WriteXml(directory + "\\Items" + datetime);                  
                    return true;
                }
                catch (Exception ex)
                {
                    return false;

                }
            }
            
        }


        public interface GetContent
        {
            ConfigData GetConfiguration();
            List<Item> GetItemListFromPage(string Url);
            DataSet MakeTable(string tableName,List<Item> itemList);
            bool SaveFileAsJson(string Destination, List<Item> List);
            bool SaveFileAsXml(string directory, DataSet set);            
        }

        static void Main(string[] args)
        {

            GetContent content = new GetContentFromAllegro();
            ConfigData configData = content.GetConfiguration();
            List<Item> List = content.GetItemListFromPage(configData.Url);

            content.SaveFileAsJson(configData.Directory, List);
            DataSet set = content.MakeTable("TabelaAllegro",List);
            content.SaveFileAsXml(configData.Directory, set);
            



           

        }

    }

}

