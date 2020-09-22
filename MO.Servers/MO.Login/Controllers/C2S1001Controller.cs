﻿using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using MO.Algorithm.Enum;
using ProtoMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MO.Login.Controllers
{
    public class C2S1001Controller : BaseController
    {
        public override Task<string> GetMessage()
        {
            var param = C2S_1001.Parser.ParseFrom(ByteString.FromBase64(data));

            string versionTotal = "1.0.0";
            string versionPatch = "1.0";

            switch ((DeviceType)param.MobileType)
            {
                case DeviceType.Android:
                    versionTotal = "1.0.0";
                    versionPatch = "1.0";
                    break;
                case DeviceType.iPhone:
                case DeviceType.iPad:
                case DeviceType.iPod:
                case DeviceType.Mac:
                    versionTotal = "1.0.0";
                    versionPatch = "1.0";
                    break;
                case DeviceType.Unknow:
                    versionTotal = "1.0.0";
                    versionPatch = "1.0";
                    break;
            }

            S2C_1001 message = new S2C_1001();
            message.VersionTotal = versionTotal;
            message.VersionPatch = versionPatch;
            message.IsAppStorePass = false;//0是在审核,1是没审核
            message.FirUrl = "";
            message.ApkUrl = "";//不打开网页下载apk地址
            message.Doname = "";//热更新域名
            message.FixIp = "";//热更新IP

            return Task.FromResult(new MOMsgResult() { Content = message.ToByteString() }.ToByteString().ToBase64());
        }
    }
}