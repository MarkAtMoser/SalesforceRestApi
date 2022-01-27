using System;

namespace SalesforceDemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var client = CreateClient();
            client.login();
            var results = client.Query();
            // results.records.ForEach(c => Console.WriteLine($"Email: {c.Email}"));
            results.ForEach(c => Console.WriteLine($"Email: {c.Email}"));
            Console.WriteLine("Program Ends");
        }
        private static SalesforceClient CreateClient()
        {
            return new SalesforceClient
            {
                Username = InstanceDetailsSandbox.Username,
                Password = InstanceDetailsSandbox.Password,
                Token = InstanceDetailsSandbox.Token,
                ClientId = InstanceDetailsSandbox.ClientId,
                ClientSecret = InstanceDetailsSandbox.ClientSecret,
                loginEndpoint = InstanceDetailsSandbox.LoginEndpoint
            };
        }
    }
}
