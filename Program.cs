using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text;

class Program
{

    public static string Ascii = @"
  /$$$$$$  /$$                         /$$                          
 /$$__  $$| $$                        | $$                          
| $$  \ $$| $$  /$$$$$$  /$$  /$$  /$$| $$$$$$$   /$$$$$$   /$$$$$$ 
| $$$$$$$$| $$ |____  $$| $$ | $$ | $$| $$__  $$ /$$__  $$ /$$__  $$
| $$__  $$| $$  /$$$$$$$| $$ | $$ | $$| $$  \ $$| $$$$$$$$| $$  \__/
| $$  | $$| $$ /$$__  $$| $$ | $$ | $$| $$  | $$| $$_____/| $$      
| $$  | $$| $$|  $$$$$$$|  $$$$$/$$$$/| $$  | $$|  $$$$$$$| $$      
|__/  |__/|__/ \_______/ \_____/\___/ |__/  |__/ \_______/|__/      
 ";
    
    static async Task Main(string[] args)
    {

        string webhookUrl = "";
        Console.WriteLine(Ascii);

        while (true) 
        {
            Console.WriteLine("Please enter the URL of the webhook you want to check:");
            webhookUrl = Console.ReadLine();


            if (!webhookUrl.Contains("discord.com/api/webhooks/"))
            {
                Console.WriteLine("Invalid webhook URL. Please enter a valid Discord webhook URL.");
                
            }
            else
            {
                break;
            }
        }
        

        CancellationTokenSource cancellationTokenSource = null;

        var commands = new Dictionary<string, Func<Task>>()
    {
        { "check", () => CheckWebhookExists(webhookUrl) },
        { "stats", () => SendWebhookInfo(webhookUrl) },
        { "delete", async () => { await DeleteWebhook(webhookUrl); } },
        { "spam", async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => StartSpam(webhookUrl, cancellationTokenSource.Token));
            }
        },
        { "flood", async () =>
            {
                cancellationTokenSource = new CancellationTokenSource();
                await Task.Run(() => FloodSpam(webhookUrl, cancellationTokenSource.Token));
            }
        },
        // Ajouter des commandes supplémentaires ici
    };


        while (true)
        {
            Console.Clear();
            Console.WriteLine(Ascii);
            helps();
            Console.WriteLine("Please enter a command:");

            var command = Console.ReadLine();

            try
            {
                if (commands.TryGetValue(command, out var action))
                {
                    await action();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine(Ascii);
                    helps();
                    //Console.WriteLine($"Command '{command}' not recognized.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    static async Task SendMessageToWebhookAsync(string webhookUrll, string messagef)
    {
        var httpClient = new HttpClient();

        var json = $"{{\"content\":\"{messagef}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(webhookUrll, content);

        if (!response.IsSuccessStatusCode)
        {
           // Console.WriteLine($"Failed to send message to webhook. Response status code: {response.StatusCode}");
        }
        else
        {
            // Console.WriteLine($"Message sent successfully to webhook at {webhookUrl}");
        }
    }


    static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !#$%&'()*+,-./:;<=>?@[]^_`{|}~";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    static async Task CheckWebhookExists(string webhookUrl)
    {
        var client = new HttpClient();

        var response = await client.GetAsync(webhookUrl);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Webhook exists.");
        }
        else
        {
            Console.WriteLine("Webhook does not exist. Error code: " + response.StatusCode);
        }
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(Ascii);
        helps();
    }
    static async Task Login()
    {
        Console.WriteLine("Please enter the URL of the webhook you want to check:");
        var webhookUrl = Console.ReadLine();


        if (!webhookUrl.Contains("discord.com/api/webhooks/"))
        {
            Console.WriteLine("Invalid webhook URL. Please enter a valid Discord webhook URL.");
            return;
        }
        else
        {

        }
    }
    static async Task DeleteWebhook(string webhookUrl)
    {
        var client = new HttpClient();

        var response = await client.DeleteAsync(webhookUrl);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Webhook deleted successfully.");
        }
        else
        {
            Console.WriteLine("Failed to delete webhook. Error code: " + response.StatusCode);
        }
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(Ascii);
        helps();
    }

    static async Task StartSpam(string webhookUrl, CancellationToken cancellationToken)
    {
        Console.WriteLine("Please enter the message to send:");
        var message = Console.ReadLine();

        Console.WriteLine("Please enter the number of times to send the message (or 'infinite' to send indefinitely):");
        var countString = Console.ReadLine();

        int count;
        if (countString.ToLower() == "infinite".ToLower())
        {
            count = -1; // Use -1 to represent infinite
        }
        else
        {
            count = int.Parse(countString);
        }

        var client = new HttpClient();

        int i = 1;
        while (count == -1 || i <= count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payload = new
            {
                content = message
            };

            var response = await client.PostAsJsonAsync(webhookUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Message {i} sent successfully.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"Too many requests. Waiting for 30 seconds...");
                await Task.Delay(30000);
                continue;
            }
            else
            {
                Console.WriteLine($"Failed to send message {i}. Error code: " + response.StatusCode);
            }

            await Task.Delay(100);

            if (i % 10 == 0 && i != 0)
            {
                if(i >= count)
                {

                }
                else
                {
                    Console.WriteLine("Pausing for 5 seconds...");
                    await Task.Delay(5000);
                }
                
            }

            i++;
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(Ascii);
        helps();
    }
    static async Task FloodSpam(string webhookUrl, CancellationToken cancellationToken)
    {
        

        int countInt = 0;
        string message = "0";
        while (true)
        {
            Console.WriteLine("Please enter the number of carractere  max 2000:");
            var countRandom = Console.ReadLine();
            try
            {
                countInt = Convert.ToInt32(countRandom);
                if (countInt <= 2000 && countInt >= 0)
                {
                    int CountNumberString = countInt;
                    message = GenerateRandomString(CountNumberString);
                    break;
                }
            }
            catch
            {

            }

           
        }
        
        
        Console.WriteLine("Please enter the number of times to send the message (or 'infinite' to send indefinitely):");
        var countString = Console.ReadLine();

        int count;
        if (countString.ToLower() == "infinite".ToLower())
        {
            count = -1; // Use -1 to represent infinite
        }
        else
        {
            count = int.Parse(countString);
        }

        var client = new HttpClient();
        
        int i = 1;
        while (count == -1 || i <= count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var payload = new
            {
                content = message
            };

            var response = await client.PostAsJsonAsync(webhookUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Message {i} sent successfully.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"Too many requests. Waiting for 30 seconds...");
                await Task.Delay(30000);
                continue;
            }
            else
            {
                Console.WriteLine($"Failed to send message {i}. Error code: " + response.StatusCode);
            }

            await Task.Delay(100);

            if (i % 10 == 0 && i != 0)
            {
                Console.WriteLine("Pausing for 5 seconds...");
                await Task.Delay(5000);
            }

            i++;
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(Ascii);
        helps();
    }
    static async Task SendWebhookInfo(string webhookUrl)
    {
        var client = new HttpClient();

        var response = await client.GetAsync(webhookUrl);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("Failed to fetch webhook info. Error code: " + response.StatusCode);
            return;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var webhook = JsonConvert.DeserializeObject<Webhook>(responseContent);

        Console.WriteLine($"Webhook URL: {webhookUrl}");
        Console.WriteLine($"Webhook ID: {webhook.Id}");
        Console.WriteLine($"Webhook Name: {webhook.Name}");
        Console.WriteLine($"Webhook Avatar URL: {webhook.AvatarUrl}");
        Console.WriteLine($"Webhook Type: {(webhook.Type == WebhookType.Incoming ? "Incoming" : "Unknown")}");
        Console.WriteLine($"Webhook Channel ID: {webhook.ChannelId}");
        Console.WriteLine($"Webhook Guild ID: {webhook.GuildId}");
        Console.WriteLine($"Webhook Token: {webhook.Token}");

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.Clear();
        Console.WriteLine(Ascii);
        helps();
    }

    class Webhook
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public WebhookType Type { get; set; }
        public string ChannelId { get; set; }
        public string GuildId { get; set; }
        public string Token { get; set; }
    }

    enum WebhookType
    {
        Incoming,
        Unknown
    }
    
    static async Task helps()
    {
        Console.WriteLine(@"Command :

-check   Check the status of the webhook.
-stats   Get more information about the webhook.
-spam    Send spam messages to this webhook.
-flood   Send a large number of messages to this webhook.
-delete  Delete this webhook.
");
        

    }

    // Ajouter des méthodes pour d'autres commandes ici
}