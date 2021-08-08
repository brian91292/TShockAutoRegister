using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;
using static TShockAPI.GetDataHandlers;
using System.Linq;
using System.Threading.Tasks;

namespace AutoRegister
{
    /// <summary>
    /// The main plugin class should always be decorated with an ApiVersion attribute. The current API Version is 1.25
    /// </summary>
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public override string Name => "AutoRegister";

        /// <summary>
        /// The version of the plugin in its current state.
        /// </summary>
        public override Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// The author(s) of the plugin.
        /// </summary>
        public override string Author => "brian91292 & moisterrific";

        /// <summary>
        /// A short, one-line, description of the plugin's purpose.
        /// </summary>
        public override string Description => "A TShock plugin to automatically register a user account for new players.";

        /// <summary>
        /// The plugin's constructor
        /// Set your plugin's order (optional) and any other constructor logic here
        /// </summary>
        public Plugin(Main game) : base(game)
        {
        }

        /// <summary>
        /// Performs plugin initialization logic.
        /// Add your hooks, config file read/writes, etc here
        /// </summary>
        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer, 420);
        }

        private readonly Dictionary<string, string> tmpPasswords = new Dictionary<string, string>();

        private static readonly string result = GenerateRandomAlphanumericString();

        /// <summary>
        /// Tell the player their password if the account was newly generated
        /// </summary>
        /// <param name="args"></param>
        async void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            await Task.Delay(1000);
            if (tmpPasswords.TryGetValue(result, out string newPass))
            {
                try
                {
                    player.SendSuccessMessage($"Account \"{player.Name}\" has been registered.");
                    player.SendInfoMessage("Your password is " + newPass);
                }
                catch { }
                tmpPasswords.Remove(result);
            }
            else if (!player.IsLoggedIn)
            {
                player.SendErrorMessage("Sorry, " + player.Name + " was already taken by another person.");
                player.SendErrorMessage("Please try a different username.");
            }
        }

        /// <summary>
        /// Fired when a new user joins the server.
        /// </summary>
        /// <param name="args"></param>
        void OnServerJoin(JoinEventArgs args)
        {
            if (TShock.Config.Settings.RequireLogin)
            {
                var player = TShock.Players[args.Who];

                if (TShock.UserAccounts.GetUserAccountByName(player.Name) == null && player.Name != TSServerPlayer.AccountName)
                {
                    tmpPasswords[result] =
                        Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10).Replace('l', 'L')
                            .Replace('1', '7').Replace('I', 'i').Replace('O', 'o').Replace('0', 'o');
                    TShock.UserAccounts.AddUserAccount(new UserAccount(
                        player.Name,
                        BCrypt.Net.BCrypt.HashPassword(tmpPasswords[result].Trim(), TShock.Config.Settings.BCryptWorkFactor),
                        player.UUID,
                        TShock.Config.Settings.DefaultRegistrationGroupName,
                        DateTime.UtcNow.ToString("s"),
                        DateTime.UtcNow.ToString("s"),
                        ""));

                    TShock.Log.ConsoleInfo(player.Name + $" registered an account: \"{player.Name}\"");
                }
            }
        }

        /// <summary>
        /// Generates a random alphanumeric string.
        /// </summary>
        /// <param name="length">The desired length of the string</param>
        /// <returns>The string which has been generated</returns>
        public static string GenerateRandomAlphanumericString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var random = new Random();
            var randomString = new string(Enumerable.Repeat(chars, length)
                                                    .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }

        /// <summary>
        /// Performs plugin cleanup logic
        /// Remove your hooks and perform general cleanup here
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerJoin.Deregister(this, OnServerJoin);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            }
            base.Dispose(disposing);
        }
    }
}
