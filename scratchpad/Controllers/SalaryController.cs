using HtmlAgilityPack;
using scratchpad.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using WebApi.Models;
using WebApi.Models.Context;

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
        public IEnumerable<Salary> GetSalary(string firstName, string lastName, int year, int campus, StreamWriter sw)
        {
            HtmlDocument doc = new HtmlDocument();

            try
            {
                using (var wc = new WebClient())
                {
                    var result = wc.DownloadString(new Uri(ConstructGetSalaryByNameUri(firstName, lastName, year, campus)));
                    doc.LoadHtml(result);
                };
            }
            catch
            {

                sw.WriteLine(string.Concat("Couldnt find ", firstName, " ", lastName));
            }
            var newdoc = new HtmlDocument();
            if (doc.GetElementbyId("results") == null)
            {
                sw.WriteLine(string.Format("Couldnt find results for {0} {1}", firstName, lastName));
                return null;
            }
            var test1 = doc.GetElementbyId("results").OuterHtml;
            newdoc.LoadHtml(test1);
            var test2 = newdoc.DocumentNode.Descendants("td").Where(x => !x.InnerText.Contains("google")).Select(x => x.InnerText);
            var salaryList = new List<Salary>();

            var availableYears = GetAvailableYear();
            var availableCampus = GetCampusCodes();
            for (int i = 0; i < test2.Count() / 5; i++)
            {
                var fiveItems = test2.Skip(i * 5).Take(5);
                var salary = new Salary()
                {
                    Name = fiveItems.ElementAt(0),
                    Title = fiveItems.ElementAt(1),
                    Department = fiveItems.ElementAt(2),
                    FTR = decimal.Parse(fiveItems.ElementAt(3), NumberStyles.Currency).ToString("#.##"),
                    GF = decimal.Parse(fiveItems.ElementAt(4), NumberStyles.Currency).ToString("#.##"),
                    //CampusCode = availableCampus.CampusMap[campus.ToString()],
                    Year = availableYears.AvailableYearMap[year.ToString()]
                };


                salaryList.Add(salary);
            }
            return salaryList;
        }
        [HttpGet]
        public string UpdateNames()
        {
            using (var db = new SalaryInfoContext())
            {
                foreach (var item in db.SalarySet)
                {
                    if(item.FirstName != null)
                    {
                        item.FirstName = item.Name.Split(',')[1];
                        db.SaveChanges();
                    }
                    if (item.LastName != null)
                    {
                        item.LastName = item.Name.Split(',')[0];
                        db.SaveChanges();
                    }
                }

            }
            return "Ok";
        }
        [HttpGet]
        public IEnumerable<Salary> GetSalaryFromDb(string name, string year, string campus)
        {
            using (var db = new SalaryInfoContext())
            {
                var names = name.Split(' ');

                var t = db.SalarySet.Where(x => (x.Name.ToLower().Contains(names[0].ToLower())
                && x.Name.ToLower().Contains(names[1].ToLower()))
                && x.Year == year)
                .ToList();
                return t;
            }
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
                var statTable = newdoc.DocumentNode.Descendants("table").Skip(1).FirstOrDefault();
                if (statTable == null) return null;
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

        [HttpGet]
        public List<Numbers> GetHighestSalary(int year, int campus)
        {
            HtmlDocument doc = new HtmlDocument();

            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(string.Format(@"http://www.umsalary.info/numbers.php?Year={0}&Campus={1}&RPT_PERIOD=0", year, campus)));
                doc.LoadHtml(result);
                var htmlNode = doc.GetElementbyId("maincontent").OuterHtml;
                var newdoc = new HtmlDocument();
                newdoc.LoadHtml(htmlNode);
                var statTable = newdoc.DocumentNode.Descendants("table").Skip(1).FirstOrDefault();
                if (statTable == null) return null;
                var statChildren = statTable.OuterHtml;
                newdoc.LoadHtml(statChildren);
                var t1 = newdoc.DocumentNode.Descendants("table").FirstOrDefault();
                if (t1 == null) return null;
                var data = t1.OuterHtml;
                newdoc.LoadHtml(data);

                var list = new List<Numbers>();
                for (var i = 0; i < newdoc.DocumentNode.Descendants("td").Count() / 4; i++)
                {
                    var fourItems = newdoc.DocumentNode.Descendants("td").Skip(i * 4).Take(4);
                    var record = new Numbers
                    {
                        Name = fourItems.ElementAt(0).InnerText,
                        Title = fourItems.ElementAt(1).InnerText,
                        Department = fourItems.ElementAt(2).InnerText,
                        Salary = fourItems.ElementAt(3).InnerText
                    };
                    list.Add(record);
                }

                return list;
            };
        }



        [HttpGet]
        public AvailableYears GetAvailableYear()
        {
            HtmlDocument doc = new HtmlDocument();
            List<string> keys;
            List<string> values;

            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(baseUrl));
                doc.LoadHtml(result);
                var htmlNode = doc.GetElementbyId("maincontent").OuterHtml;
                var newdoc = new HtmlDocument();
                newdoc.LoadHtml(htmlNode);
                var statTable = newdoc.DocumentNode.Descendants("select").First().Descendants("option");
                keys = statTable.Select(x => x.Attributes.Single(a => a.Name == "value").Value).ToList();
                values = statTable.Select(x => x.NextSibling.InnerText.Replace("\t", string.Empty).Replace("\n", string.Empty)).ToList();
            };

            return new AvailableYears(keys, values);
        }

        [HttpGet]
        public AvailableCampus GetCampusCodes()
        {
            HtmlDocument doc = new HtmlDocument();
            List<string> keys;
            List<string> values;

            using (var wc = new WebClient())
            {
                var result = wc.DownloadString(new Uri(baseUrl));
                doc.LoadHtml(result);
                var htmlNode = doc.GetElementbyId("maincontent").OuterHtml;
                var newdoc = new HtmlDocument();
                newdoc.LoadHtml(htmlNode);
                var statTable = newdoc.DocumentNode.Descendants("select").Last().Descendants("option");
                keys = statTable.Select(x => x.Attributes.Single(a => a.Name == "value").Value).ToList();
                values = statTable.Select(x => x.NextSibling.InnerText.Replace("\t", string.Empty).Replace("\n", string.Empty)).ToList();
            };

            return new AvailableCampus(keys, values);
        }
        [HttpPost]
        public void SaveAllSalaries()
        {
            var dictionary = new Dictionary<string, string>()
            {
                //{ "2","2013-2014" },
                //{ "3","2012-2013" },
                //{ "4","2011-2012" },
                //{ "5","2010-2011" },
                //{ "6","2009-2010" },
                //{ "7","2008-2009" },
                //{ "8","2007-2008" },
                { "9","2006-2007" },
                { "10","2005-2006" },
                //{ "11","2004-2005" },
                //{ "12","2003-2004" },
                //{ "13","2002-2003" },
            };
            foreach (var item in dictionary.Keys)
            {
                HtmlDocument doc = new HtmlDocument();
                try
                {
                    using (var wc = new WebClient())
                    {
                        var result = wc.DownloadString(new Uri("http://www.umsalary.info/deptsearch.php?Dept=%25&Year=" + item + "&Campus=0"));
                        doc.LoadHtml(result);
                    };
                }
                catch
                {

                }
                var data = doc.GetElementbyId("maincontent").Descendants("table").Last().Descendants("td").Where(x => !x.InnerText.Contains("google")).Select(x => x.InnerText);
                var salaryList = new List<Salary>();

                var availableYears = GetAvailableYear();
                var availableCampus = GetCampusCodes();

                for (int i = 0; i < data.Count() / 5; i++)
                {
                    var fiveItems = data.Skip(i * 5).Take(5);
                    var salary = new Salary()
                    {
                        Name = fiveItems.ElementAt(0),
                        Title = fiveItems.ElementAt(1),
                        Department = fiveItems.ElementAt(2),
                        FTR = decimal.Parse(fiveItems.ElementAt(3), NumberStyles.Currency).ToString("#.##"),
                        GF = decimal.Parse(fiveItems.ElementAt(4), NumberStyles.Currency).ToString("#.##"),
                        //CampusCode = availableCampus.CampusMap[campus.ToString()],
                        Year = dictionary[item]
                    };

                    salaryList.Add(salary);
                }
                using (var db = new AllSalariesContext())
                {
                    db.SalarySet.AddRange(salaryList);
                    db.SaveChanges();
                }
            }
        }


        [HttpGet]
        public void GetSharedServicesSalary()
        {
            HtmlDocument doc = new HtmlDocument();

            try
            {
                using (var wc = new WebClient())
                {
                    var result = wc.DownloadString(new Uri("http://www.umsalary.info/UM_Shared_Services.php"));
                    doc.LoadHtml(result);
                };
            }
            catch
            {

            }
            var data = doc.DocumentNode.Descendants("table").Skip(1).First().Descendants("td").Where(x => !x.InnerText.Contains("google")).Where(x => !x.InnerText.Contains("\t")).Select(x => x.InnerText);
            var salaryList = new List<Salary>();

            for (int i = 0; i < data.Count() / 5; i++)
            {
                var fiveItems = data.Skip(i * 5).Take(5);
                var salary = new Salary()
                {
                    Name = fiveItems.ElementAt(0),
                    Title = fiveItems.ElementAt(1),
                    Department = fiveItems.ElementAt(2),
                    FTR = decimal.Parse(fiveItems.ElementAt(3), NumberStyles.Currency).ToString("#.##"),
                    GF = decimal.Parse(fiveItems.ElementAt(4), NumberStyles.Currency).ToString("#.##"),
                    //Year = availableYears.AvailableYearMap[year.ToString()]
                };


                salaryList.Add(salary);
            }

        }
    }
}


//                                      <option value='0'>2014-2015</option>
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