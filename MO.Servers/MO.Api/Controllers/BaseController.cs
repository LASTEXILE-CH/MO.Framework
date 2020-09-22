﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MO.Algorithm.Redis;
using Newtonsoft.Json;
using ProtoMessage;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace MO.Api.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public abstract class BaseController : Controller
    {
        const string dataqueryname = "data";
        const string tokenqueryname = "token";
        const string useridqueryname = "userid";
        protected string data = string.Empty;
        protected string token = string.Empty;
        protected long userId = 0;

        [HttpGet]
        public abstract Task<string> GetMessage();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.HttpContext.Request.Query.ContainsKey(dataqueryname))
            {
                data = context.HttpContext.Request.Query[dataqueryname].ToString();
            }

            if (context.HttpContext.Request.Query.ContainsKey(tokenqueryname))
            {
                token = context.HttpContext.Request.Query[tokenqueryname].ToString();
            }

            if (context.HttpContext.Request.Query.ContainsKey(useridqueryname))
            {
                var struserid = context.HttpContext.Request.Query[useridqueryname].ToString();
                long.TryParse(struserid, out userId);
            }

            var redisToken = TokenRedis.Client.Get<string>(userId.ToString());
            if (string.IsNullOrEmpty(redisToken) ||
                string.IsNullOrEmpty(token) ||
                redisToken != token)
            {
                var result = new MOMsgResult();
                result.ErrorCode = 10001;
                result.ErrorInfo = "Token Error";
                context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(result), Encoding.UTF8);
                return;
            }
        }
    }
}