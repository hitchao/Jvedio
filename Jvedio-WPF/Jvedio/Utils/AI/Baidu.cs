using System;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Text;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Windows;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class AccessToken

    {
        private static string clientId;// 百度云中开通对应服务应用的 API Key 建议开通应用的时候多选服务
        private static string clientSecret; // 百度云中开通对应服务应用的 Secret Key

        public static void Init()
        {
            clientId = Properties.Settings.Default.Baidu_API_KEY.Replace(" ", "");
            clientSecret = Properties.Settings.Default.Baidu_SECRET_KEY.Replace(" ", "");
        }




        public static string getAccessToken()
        {
            Init();
            string authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<string, string>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            //Console.WriteLine(result);
            return GetValue(result, "access_token").Replace("\"", "");
        }

        public static string GetValue(string json, string key)
        {

            string result = ""; //解析失败的默认返回值
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            try
            {
                Dictionary<string, object> obj_json = serializer.DeserializeObject(json) as Dictionary<string, object>;
                if (obj_json.ContainsKey(key))
                {
                    result = serializer.Serialize(obj_json[key]);
                }
            }
            catch (Exception) { }

            return result;
        }

    }

    public static class FaceDetect
    {

        // 人脸检测与属性分析    https://cloud.baidu.com/doc/FACE/s/yk37c1u4t
        public static string faceDetect(string token, Bitmap bitmap, string imagepath = "")
        {
            //string token = "[调用鉴权接口获取的token]";
            string host = "https://aip.baidubce.com/rest/2.0/face/v3/detect?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.KeepAlive = true;
            string base64Str = ImageProcess.ImageToBase64(bitmap);
            if (base64Str == null) return "";
            //Console.WriteLine(base64Str);
            string str = "{\"image\":\"" + base64Str + "\",\"image_type\":\"BASE64\",\"face_field\":\"age,beauty,expression,face_shape,gender,glasses,landmark,landmark150,race,quality,eye_status,emotion,face_type,mask,spoofing\",\"max_face_num\":1,\"face_type\":\"LIVE\",\"liveness_control\":\"NONE\"}";
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string result = reader.ReadToEnd();
            //Console.WriteLine("人脸检测与属性分析:");
            //Console.WriteLine(result);
            return result;
        }

        public static Task<Int32Rect> GetAIResult(Movie movie, string path)
        {
            return Task.Run(() =>
            {
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(path);
                string token = AccessToken.getAccessToken();
                string FaceJson = FaceDetect.faceDetect(token, bitmap);

                Dictionary<string, string> result;
                Int32Rect int32Rect;
                (result, int32Rect) = FaceParse.Parse(FaceJson);
                if (result != null && int32Rect != Int32Rect.Empty)
                {
                    MySqlite dataBase = new MySqlite("AI");
                    dataBase.SaveBaiduAIByID(movie.id, result);
                    dataBase.CloseDB();
                    return int32Rect;
                }
                else
                    return Int32Rect.Empty;
            });

        }



    }


    public static class FaceParse
    {
        public static string FaceJson;


        public static (Dictionary<string, string>, Int32Rect) Parse(string json)
        {
            Dictionary<string, string> dicresult = null;
            Int32Rect int32Rect = Int32Rect.Empty;
            FaceJson = json;
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> dict = serializer.DeserializeObject(FaceJson) as Dictionary<string, object>;
            uint facenum = 0;
            if (dict.ContainsKey("result"))
            {
                Dictionary<string, object> result = dict["result"] as Dictionary<string, object>;
                try { if ((bool)result?.ContainsKey("face_num")) facenum = uint.Parse(result["face_num"].ToString()); } catch { }
                if (facenum > 0)
                {

                    object[] face_list_object = result["face_list"] as object[];
                    foreach (Dictionary<string, object> item in face_list_object)
                    {
                        var location = item["location"] as Dictionary<string, object>;
                        var age = item["age"].ToString();
                        var beauty = item["beauty"].ToString();
                        var expression = item["expression"] as Dictionary<string, object>;
                        var face_shape = item["face_shape"] as Dictionary<string, object>;
                        var gender = item["gender"] as Dictionary<string, object>;
                        var glasses = item["glasses"] as Dictionary<string, object>;
                        var race = item["race"] as Dictionary<string, object>;
                        var emotion = item["emotion"] as Dictionary<string, object>;
                        var mask = item["mask"] as Dictionary<string, object>;
                        dicresult = new Dictionary<string, string>();
                        dicresult.Add("age", age);
                        dicresult.Add("beauty", beauty);
                        dicresult.Add("expression", expression["type"].ToString());
                        dicresult.Add("face_shape", face_shape["type"].ToString());
                        dicresult.Add("gender", gender["type"].ToString());
                        dicresult.Add("glasses", glasses["type"].ToString());
                        dicresult.Add("race", race["type"].ToString());
                        dicresult.Add("emotion", emotion["type"].ToString());
                        dicresult.Add("mask", mask["type"].ToString());

                        int32Rect = new Int32Rect((int)float.Parse(location["left"].ToString()), (int)float.Parse(location["top"].ToString()), (int)float.Parse(location["width"].ToString()), (int)float.Parse(location["height"].ToString())); ;
                        return (dicresult, int32Rect);
                    }

                }
            }
            return (dicresult, int32Rect);


        }

        public static void ShowDic(Dictionary<string, object> dict)
        {
            foreach (KeyValuePair<string, object> keyValuePair in dict)
            {
                Console.WriteLine(keyValuePair.Key + "：" + keyValuePair.Value);
            }
        }
    }
}