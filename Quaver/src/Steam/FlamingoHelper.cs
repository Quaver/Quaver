﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Net.Handlers;

namespace Quaver.Steam
{
    public static class FlamingoHelper
    {
        /// <summary>
        ///     Initializes the Flamingo event handlers so that we can execute
        ///     methods in Quaver when they are received from Flamingo.
        /// </summary>
        public static void InitializeFlamingoEventHandlers()
        {
            LoginReplyHandler.LoginReplyEvent += Login.OnLoginReplyEvent;
        }
    }
}
