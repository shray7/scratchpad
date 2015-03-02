using HtmlAgilityPack;
using scratchpad.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace scratchpad.Controllers
{
    public class SalaryController : ApiController
    {
        string baseUrl = "http://umsalary.info/index.php?";

        private string ConstructUri(string firstName = "", string lastName = "", int year = 0, int campus = 0)
        {
            return baseUrl + "FName=" + firstName + "&LName=" + lastName + "&Year=" + year.ToString() + "&Campus=" + campus.ToString();
        }

        [HttpGet]
        public IEnumerable<Salary> GetSalary(string firstName, string lastName, int year, int campus)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(ConstructUri(firstName, lastName, year, campus)));
                doc.LoadHtml(result);
            }
            var test1 = doc.GetElementbyId("results").OuterHtml;
            var newdoc = new HtmlDocument();
            newdoc.LoadHtml(test1);
            var test2 = newdoc.DocumentNode.Descendants("td").Select(x => x.InnerText);
            var salaryList = new List<Salary>();
            for (int i = 0; i < test2.Count() / 5; i++)
            {
                var fiveItems = test2.Skip(i * 5).Take(5);
                var salary = new Salary();

                salary.Name = fiveItems.ElementAt(0);
                salary.Title = fiveItems.ElementAt(1);
                salary.Department = fiveItems.ElementAt(2);
                salary.FTR = decimal.Parse(fiveItems.ElementAt(3), NumberStyles.Currency);
                salary.GF = decimal.Parse(fiveItems.ElementAt(4), NumberStyles.Currency);

                salaryList.Add(salary);
            }
            return salaryList;
        }
        [HttpGet]
        public string GetSalaryAsync(string firstName, string lastName, int year, int campus)
        {
            var jss = new JavaScriptSerializer();
            return jss.Serialize(new { results = GetSalary(firstName, lastName, year, campus) });
        }


    }
}
