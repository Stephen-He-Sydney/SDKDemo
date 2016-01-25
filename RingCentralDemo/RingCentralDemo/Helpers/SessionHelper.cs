using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RingCentralDemo.Helpers
{
    public static class SessionHelper
    {
        /// <summary>
        /// Read a specifed session
        /// </summary>
        public static object Get(string name)
        {
            if (HttpContext.Current.Session[name] != null)
            {
                return (object)HttpContext.Current.Session[name];
            }
            return null;
        }

        /// <summary>
        /// Save a specifed session
        /// </summary>
        public static void Set(string name, object value)
        {
            HttpContext.Current.Session.Add(name, value);
        }

        /// <summary>
        /// Remove a specifed session
        /// </summary>
        public static void Remove(string name)
        {
            if (HttpContext.Current.Session[name] != null)
            {
                HttpContext.Current.Session.Remove(name);
            }
        }

        /// <summary>
        /// Remove all sessions
        /// </summary>
        public static void RemoveAll()
        {
            HttpContext.Current.Session.RemoveAll();
        }
    }
}