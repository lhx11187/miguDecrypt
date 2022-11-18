using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
/// <summary>
/// 32位MD5加密
/// </summary>
/// <param name="password"></param>
/// <returns></returns>
static string MD5Encrypt32(string password)
{
    MD5 md5 = new MD5CryptoServiceProvider();
    //byte[] fromData = System.Text.Encoding.Unicode.GetBytes(myString);
    byte[] fromData = System.Text.Encoding.UTF8.GetBytes(password);//
    byte[] targetData = md5.ComputeHash(fromData);
    string byte2String = null;

    for (int i = 0; i < targetData.Length; i++)
    {
        //这个是很常见的错误，你字节转换成字符串的时候要保证是2位宽度啊，某个字节为0转换成字符串的时候必须是00的，否则就会丢失位数啊。不仅是0，1～9也一样。
        //byte2String += targetData[i].ToString("x");//这个会丢失
        byte2String = byte2String + targetData[i].ToString("X2");
    }

    return byte2String;
}

static string MD5Encrypt64(string password)
{
    string cl = password;
    //string pwd = "";
    MD5 md5 = MD5.Create(); //实例化一个md5对像
    // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
    byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
    return Convert.ToBase64String(s);
}
string filePath = "";
string file_key = "";
string song_id = "0";
string outPath = "";

string lrcUrl = "";
if (args.Length > 0)
{
    for(int i=0;i<args.Length;i++)
    {
        //Console.WriteLine(arg); 
        if (args[i] == "-h")
        {
            Console.Write(@"
  -d string
        song id
  -i string
        input filename
  -k string
        file key, length = 32
  -o string
        output filename");
            return;
        }
        if (args[i] == "-d")
        {
            song_id = args[i + 1].Trim();
        }
        if (args[i] == "-i")
        {
            filePath = args[i + 1].Trim();
        }
        if (args[i] == "-k")
        {
            file_key = args[i + 1].Trim();
        }
        if (args[i] == "-o")
        {
            outPath = args[i + 1].Trim();
        }
    }
}

if(!string.IsNullOrEmpty(song_id))
{
    var httpClient=new HttpClient();
    var response=await httpClient.GetAsync($"https://app.c.nf.migu.cn/MIGUM2.0/v2.0/content/querySongBySongId.do?songId={song_id}&contentId=0");
    response.EnsureSuccessStatusCode();
    var statusCode=response.StatusCode;
    var headers=response.Headers;

    var str=await response.Content.ReadAsStringAsync();
    JObject _JObject = JObject.Parse(str);
    string songName = _JObject["resource"][0]["songName"].ToString();
    string singer = _JObject["resource"][0]["singer"].ToString();
    lrcUrl=_JObject["resource"][0]["lrcUrl"].ToString();
    file_key =_JObject["resource"][0]["z3dCode"]["androidFileKey"].ToString();
}

string key_str = MD5Encrypt32("AC89EC47A70B76F307CB39A0D74BCCB0" + file_key); 
Console.WriteLine(key_str);

char[] key = key_str.ToArray();



byte[] bytes= File.ReadAllBytes(filePath);
for (int i = 0; i < bytes.Length; i++)
{
    bytes[i] -= ((byte)key[i % 32]);
}
if (string.IsNullOrEmpty(outPath))
{
    File.WriteAllBytes(filePath.Replace(".mg3d", ".解密.wav"), bytes);
    if (!string.IsNullOrEmpty(lrcUrl))
    {
        WebClient webClient = new WebClient();
        webClient.Encoding = Encoding.UTF8;

        string outText = webClient.DownloadString(lrcUrl);
        File.WriteAllText(filePath.Replace(".mg3d", ".解密.lrc"), outText);
    }
}
else
{
    File.WriteAllBytes(outPath, bytes);
    if (!string.IsNullOrEmpty(lrcUrl))
    {
        WebClient webClient = new WebClient();
        webClient.Encoding = Encoding.UTF8;

        string outText = webClient.DownloadString(lrcUrl);
        File.WriteAllText(filePath.Replace(".mg3d", ".解密.lrc"), outText);
    }
}
