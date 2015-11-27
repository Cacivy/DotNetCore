        /// <summary>
        /// 通过第三方 解析VIN 信息
        /// 2015-10-16
        /// </summary>
        /// <param name="vin"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetUrlVin(string vin)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            //VIN查询
            string vincode = vin.Trim().ToUpper().Replace(" ", "");
            if (string.IsNullOrEmpty(vincode) || vincode.Length != 17) return null;
            //发送请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.vin114.net/vin/carinfo.do?op=decodeVINByNewByVin&vin=" + vincode);
            request.Method = "get";
            request.Timeout = 1000000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2357.134 Safari/537.36";
            request.ContentType = "text/javascript";
            request.Headers.Set("Accept-Language", "zh-CN,zh;q=0.8");
            request.Headers.Set("Accept-Encoding", "gzip, deflate, sdch");
            //获取请求
            string html = "";
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.Default);

                html = sr.ReadToEnd().Trim();
            }
            catch (Exception)
            {
                return null;

            }
            string tbody;
            Regex reg = new Regex("(?<=(" + "<td align=\"center\" bgcolor=\"#DCEBF8\"><span class=\"t1\">" + "))[.\\s\\S]*?(?=(" + "</span></td>" + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            try
            {
                tbody = html.Substring(html.IndexOf("table"), html.LastIndexOf("table") - html.IndexOf("table"));
            }
            catch (Exception)
            {

                return null;
            }

            //获取数据
            string[] xarr = tbody.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            int valid = 0;
            foreach (var x in xarr)
            {
                string text = reg.Match(x).ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    if (valid % 2 == 0)
                    {
                        dic.Add(text, vincode);
                    }
                    else
                    {
                        List<string> list = new List<string>();
                        list.AddRange(dic.Keys);
                        dic[list[list.Count - 1]] = text;
                    }
                    valid++;
                }
                else
                {
                    if (reg.IsMatch(x))
                    {
                        List<string> list = new List<string>();
                        list.AddRange(dic.Keys);
                        dic[list[list.Count - 1]] = text;

                        valid++;
                    }
                }
            }
            //品牌、车型、生产年份、车辆代码、发动机型号、排量、进气形式、功率（Kw）、变速器类型、档位数、驱动形式、底盘号
            List<string> liststring = new List<string>() { "品牌", "车型", "生产年份", "车辆代码", "发动机型号", "排量", "进气形式", "功率(Kw)", "变速器类型", "档位数", "驱动形式", "底盘号" };
            Dictionary<string, object> dicresult = new Dictionary<string, object>();
            foreach (var item in dic)
            {
                if (liststring.Contains(item.Key))
                {
                    dicresult.Add(item.Key,item.Value);
                }
            }

            return dicresult.Count>0?dicresult:null;
        }
