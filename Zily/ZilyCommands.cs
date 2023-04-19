﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.Zily
{
    public partial class  ZilyStream
    {
        /// <summary>
        /// Sends the <see cref="HeaderFlag.Ok"/> response.
        /// </summary>
        protected void Ok()
        {
            WriteCommand(HeaderFlag.Ok);
        }

        /// <summary>
        /// Sends the <see cref="HeaderFlag.Fail"/> response with given message.
        /// </summary>
        /// <param name="message">
        /// The error message.
        /// </param>
        protected void Fail(string message)
        {
            logger.Error(message);
            WriteCommand(HeaderFlag.Fail, message);
        }

        /// <summary>
        /// Sends the <see cref="HeaderFlag.Warn"/> response with given message.
        /// </summary>
        /// <param name="message">
        /// The warning message.
        /// </param>
        protected void Warn(string message)
        {
            logger.Warning(message);
            WriteCommand(HeaderFlag.Warn, message);
        }

        /// <summary>
        /// Sends the <see cref="HeaderFlag.VersionInfo"/> response.
        /// </summary>
        protected void VersionInfo()
        {
            WriteCommand(HeaderFlag.VersionInfo, API.ToString());
        }

        /// <summary>
        /// Sends a <see cref="HeaderFlag.Write"/> request with given text.
        /// </summary>
        /// <param name="text">
        /// The text that would be wrote to the stream.
        /// </param>
        public void Write(string text)
        {
            WriteCommand(HeaderFlag.Write, text);
        }

        /// <summary>
        /// Sends a <see cref="HeaderFlag.Version"/> request.
        /// </summary>
        public void Version()
        {
            WriteCommand(HeaderFlag.Version);
        }
    }
}