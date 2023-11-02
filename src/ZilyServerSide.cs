using System;
using System.IO;

using Serilog.Core;

namespace SAPTeam.Zily
{
    /// <summary>
    /// Provides properties to identify sides and establishes a zily connection.
    /// </summary>
    public class ZilyServerSide : ZilySide
    {
        /// <summary>
        /// Waits until a client connects to the server and then establishes the Zily connection.
        /// </summary>
        public void Accept()
        {
            bool everConnected = false;

            Parent.logger.Debug("Waiting for client");
            Wait();

            while (true)
            {
                var header = ZilyHeader.Parse(Parent.Stream);
                if (!everConnected)
                {
                    Parent.logger.Information("A new client connected to the pipe server");
                    Parent.logger.Information("Establishing a Zily connection");
                    everConnected = true;
                }

                ParseHeader(header);

                if (header.Flag == ZilyHeaderFlag.Connected)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Waits before starting client acceptation.
        /// </summary>
        public virtual void Wait()
        {

        }
    }
}