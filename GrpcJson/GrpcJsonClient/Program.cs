// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;

Console.WriteLine("Hello, World!");

using var client = new HttpClient();

string getResp = await client.GetStringAsync("https://localhost:7197/greeter/Akos");
Console.WriteLine(getResp);

Console.WriteLine("--------------------------------");

var postResp = await client.PostAsJsonAsync("https://localhost:7197/greeter", new { name = "Akos", count = 3 });
string postRespString = await postResp.Content.ReadAsStringAsync();
Console.WriteLine(postRespString);

Console.ReadLine();
