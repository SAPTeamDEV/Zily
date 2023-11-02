using System;

using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilySide : Side
    {
        /// <inheritdoc/>
        public override string Protocol { get; } = "zily";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 0);

        /// <inheritdoc/>
        public override string Name { get; } = "Zily";

        /// <summary>
        /// Gets the proper flag for disconnect message.
        /// </summary>
        public virtual int DisconnectFlag => ZilyHeaderFlag.Disconnected;

        /// <summary>
        /// Gets the <see cref="ZilyStream"/> that interacts with this Side.
        /// </summary>
        protected ZilyStream Parent { get; private set; }

        /// <summary>
        /// Parses the given <see cref="ZilyHeader"/>.
        /// </summary>
        /// <param name="header">
        /// The header object.
        /// </param>
        public virtual void ParseHeader(ZilyHeader header)
        {
            Parent.logger.Debug("Parsing data with flag {flag} and message \"{text}\"", header.Flag, header.Text != null ? header.Text.Replace("\n", "") : null);

            switch (header.Flag)
            {
                case ZilyHeaderFlag.Ok:
                    if (Parent.LastRequest > 1)
                    {
                        ParseResponse(Parent.LastRequest, header.Text);
                        Parent.LastRequest = 0;
                    }
                    break;
                case ZilyHeaderFlag.Warn:
                    Parent.logger.Warning(header.Text);
                    break;
                case ZilyHeaderFlag.Fail:
                    Parent.logger.Error(header.Text);
                    break;
                case ZilyHeaderFlag.Connected:
                    Parent.IsOnline = true;
                    break;
                case ZilyHeaderFlag.Disconnected:
                    Parent.IsOnline = false;
                    break;
                case ZilyHeaderFlag.SideIdentifier:
                    Parent.WriteCommand(new ZilyHeader(ZilyHeaderFlag.Ok, GetIdentifier()));
                    break;
                case ZilyHeaderFlag.Write:
                    Console.Write(header.Text);
                    Ok();
                    break;
                default:
                    Parent.logger.Error("Flag is invalid: {flag}", header.Flag);
                    break;
            }
        }

        public virtual void ParseResponse(int lastRequestFlag, string responseText)
        {
            switch (lastRequestFlag)
            {
                case ZilyHeaderFlag.SideIdentifier:
                    var otherSide = Parse(responseText);
                    if (Protocol != otherSide.Protocol || Version.Major != otherSide.Version.Major)
                    {
                        Parent.logger.Fatal("Cannot connect to the server");
                        Parent.Close();
                    }
                    else
                    {
                        Parent.OtherSide = otherSide;
                    }
                    break;
            }
        }

        /// <summary>
        /// Set corresponding <see cref="ZilyStream"/> as parent object of this Side.
        /// </summary>
        /// <param name="parent">
        /// An instance of <see cref="ZilyStream"/>.
        /// </param>
        /// <exception cref="ArgumentException"></exception>
        public void SetParent(ZilyStream parent)
        {
            if (Parent == null)
            {
                Parent = parent;
            }
            else
            {
                throw new ArgumentException("The Parent property is already set.");
            }
        }

        /// <summary>
        /// Sends a Ok message.
        /// </summary>
        protected void Ok()
        {
            Parent.WriteCommand(ZilyHeader.Ok);
        }
    }
}