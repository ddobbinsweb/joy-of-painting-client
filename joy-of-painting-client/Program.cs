// See https://aka.ms/new-console-template for more information
using joy_of_painting_client;
using joy_of_painting_client.Models;
using Newtonsoft.Json;

Console.WriteLine("Joy of Painting!");
// Get Paintings by Artist or By Category
Console.WriteLine("Lets get started, would you like to get paintings by Artist or by Category?");
Console.WriteLine("Artist : 0");
Console.WriteLine("Category: 1");

// todo make this an input
string key = "486c153f-e4f0-4657-8ac8-fe3850bb51ad";


var getPaitingOption = Console.ReadLine();
if (getPaitingOption == "0")
{
    var artistClient = new BaseClient<List<Artist>>("artist/search",key);
    // get artists
    var values = new Dictionary<string, string?>
  {
      { "name", null },
  };

    var content = JsonConvert.SerializeObject(values);
    var response = await artistClient.Post( values);

    Console.WriteLine(response);
}
else
{
    // get categories
}

// get all categories

// output the category options

// ask user for input of which category

// get paintings by category selected

// 

// Specifying a file path
string path = @"C:\Users\David Dobbins\Pictures\joy-of-painting\8.jpg";

var pixelator = new Pixelator();

var strokes = pixelator.PixelateImage(path);

var json = JsonConvert.SerializeObject(strokes);

var temp = json;