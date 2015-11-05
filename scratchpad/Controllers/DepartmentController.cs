using HtmlAgilityPack;
using scratchpad.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;
using WebApi.Models;
using WebApi.Models.Context;

namespace WebApi.Controllers
{
    public class DepartmentController : ApiController
    {
        protected string baseUrl = "http://www.umsalary.info/alldeptsearch.php?";

        private string ConstructDepartmentPages(int pageNumber)
        {
            return string.Concat(baseUrl, "page=", pageNumber.ToString());
        }
        [HttpGet]
        public IEnumerable<string> GetAllDepartments()
        {
            var doc = new HtmlDocument();
            var list = new List<string>();

            using (var wc = new WebClient())
            {
                for (var i = 1; i < 46; i++)
                {
                    var result = wc.DownloadString(new Uri(ConstructDepartmentPages(i)));
                    doc.LoadHtml(result);
                    var htmlNode = doc.GetElementbyId("maincontent").OuterHtml;

                    var newdoc = new HtmlDocument();
                    newdoc.LoadHtml(htmlNode);
                    var statTable = newdoc.DocumentNode.Descendants("table").First().InnerHtml;

                    var tableDoc = new HtmlDocument();
                    tableDoc.LoadHtml(statTable.ToString());

                    var alink = tableDoc.DocumentNode.Descendants("a");
                    var departmentList = alink.Select(x => x.InnerText);
                    list.AddRange(departmentList);
                }
            };
            return list;
        }

        [HttpPost]
        public void SaveAllDepartments()
        {
            var departments = GetAllDepartments();
            using (var db = new DepartmentContext())
            {
                foreach (var item in departments)
                {
                    var department = new Department() { DepartmentName = item };
                    db.DepartmentSet.Add(department);
                }
                db.SaveChanges();
            }
        }
    }
}
