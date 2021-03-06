#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
//
// Trx Framework - <http://www.trxframework.org/>
// Copyright (C) 2004-2012  Zabaleta Asociados SRL
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using System;
using System.Text;
using System.Threading;

namespace Trx.Logging
{
    public class Renderer : IRenderer
    {
        #region IRenderer Members
        public string Render(DateTime dateTime, LogLevel level, object message)
        {
            return DoRender(dateTime, level, message, null);
        }

        public string Render(DateTime dateTime, LogLevel level, object message, Exception cause)
        {
            return DoRender(dateTime, level, message, cause);
        }
        #endregion

        private static string DoRender(DateTime dateTime, LogLevel level, object message, Exception cause)
        {
            if (message == null && cause == null)
                return string.Empty;

            var sb = new StringBuilder(string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}.{6:000} [{7}] {8} - ",
                dateTime.Year, dateTime.Month, dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond,
                Thread.CurrentThread.ManagedThreadId,
                level.ToString().ToUpper()));

            if (message != null)
            {
                if (message is IRenderable)
                    ((IRenderable) message).Render(sb, string.Empty, "   ");
                else
                    sb.Append(message.ToString());
                sb.AppendLine();
            }

            if (cause != null)
            {
                sb.Append(cause.ToString());
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}