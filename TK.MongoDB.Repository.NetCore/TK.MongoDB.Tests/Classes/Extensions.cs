using System;

namespace TK.MongoDB.Tests.Classes
{
    public static class Extensions
    {
        public static string GetTimestamp(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmssffff");
        }
    }
}
