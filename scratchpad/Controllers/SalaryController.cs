using HtmlAgilityPack;
using scratchpad.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace scratchpad.Controllers
{
    public class SalaryController : ApiController
    {
        string baseUrl = "http://umsalary.info/index.php?";
        string baseUrlByTitle = "http://www.umsalary.info/titlesearch.php?";

        private string ConstructGetSalaryByNameUri(string firstName = "", string lastName = "", int year = 0, int campus = 0)
        {
            return baseUrl + "FName=" + firstName + "&LName=" + lastName + "&Year=" + year.ToString() + "&Campus=" + campus.ToString();
        }
        private string ConstructGetSalaryByTitle(string titleSearch, int year, int campus)
        {
            return baseUrlByTitle + string.Format("Title={0}&year={1}&campus{2}", titleSearch, year, campus);
        }
        [HttpGet]
        public IEnumerable<Salary> GetSalary(string firstName, string lastName, int year, int campus)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();


            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(ConstructGetSalaryByNameUri(firstName, lastName, year, campus)));
                doc.LoadHtml(result);
            };
            var newdoc = new HtmlDocument();
            var test1 = doc.GetElementbyId("results").OuterHtml;
            newdoc.LoadHtml(test1);
            var test2 = newdoc.DocumentNode.Descendants("td").Where(x => !x.InnerText.Contains("google")).Select(x=>x.InnerText);
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
        public SalaryByTitle GetSalaryByTitle(string titleSearch, int year, int campus)
        {
            HtmlDocument doc = new HtmlDocument();

            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(ConstructGetSalaryByTitle(titleSearch, year, campus)));
                doc.LoadHtml(result);
                var htmlNode = doc.GetElementbyId("maincontent").OuterHtml;
                var newdoc = new HtmlDocument();
                newdoc.LoadHtml(htmlNode);
                var statTable = newdoc.DocumentNode.Descendants("table").Skip(1).First();
                var statChildren = statTable.ChildNodes;

                return new SalaryByTitle
                {
                    NumberPeopleWithTitle = statChildren.Descendants("td").Skip(1).First().InnerText,
                    AvgSalary = statChildren.Descendants("td").Skip(5).First().InnerText,
                    MinSalary = statChildren.Descendants("td").Skip(7).First().InnerText,
                    MaxSalary = statChildren.Descendants("td").Skip(3).First().InnerText
                };
            };
        }
    }
}


//<option value='0'>2014-2015</option>
//                                      <option value='1'>2013-2014</option>
//                                      <option value='2'>2012-2013</option>
//                                      <option value='3'>2011-2012</option>
//                                      <option value='4'>2010-2011</option>
//                                      <option value='5'>2009-2010</option>
//                                      <option value='6'>2008-2009</option>
//                                      <option value='7'>2007-2008</option>
//                                      <option value='8'>2006-2007</option>
//                                      <option value='9'>2005-2006</option>
//                                      <option value='10'>2004-2005</option>
//                                      <option value='11'>2003-2004</option>
//                                      <option value='12'>2002-2003</option>
//                                  </select>
//                              </td>
//                          </tr>
//                          <tr>
//                              <td>Campus</td>
//                              <td>
//                                  <select name="Campus">
//                                      <option value="0">All</option>
//                                      <option value="1">Ann Arbor</option>
//                                      <option value="2">Flint</option>
//                                      <option value="3">Dearborn</option>