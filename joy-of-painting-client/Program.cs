// See https://aka.ms/new-console-template for more information
using joy_of_painting_client;
using joy_of_painting_client.Models;
using joy_of_painting_client.Responses;
using Newtonsoft.Json;
using System.Net;
using System;
using System.Runtime.CompilerServices;

Console.WriteLine("Joy of Painting!");
// Get Paintings by Artist or By Category
Console.WriteLine("Lets get started, would you like to get paintings by Artist or by Category?");
Console.Write("Select Artist( 0 ) or Category( 1 ):");


// todo make this an input
string key = "486c153f-e4f0-4657-8ac8-fe3850bb51ad";
var pixelator = new Pixelator();

var getPaitingOption = Console.ReadLine();
if (getPaitingOption == "0")
{
    var artistClient = new BaseClient<ListResponse<Artist>>("artist/search", key);
    // get artists
    var values = new Dictionary<string, string?>
    {
      { "name", null },
    };


    var response = await artistClient.Post(values);
    Console.WriteLine("|Id|Name             |# of Paintings|");
    Console.WriteLine("--------------------------------------");
    foreach (var item in response.Items.OrderBy(x => x.Id))
    {
        string artistTable = String.Format("|{0,2}|{1,5}|{2,2}|", item.Id, item.Name, item.Paintings.Count);
        Console.WriteLine(artistTable);
    }
    Console.Write("select artist id:");
    var artistIdString = Console.ReadLine();
    if (!string.IsNullOrEmpty(artistIdString) && artistIdString != "0")
    {
        var artistId = Int32.Parse(artistIdString);
        // get artist paintings
        var artist = response.Items.Find(x => x.Id == artistId);
        if (artist != null)
        {
            Console.WriteLine($"Name: {artist.Name}");
            Console.WriteLine($"Photo: {artist.Url}");
            foreach (var painting in artist.Paintings)
            {
                Console.WriteLine($"painting Id: {painting.Id}");
                Console.WriteLine($"painting name: {painting.Name}");
                Console.WriteLine($"painting url: {painting.Url}");
            }
        }
        Console.Write("Select Painting Id: ");
        var paintingIdString = Console.ReadLine();
        if (!string.IsNullOrEmpty(paintingIdString) && paintingIdString != "0")
        {
            var paintingId = Int32.Parse(paintingIdString);
            var painting = artist?.Paintings.Find(x => x.Id == paintingId);

            if (painting != null)
            {
                await Helper.DownloadImageAsync($@"C:\Users\David Dobbins\Pictures\joy-of-painting\", painting.Id.ToString(), new Uri(painting.Url));

                var strokes = pixelator.PixelateImage($@"C:\Users\David Dobbins\Pictures\joy-of-painting\{painting.Id}.jpg");

                var pixelation = new Pixelation()
                {
                    Brushstrokes = strokes,
                    PaintingId = paintingId
                };

                // upload pixalation
                var pixelationClient = new BaseClient<PixelationResponse>("pixelation", key);
                var pixelationResponse = await pixelationClient.Post(pixelation);
                if (pixelationResponse != null)
                {
                    var pixelationJson = JsonConvert.SerializeObject(pixelationResponse);
                    Console.WriteLine(pixelationJson);
                }
                Console.ReadLine();
            }
        }
    }
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
