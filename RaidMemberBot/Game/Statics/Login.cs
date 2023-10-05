using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using System;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Class about out of game related methods and fields
    /// </summary>
    public sealed class Login
    {
        private Login()
        {
        }

        /// <summary>
        ///     Access to the class
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Login Instance { get; } = new Login();

        /// <summary>
        ///     Number of avaible characters (returns 0 if not at the character selection screen)
        /// </summary>
        /// <value>
        ///     The number character count.
        /// </value>
        public int NumCharacterCount => Memory.Reader.Read<int>(
            Offsets.CharacterScreen.NumCharacters);

        /// <summary>
        ///     The login state
        /// </summary>
        /// <value>
        ///     The state of the login.
        /// </value>
        public LoginStates LoginState
            =>
                (LoginStates)
                Enum.Parse(typeof(LoginStates),
                    Offsets.CharacterScreen.LoginState.ReadString());

        /// <summary>
        ///     Get the message displayed by the messagebox which appears as soon as you hit login
        /// </summary>
        public string GlueDialogText
        {
            get
            {
                var result = Lua.Instance.ExecuteWithResult("{0} = GlueDialogText:GetText()");
                return result[0];
            }
        }

        /// <summary>
        ///     Resets the out of game state back to the start screen
        /// </summary>
        public void ResetLogin()
        {
            Lua.Instance.Execute("arg1 = 'ESCAPE' GlueDialog_OnKeyDown()");
            Lua.Instance.Execute(
                "if RealmListCancelButton ~= nil then if RealmListCancelButton:IsVisible() then RealmListCancelButton:Click(); end end GlueDialogText:SetText('')");
        }

        /// <summary>
        ///     Login with the saved credentials
        /// </summary>
        public void DefaultServerLogin(string accountName)
        {
            if (LoginState != LoginStates.login) return;
            Lua.Instance.Execute("DefaultServerLogin('" + accountName + "', 'password');");
        }

        /// <summary>
        ///     Enters the world.
        /// </summary>
        public void EnterWorld(int characterSlot)
        {
            if (!Wait.For2("AntiEnterWorldCrash", 500, true)) return;

            Functions.SelectCharacterAtIndex(characterSlot);
            Functions.EnterWorld();
        }
    }
}
