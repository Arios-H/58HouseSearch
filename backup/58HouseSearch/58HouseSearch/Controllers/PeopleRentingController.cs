﻿using _58HouseSearch.Common;
using _58HouseSearch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _58HouseSearch.Controllers
{
    public class PeopleRentingController : Controller
    {
        //
        // GET: /PeopleRenting/

        public ActionResult Index()
        {
            HTTPHelper.WritePVInfo(Request.UserHostAddress, Request.Path);
            return View();
        }

        public ActionResult GetRentingData()
        {
            //var indexURL = "http://www.huzhumaifang.com/Renting/index";
            //var pageCount = GetPageCount(indexURL);每页数据量太少，只取前一百条数据
            var roomList = Enumerable.Range(1, 10).Select(index => GetRoomList($"http://www.huzhumaifang.com/Renting/index/p/{index}.html")).Aggregate((a, b) => a.Concat(b));
            return Json(new { IsSuccess = true, HouseInfos = roomList });
        }


        public ActionResult GetRentingDatabyPageIndex(int index)
        {
            try
            {
                var roomList = GetRoomList($"http://www.huzhumaifang.com/Renting/index/p/{index}.html");
                return Json(new { IsSuccess = true, HouseInfos = roomList });
            }catch(Exception ex)
            {
                return Json(new { IsSuccess = false, Error =$"http://www.huzhumaifang.com/Renting/index/p/{index}.html"+
                    "获取数据异常，可能是哪里挂了吧。看不懂的异常如下：" +ex.ToString() });
            }
          
        }


        private int GetPageCount(string indexURL)
        {
            var htmlResult = HTTPHelper.GetHTMLByURL(indexURL);
            var page = new AngleSharp.Parser.Html.HtmlParser().Parse(htmlResult);
            return Convert.ToInt32(page.QuerySelector("a.end")?.TextContent ?? "0");
        }


        private IEnumerable<HouseInfo> GetRoomList(string url)
        {
            var htmlResult = HTTPHelper.GetHTMLByURL(url);
            var page = new AngleSharp.Parser.Html.HtmlParser().Parse(htmlResult);
            return page.QuerySelector("ul.screening_left_ul").QuerySelectorAll("li").Select(element =>
            {
                var screening_time = element.QuerySelector("p.screening_time").TextContent;
                var screening_price = element.QuerySelector("h5").TextContent;
                var locationInfo = element.QuerySelector("a");
                var locationContent = locationInfo.TextContent.Split('，').FirstOrDefault();
                var location = locationContent.Remove(0, locationContent.IndexOf("租") + 1);

                decimal housePrice = 0;
                decimal.TryParse(screening_price.Replace("￥", "").Replace("元/月", ""),out housePrice);

                var markBGType = (housePrice / 1000) > (int)LocationMarkBGType.Black ? LocationMarkBGType.Black : (LocationMarkBGType)(housePrice / 1000);

                return new HouseInfo
                {
                    Money = screening_price,
                    HouseURL = "http://www.huzhumaifang.com" + locationInfo.GetAttribute("href"),
                    HouseLocation = location,
                    HouseTime = screening_time,
                    HousePrice = housePrice,
                    LocationMarkBG = markBGType.ToString() + ".png",

                };
            });
        }



    }
}
