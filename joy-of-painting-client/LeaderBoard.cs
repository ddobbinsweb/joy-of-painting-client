using joy_of_painting_client.Game;
using joy_of_painting_client.Responses;
using Spectre.Console;

namespace joy_of_painting_client;

public static class LeaderBoard
{
    public static async Task ShowAsync()
    {
        var table = new Table().Centered();

        await AnsiConsole.Live(table)
              .AutoClear(false)   // Do not remove when done
               .Overflow(VerticalOverflow.Ellipsis) // Show ellipsis when overflowing
            .Cropping(VerticalOverflowCropping.Top) // Crop overflow at top
            .StartAsync(async ctx =>
            {
                string? userKey = GameSettings.GetUserKey();
                var leaderBoardClient = new BaseClient("leaderboard", userKey);
                var response = await leaderBoardClient.GetAllAsync<LeaderBoardResponse>("/general");

                if (response != null)
                {
                    table.AddColumn("Place");
                    table.AddColumn("Name");
                    table.AddColumn("score");
                    table.AddColumn("profile");
                    ctx.Refresh();
                    foreach (var item in response.Results)
                    {

                        table.AddRow(item.Order.ToString(), item.PainterName, item.Score.ToString(), $"[green] https://jop.revunit.com/profile/{item.PainterId} [/]");
                        ctx.Refresh();
                    }
                }
            });


        if (!AnsiConsole.Confirm("Go Back"))
        {
            await GameLayout.Reset();
            await ShowAsync();
        }
    }
}
