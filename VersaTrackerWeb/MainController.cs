using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace VersaTrackerWeb
{
    public class MainController : Controller
    {
        public string Index()
        {
            return "POLUHUI";
        }

        public string Search(int id, string item = "")
        {
            return HtmlEncoder.Default.Encode($"Searching lots for realm id {id} with name \"{item}\"");
        }
    }
}
