using System;

namespace Makelaars
{
    public class EnvironmentVariables
    {
        public static string API_KEY = Environment.GetEnvironmentVariable("API_KEY") ?? "ac1b0b1572524640a0ecc54de453ea9f";
        public static string API_URL = Environment.GetEnvironmentVariable("API_URL") ?? "http://partnerapi.funda.nl";
        public static int PAGE_SIZE = int.Parse(Environment.GetEnvironmentVariable("PAGE_SIZE") ?? "25");
    }
}
