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
        public override Version Version => new Version(1, 2, 0);

        /// <summary>
        /// The author(s) of the plugin.
        /// </summary>
        public override string Author => "brian91292, maintained by moisterrific";

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


        public static void Log(string msg,
                        [CallerMemberName] string member = "",
                        [CallerLineNumber] int line = 0)
        {
            Console.WriteLine($"AutoRegister::{member}({line}): {msg}");
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

        private Dictionary<string, string> tmpPasswords = new Dictionary<string, string>();
        /// <summary>
        /// Tell the player their password if the account was newly generated
        /// </summary>
        /// <param name="args"></param>
        void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            var player = TShock.Players[args.Who];
            
            if (tmpPasswords.TryGetValue(player.Name + player.UUID + player.IP, out string newPass))
            {
                try
                {
                    player.SendSuccessMessage($"Account \"{player.Name}\" has been registered.");
                    player.SendInfoMessage("Your password is " + newPass);
                }
                catch { }
                tmpPasswords.Remove(player.Name + player.UUID + player.IP);
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
                    tmpPasswords[player.Name + player.UUID + player.IP] =
                        Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10).Replace('l', 'L')
                            .Replace('1', '7').Replace('I', 'i').Replace('O', 'o').Replace('0', 'o');
                    TShock.UserAccounts.AddUserAccount(new UserAccount(
                        player.Name,
                        BCrypt.Net.BCrypt.HashPassword(tmpPasswords[player.Name + player.UUID + player.IP].Trim()),
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
