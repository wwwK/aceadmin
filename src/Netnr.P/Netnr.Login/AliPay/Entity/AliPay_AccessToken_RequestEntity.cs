using System;

namespace Netnr.Login
{
    /// <summary>
    /// Step2：Oauth2/access token
    /// </summary>
    public class AliPay_AccessToken_RequestEntity
    {
        /// <summary>
        /// 支付宝分配给开发者的应用ID
        /// </summary>
        [Required]
        public string app_id { get; set; } = AliPayConfig.AppId;

        /// <summary>
        /// 接口名称
        /// </summary>
        [Required]
        public string method { get; set; } = "alipay.system.oauth.token";

        /// <summary>
        /// 仅支持JSON
        /// </summary>
        public string format { get; set; } = "JSON";

        /// <summary>
        /// 请求使用的编码格式，如utf-8,gbk,gb2312等
        /// </summary>
        [Required]
        public string charset { get; set; } = "utf-8";

        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        [Required]
        public string sign_type { get; set; } = "RSA2";

        /// <summary>
        /// 商户请求参数的签名串
        /// </summary>
        [Required]
        public string sign { get; set; }

        /// <summary>
        /// 发送请求的时间，格式"yyyy-MM-dd HH:mm:ss"
        /// </summary>
        [Required]
        public string timestamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// 调用的接口版本，固定为：1.0
        /// </summary>
        [Required]
        public string version { get; set; } = "1.0";

        /// <summary>
        /// 值为authorization_code时，代表用code换取；值为refresh_token时，代表用refresh_token换取
        /// </summary>
        [Required]
        public string grant_type { get; set; } = "authorization_code";

        /// <summary>
        /// 授权码，用户对应用授权后得到。
        /// </summary>
        public string code { get; set; }
    }
}